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
	/// <typeparam name="TState"> The type of the scope. </typeparam>
	/// <param name="scope"> The scope to add. </param>
	/// <returns> An <see cref="IDisposable"/> that will remove the scope when it is disposed. </returns>
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
/// This class supports both execution context-aware scopes (which flow with async operations) and global scopes.
/// Scopes are ordered to ensure correct nesting when logging. Thread safety is provided for concurrent operations.
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

	// This does not need to be thread-safe as ExecutionContextAware scopes are only accessible within the execution context they were created in and therefore concurrent access is not possible.
	private readonly AsyncLocal<Dictionary<object, int>> _executionContextAwareScopes;

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
		_scopes = new();  // ConcurrentDictionary — thread-safe for Independent scopes accessed from multiple execution contexts.
		_executionContextAwareScopes = new();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public IDisposable AddScope<TState>(TState scope) where TState : notnull
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (scope is null) return DisposableAction.NoDisposableAction;

		// Determine which collection to use.
		IDictionary<object, int> scopes = scope is ILogScope logScope && logScope.Type == LogScopeType.ExecutionContextAware ? _executionContextAwareScopes.Value ??= [] : _scopes;
		
		// Only add unique items.
		if (scopes is ConcurrentDictionary<object, int> concurrentScopes)
			concurrentScopes.TryAdd(scope, Interlocked.Increment(ref _scopeOrder));
		else if (!scopes.ContainsKey(scope))
			scopes.Add(scope, Interlocked.Increment(ref _scopeOrder));

		// Return disposable that will remove the scope.
		return new DisposableAction
		(
			() =>
			{
				try
				{
					if (scopes is ConcurrentDictionary<object, int> cd) cd.TryRemove(scope, out _);
					else scopes.Remove(scope);
				}
                catch (Exception) { /* ignore */ }
            }
        );
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
        #region Delegates / Events
        #endregion

        #region Constants
        #endregion

        #region Fields

		#endregion

        #region Properties

        public static IDisposable NoDisposableAction { get; } = new DisposableAction(() => { });

        #endregion
		
        #region Methods

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                dispose.Invoke();
            }
            catch (Exception) { /* ignore */ }
        }

        #endregion
    }

    #endregion
}