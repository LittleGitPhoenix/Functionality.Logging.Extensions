#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

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
	
	private readonly Dictionary<object, int> _scopes;
	
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
		_scopes = new();
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
		var scopes = scope is ILogScope logScope && logScope.Type == LogScopeType.ExecutionContextAware ? _executionContextAwareScopes.Value ??= [] : _scopes;
		
        // Only add unique items.		
		if (!scopes.ContainsKey(scope)) scopes.Add(scope, Interlocked.Increment(ref _scopeOrder));

		// Return disposable that will remove the scope.
		return new DisposableAction
		(
			() =>
            {
                try
                {
                    if (scopes.ContainsKey(scope)) scopes.Remove(scope);
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

	sealed class DisposableAction : IDisposable
    {
        #region Delegates / Events
        #endregion

        #region Constants
        #endregion

        #region Fields

        private readonly Action _dispose;

        #endregion

        #region Properties

        public static IDisposable NoDisposableAction { get; } = new DisposableAction(() => { });

        #endregion

        #region (De)Constructors

        public DisposableAction(Action dispose)
        {
            _dispose = dispose;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                _dispose.Invoke();
            }
            catch (Exception) { /* ignore */ }
        }

        #endregion
    }

    #endregion
}