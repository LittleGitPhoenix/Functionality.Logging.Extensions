#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections.Concurrent;

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Defines a contract for managing logging scopes within a logging context.
/// </summary>
public interface ILogScopeManager
{
	/// <summary>
	/// Adds the <paramref name="scope"/> to the internal collection.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>ExecutionContextAware</b> scopes use copy-on-write semantics.
	/// Each call creates a new <see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/> that inherits all currently-visible entries and then assigns it to <see cref="System.Threading.AsyncLocal{T}.Value"/>.
	/// This assignment only modifies the current execution-context slot; parent and sibling contexts retain their own snapshots.
	/// </para>
	/// <para>
	/// <b>Known limitation:</b> if a child method adds its scope <em>before</em> its first real suspension point (i.e. before an actual <c>Task.Run</c> or a truly yielding <c>await</c>),
	/// both caller and callee share the same execution context at that point. The assignment therefore overwrites the caller's slot. When the scope is later disposed (often on a different
	/// thread-pool thread due to <c>ConfigureAwait(false)</c>) the restore applies to that continuation's context, not to the original caller's slot, leaving the caller permanently carrying the child's scope.
	/// This is a structural .NET limitation. The recommended mitigation is to give each class its own <c>ILogger</c> instance and group them via <c>ILoggerGroup</c>.
	/// </para>
	/// </remarks>
	/// <typeparam name="TState"> The type of the scope. </typeparam>
	/// <param name="scope"> The scope to add. </param>
	/// <returns>
	/// An <see cref="IDisposable"/> that removes or restores the scope when disposed.
	/// <list type="bullet">
	/// <item><description>
	/// For <see cref="LogScopeType.ExecutionContextAware"/> scopes (and non-<see cref="ILogScope"/> values): disposing
	/// restores the <see cref="System.Threading.AsyncLocal{T}"/> slot to the snapshot that existed before this scope
	/// was added. The restore applies to whichever execution context <c>Dispose</c> is called on.
	/// </description></item>
	/// <item><description>
	/// For <see cref="LogScopeType.Independent"/> scopes: disposing removes the scope from the shared global collection.
	/// </description></item>
	/// </list>
	/// </returns>
	IDisposable AddScope<TState>(TState scope) where TState : notnull;

	/// <summary>
	/// Retrieves the values of all active logging scopes in the order they were applied.
	/// </summary>
	/// <returns> An enumerable collection of objects representing the values of the current logging scopes, in FIFO order. </returns>
	IEnumerable<object> GetScopeValues();
}

/// <summary>
/// Manages the collection of active log scopes. Provides functionality to add and retrieve scope values, supporting both execution context-aware and global scopes.
/// </summary>
/// <remarks>
/// <para>
/// Scopes are categorised by their <see cref="LogScopeType"/> and stored accordingly:
/// </para>
/// <list type="bullet">
/// <item><description>
/// <b><see cref="LogScopeType.Independent"/></b> — stored in a single shared <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// Every log event emitted through any execution context sees these scopes. Suitable for static, lifetime-long metadata
/// such as service name, version, or environment.
/// </description></item>
/// <item><description>
/// <b><see cref="LogScopeType.ExecutionContextAware"/></b> (and non-<see cref="ILogScope"/> values) — stored using copy-on-write <see cref="System.Threading.AsyncLocal{T}"/> semantics.
/// Each <see cref="AddScope{TState}"/> call creates a new dictionary snapshot and assigns it to the current execution-context slot only. Parent and sibling contexts are unaffected.
/// Suitable for per-request or per-operation data such as a trace identifier.
/// </description></item>
/// </list>
/// <para>
/// <b>Choosing a <see cref="LogScopeType"/>:</b>
/// </para>
/// <list type="bullet">
/// <item><description>
/// Scope that belongs to <em>this execution</em> (a request, an operation, a unit of work) →
/// <see cref="LogScopeType.ExecutionContextAware"/> / <see cref="LogScope.CreateAware(System.Collections.Generic.IDictionary{string,object?})"/>.
/// </description></item>
/// <item><description>
/// Scope that belongs to <em>this logger forever</em> (service name, version, environment) →
/// <see cref="LogScopeType.Independent"/> / <see cref="LogScope.CreateIndependent(System.Collections.Generic.IDictionary{string,object?})"/>.
/// </description></item>
/// </list>
/// <para>
/// <b>Known limitation:</b> execution-context isolation only takes effect once the .NET runtime creates a new execution context (e.g. via <c>Task.Run</c> or a truly yielding <c>await</c>).
/// Scope added in a child method before its first suspension point modifies the caller's slot directly and may not be correctly restored. See <see cref="AddScope{TState}"/> for a full explanation.
/// </para>
/// </remarks>
public class LogScopeManager : ILogScopeManager
{
    #region Delegates / Events
    #endregion

