//#region LICENSE NOTICE
////! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
//#endregion

//namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

///// <summary>
///// Defines a contract for managing logging scopes within a logging context.
///// </summary>
//public interface ILogScopeManager
//{
//	/// <summary>
//	/// Adds the <paramref name="scope"/> to the internal collection.
//	/// </summary>
//	/// <typeparam name="TState"> The type of the scope. </typeparam>
//	/// <param name="scope"> The scope to add. </param>
//	/// <returns> An <see cref="IDisposable"/> that will remove the scope when it is disposed. </returns>
//	IDisposable AddScope<TState>(TState scope) where TState : notnull;

//	/// <summary>
//	/// Retrieves the values of all active logging scopes in the order they were applied.
//	/// </summary>
//	/// <returns> An enumerable collection of objects representing the values of the current logging scopes, in FIFO order. </returns>
//	internal IEnumerable<object> GetScopeValues();
//}

/////// <summary>
/////// Collection of log scopes used by the <see cref="FrameworkLogger"/>.
/////// </summary>
/////// <remarks>
/////// <para> Based on: </para>
/////// <para> • https://github.com/serilog/serilog-extensions-logging/blob/v3.1.0/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerProvider.cs (v3.1.0) </para>
/////// <para> • https://github.com/serilog/serilog-extensions-logging/blob/v3.1.0/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerScope.cs (v3.1.0) </para>
/////// </remarks>

///// <summary>
///// Manages the collection of active log scopes. Provides functionality to add and retrieve scope values, supporting both execution context-aware and global scopes.
///// </summary>
///// <remarks>
///// This class supports both execution context-aware scopes (which flow with async operations) and global scopes.
///// Scopes are ordered to ensure correct nesting when logging. Thread safety is provided for concurrent operations.
///// </remarks>
//class LogScopeManager : ILogScopeManager
//{
//    #region Delegates / Events
//    #endregion

//    #region Constants

//    //private const string NoName = "None";

//	#endregion

//	#region Fields

//	private int _scopeOrder;
	
//	private readonly Dictionary<object, int> _scopes;
	
//	private readonly AsyncLocal<Dictionary<object, int>> _executionContextAwareScopes;

//	#endregion

//	#region Properties
//	#endregion

//	#region (De)Constructors

//	/// <summary>
//	/// Constructor
//	/// </summary>
//	public LogScopeManager()
//    {
//		// Save parameters.

//		// Initialize fields.
//		_scopes = new();
//		_executionContextAwareScopes = new();
//	}

//	#endregion

//	#region Methods

//	/// <inheritdoc />
//	public IDisposable AddScope<TState>(TState scope) where TState : notnull
//    {
//        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
//        if (scope is null) return DisposableAction.NoDisposableAction;

//		// Determine which collection to use.
//		var scopes = scope is Phoenix.Functionality.Logging.Base.IExecutionContextAwareLogScope ? _executionContextAwareScopes.Value ??= [] : _scopes;
		
//        // Only add unique items.		
//		if (!scopes.ContainsKey(scope)) scopes.Add(scope, Interlocked.Increment(ref _scopeOrder));

//		// Return disposable that will remove the scope.
//		return new DisposableAction
//		(
//			() =>
//            {
//                try
//                {
//                    if (scopes.ContainsKey(scope)) scopes.Remove(scope);
//                }
//                catch (Exception) { /* ignore */ }
//            }
//        );
//    }

//	///// <summary>
//	///// Converts all internal scopes into <see cref="LogEventPropertyValue"/>s to be used by <see cref="Serilog"/>.
//	///// </summary>
//	///// <returns> The <see cref="LogEventPropertyValue"/>s of this scope. </returns>
//	//internal IEnumerable<LogEventPropertyValue> CreateLogEventPropertyValues(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
//	//{
//	//	var scopes = this.GetScopeValues();
//	//	foreach (var scope in scopes)
//	//	{			
//	//		EnrichWithStateAndCreateScopeItem(logEvent, propertyFactory, scope, update: false, out var scopeItem);
//	//		if (scopeItem is null) continue;
//	//		yield return scopeItem;
//	//	}
//	//}

//	/// <inheritdoc />
//	public IEnumerable<object> GetScopeValues()
//	{
//		// Concat both scope collections and order by value to maintain correct order.
//		return (_executionContextAwareScopes.Value ?? [])
//			.Concat(_scopes)
//			.OrderBy(pair => pair.Value)
//			.Select(pair => pair.Key)
//			;
//	}

