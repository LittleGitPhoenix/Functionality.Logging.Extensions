#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Implementation of an <see cref="ILogger"/> that handles <see cref="ILogScope"/>s and forwards events to another <see cref="ILogger"/>.
/// </summary>
public class LogScopeHandlingLogger : ILogger
{
	#region Delegates / Events
    #endregion

    #region Constants
	#endregion

	#region Fields

	private readonly ILogger _underlyingLogger;

	#endregion

	#region Properties

	internal ILogScopeManager ScopeManager { get; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor using a new <see cref="LogScopeManager"/>.
	/// </summary>
	/// <param name="underlyingLogger"> The <see cref="ILogger"/> that will be used to output log events. </param>
	public LogScopeHandlingLogger(ILogger underlyingLogger)
		: this(underlyingLogger, new LogScopeManager()) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="underlyingLogger"> The <see cref="ILogger"/> that will be used to output log events. </param>
	/// <param name="scopeManager"> The <see cref="ILogScopeManager"/> that handles log scopes. </param>
	public LogScopeHandlingLogger(ILogger underlyingLogger, ILogScopeManager scopeManager)
    {
        // Save parameters.
		_underlyingLogger = underlyingLogger;
        this.ScopeManager = scopeManager;

        // Initialize fields.
    }

    #endregion

    #region Methods

    #region Implementation of Microsoft.Extensions.Logging.ILogger

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => _underlyingLogger.IsEnabled(logLevel);

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => this.ScopeManager.AddScope(state);

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		//* Collect all scopes and begin a separate scope on the underlying logger for each one.
		//! Each scope is passed individually (not wrapped in a collection) so that loggers like Serilog can recognize each scope's runtime type (e.g. IEnumerable<KeyValuePair<string, object?>>) via pattern matching and extract individual properties from it.
		//! Wrapping all scopes into a single IEnumerable<object> would break this because the container type would not match the pattern, causing properties to be lost.
		//* From here it is the responsibility of the underlying logger to ensure that the scopes are properly added to the log event.
		//* Ideally, the underlying logger should add all scopes as execution context aware (AsyncLocal) properties to the log event regardless of their actual LogScopeType,
		//* as this method directly leads to the log event being emitted and parallel log events from other threads/tasks should not interfere with the scopes of the log event processed here.
		//* Be aware though, that it is not guaranteed that the underlying logger actually adds the scopes as execution context aware properties or handles scopes at all.
		//* In any way, further handling is out of scope for this class.
		var scopeValues = this.ScopeManager.GetScopeValues();
		var scopes = scopeValues.Select(singleScopeValue => _underlyingLogger.BeginScope(singleScopeValue)).ToArray();
		try
		{
			_underlyingLogger.Log(logLevel, eventId, state, exception, formatter);
		}
		finally
		{
			foreach (var scope in scopes) scope?.Dispose();
		}
	}

	#endregion

	#region Helper
	#endregion

	#endregion
}