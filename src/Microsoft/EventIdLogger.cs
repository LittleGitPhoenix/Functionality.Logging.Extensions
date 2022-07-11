#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// An <see cref="ILogger"/> decorator that logs messages along with an event id.
/// </summary>
/// <remarks> Consider not inheriting from this class anymore. Instead use the extension methods <b>LoggerExtensions.Log</b>. </remarks>
//[Obsolete($"Do not inherit from this class anymore. Instead use the extension methods {nameof(LoggerExtensions.Log)}.")]
public abstract class EventIdLogger : ILogger
{
    #region Delegates / Events
    #endregion

    #region Constants

    /// <summary> The name of the property under which the logger name will be scoped. </summary>
    public const string LoggerNameProperty = "Context";

    #endregion

    #region Fields

    private readonly ILogger _logger;
    
	#endregion

    #region Properties
    #endregion

    #region (De)Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"> The underlying <see cref="ILogger"/>. </param>
    protected EventIdLogger(ILogger logger)
    {
        // Save parameters.
        _logger = logger;

        // Initialize fields.
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"> The underlying <see cref="ILogger"/>. </param>
    /// <param name="name"> The name of the logger. Will be added as scope with the property name <paramref name="propertyName"/>. </param>
    /// <param name="propertyName"> The name of the property under which the logger <paramref name="name"/> will be scoped. Default is <see cref="EventIdLogger.LoggerNameProperty"/>. </param>
    protected EventIdLogger(ILogger logger, string name, string propertyName = EventIdLogger.LoggerNameProperty)
        : this(logger)
    {
        if (String.IsNullOrWhiteSpace(propertyName)) propertyName = EventIdLogger.LoggerNameProperty;
        this.CreateScope((propertyName, name));
    }

    #endregion

    #region Methods

    #region Implementation of ILogger

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _logger.Log(logLevel, eventId, state, exception, formatter);

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
        => _logger.IsEnabled(logLevel);

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
        => _logger.BeginScope(state);

	#endregion

	#region Log Functions

	/// <inheritdoc cref="LoggerExtensions.Log(ILogger, EventId, LogLevel, string, object?[])"/>
	//[Obsolete($"Use the {nameof(ILogger)} extension method {nameof(LoggerExtensions.Log)} instead.")]
	protected internal void LogEvent(int eventId, LogLevel logLevel, string logMessage, params object[] args)
        => _logger.Log(eventId, logLevel, logMessage, args);

	/// <inheritdoc cref="LoggerExtensions.Log(ILogger, EventId, Exception?, LogLevel, string, object?[])"/>
	//[Obsolete($"Use the {nameof(ILogger)} extension method {nameof(LoggerExtensions.Log)} instead.")]
	protected internal void LogEvent(int eventId, Exception exception, LogLevel logLevel, string logMessage, params object[] args)
		=> _logger.Log(eventId, exception, logLevel, logMessage, args);

    #endregion
		
    #endregion
}