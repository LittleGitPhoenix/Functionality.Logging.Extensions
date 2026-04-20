namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Specifies the type of scope used for logging operations.
/// </summary>
/// <remarks>
/// This enumeration indicates whether an <see cref="ILogScope"/> is independent of the current execution context or if it is aware of and flows with the execution context.
/// This affects how log data is correlated across asynchronous or multithreaded operations.
/// </remarks>
public enum LogScopeType
{
	/// <summary> Represents an <see cref="ILogScope"/> that is not influenced or controlled by external factors. </summary>
	Independent,
	/// <summary> Provides an <see cref="ILogScope"/> that is bound to the current execution context. </summary>
	ExecutionContextAware,
}