////	/// <remarks>
////	/// Adapted from https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerScope.cs → <b>EnrichWithStateAndCreateScopeItem</b>
////	/// <para>
////	/// Changes include:
////	/// <list type="bullet">
////	/// <item><description> <see cref="FrameworkLogger.OriginalFormatPropertyName"/> from <see cref="FrameworkLogger"/> class is used. </description></item>
////	/// <item><description> Conditional compilation for <b>FEATURE_ITUPLE</b> has been changed to <b>NETSTANDARD2_1 || NET8_0_OR_GREATER</b>. </description></item>
////	/// </list>
////	/// </para>
////	/// </remarks>
////	public static void EnrichWithStateAndCreateScopeItem(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, object? state, bool update, out LogEventPropertyValue? scopeItem)
////	{
////		if (state == null)
////		{
////			scopeItem = null;
////			return;
////		}

////		// Eliminates boxing of Dictionary<TKey, TValue>.Enumerator for the most common use case
////		if (state is Dictionary<string, object> dictionary)
////		{
////			// Separate handling of this case eliminates boxing of Dictionary<TKey, TValue>.Enumerator.
////			scopeItem = null;
////			foreach (var stateProperty in dictionary)
////			{
////				AddProperty(logEvent, propertyFactory, stateProperty.Key, stateProperty.Value, update);
////			}
////		}
////		else if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
////		{
////			scopeItem = null;
////			foreach (var stateProperty in stateProperties)
////			{
////				if (stateProperty is { Key: FrameworkLogger.OriginalFormatPropertyName, Value: string })
////				{
////					// `_state` is most likely `FormattedLogValues` (a MEL internal type).
////					scopeItem = new ScalarValue(state.ToString());
////				}
////				else
////				{
////					AddProperty(logEvent, propertyFactory, stateProperty.Key, stateProperty.Value, update);
////				}
////			}
////		}
////#if NETSTANDARD2_1 || NET8_0_OR_GREATER
////		else if (state is System.Runtime.CompilerServices.ITuple tuple && tuple.Length == 2 && tuple[0] is string s)
////        {
////            scopeItem = null; // Unless it's `FormattedLogValues`, these are treated as property bags rather than scope items.
////            AddProperty(logEvent, propertyFactory, s, tuple[1], update);
////        }
////#else
////		else if (state is ValueTuple<string, object?> tuple)
////		{
////			scopeItem = null;
////			AddProperty(logEvent, propertyFactory, tuple.Item1, tuple.Item2, update);
////		}
////#endif
////		else
////		{
////			scopeItem = propertyFactory.CreateProperty(NoName, state).Value;
////		}
////	}

////	/// <remarks>
////	/// Adapted from https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerScope.cs → <b>AddProperty</b>
////	/// <para>
////	/// Changes include:
////	/// <list type="bullet">
////	/// <item><description> Excanged usage of static <b>SerilogLogger</b> to <see cref="FrameworkLogger"/>. </description></item>
////	/// <item><description> Conditional compilation for features not available in <b>.NET Standard 2.0</b>. </description></item>
////	/// </list>
////	/// </para>
////	/// </remarks>
////	static void AddProperty(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, string key, object? value, bool update)
////	{
////		var destructureObjects = false;

////#if NET8_0_OR_GREATER
////		if (key.StartsWith('@'))
////#else
////		if (key.StartsWith("@"))
////#endif
////		{
////			key = FrameworkLogger.GetKeyWithoutFirstSymbol(FrameworkLogger.DestructureDictionary, key);
////			destructureObjects = true;
////		}
////#if NET8_0_OR_GREATER
////		else if (key.StartsWith('$'))
////#else
////		else if (key.StartsWith("$"))
////#endif
////		{
////			key = FrameworkLogger.GetKeyWithoutFirstSymbol(FrameworkLogger.StringifyDictionary, key);
////			value = value?.ToString();
////		}

////		var property = propertyFactory.CreateProperty(key, value, destructureObjects);
////		if (update)
////		{
////			logEvent.AddOrUpdateProperty(property);
////		}
////		else
////		{
////			logEvent.AddPropertyIfAbsent(property);
////		}
////	}

//	#endregion

//	#region Nested Types

//	sealed class DisposableAction : IDisposable
//    {
//        #region Delegates / Events
//        #endregion

//        #region Constants
//        #endregion

//        #region Fields

//        private readonly Action _dispose;

//        #endregion

//        #region Properties

//        public static IDisposable NoDisposableAction { get; } = new DisposableAction(() => { });

//        #endregion

//        #region (De)Constructors

//        public DisposableAction(Action dispose)
//        {
//            _dispose = dispose;
//        }

//        #endregion

//        #region Methods

//        /// <inheritdoc />
//        public void Dispose()
//        {
//            try
//            {
//                _dispose.Invoke();
//            }
//            catch (Exception) { /* ignore */ }
//        }

//        #endregion
//    }

//    #endregion
//}