    #region Constants
	#endregion

	#region Fields

	private int _scopeOrder;

	// This must be thread-safe as Independent scopes are deliberately shared across all execution contexts and threads and therefore concurrent access is highly probable.
	private readonly ConcurrentDictionary<object, int> _scopes;

	// Uses copy-on-write semantics:
	// Each AddScope call captures the current Value, creates a fresh Dictionary that inherits all existing entries, then assigns the new instance to AsyncLocal.Value.
	// This assignment is per-slot. It only modifies the current execution context and never touches parent or sibling contexts.
	// The dispose closure restores the previous snapshot, effectively popping the scope like a stack.
	//
	// Known limitation:
	// If a child method adds its scope BEFORE its first real suspension point (i.e. before an actual Task.Run or the first yielding await),
	// it runs on the same thread and in the same execution context as the caller. The assignment therefore overwrites the caller's slot directly.
	// The restore-on-dispose will execute on whatever thread the child's continuation is scheduled on (often a thread-pool thread when ConfigureAwait(false) is used),
	// so the caller's slot is never restored. This is a structural .NET limitation: the logger cannot control when or whether a new execution context is created.
	// The recommended mitigation is to give each class its own ILogger instance and group them via ILoggerGroup.
	private readonly AsyncLocal<Dictionary<object, int>?> _executionContextAwareScopes;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	public LogScopeManager()
    {
		// Save parameters.

		// Initialize fields.
		_scopes = new();  // ConcurrentDictionary - thread-safe for Independent scopes accessed from multiple execution contexts.
		_executionContextAwareScopes = new();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public IDisposable AddScope<TState>(TState scope) where TState : notnull
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		if (scope is null) return DisposableAction.NoDisposableAction;

		// Determine whether this scope is Independent (global ConcurrentDictionary) or ExecutionContextAware (AsyncLocal copy-on-write).
		var isExecutionContextAware = scope is not ILogScope logScope || logScope.Type == LogScopeType.ExecutionContextAware;

		if (isExecutionContextAware)
		{
			// Copy-on-write:
			// Capture the current slot value as the restore point, then build a fresh dictionary that inherits all existing entries.
			// Assigning a new instance to AsyncLocal.Value only modifies the current execution-context slot.
			// Parent and sibling contexts retain their own snapshots.
			var previous = _executionContextAwareScopes.Value;
			var newDict = previous is null
				? new Dictionary<object, int>()
				: new Dictionary<object, int>(previous)
				;

			// Only add unique items.
			if (!newDict.ContainsKey(scope)) newDict.Add(scope, Interlocked.Increment(ref _scopeOrder));

			// Assign the new dictionary to the current execution-context slot.
			_executionContextAwareScopes.Value = newDict;

			// Return a disposable that restores the previous snapshot in whatever execution context Dispose is called on.
			return new DisposableAction
			(
				() =>
				{
					try { _executionContextAwareScopes.Value = previous; }
					catch (Exception) { /* ignore */ }
				}
			);
		}
		else
		{
			// Independent scopes use a single shared ConcurrentDictionary that is visible across all execution contexts.
			_scopes.TryAdd(scope, Interlocked.Increment(ref _scopeOrder));

			return new DisposableAction
			(
				() =>
				{
					try { _scopes.TryRemove(scope, out _); }
					catch (Exception) { /* ignore */ }
				}
			);
		}
	}

	/// <inheritdoc />
	public IEnumerable<object> GetScopeValues()
	{
		// Concat both scope collections and order by value to maintain correct order.
		return (_executionContextAwareScopes.Value ?? [])
			.Concat(_scopes)
			.OrderBy(pair => pair.Value)
			.Select(pair => pair.Key)
			;
	}

	#endregion

	#region Nested Types

	private sealed class DisposableAction(Action dispose) : IDisposable
	{
        public static IDisposable NoDisposableAction { get; } = new DisposableAction(() => { });
		
        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                dispose.Invoke();
            }
            catch (Exception) { /* ignore */ }
        }
    }

    #endregion
}