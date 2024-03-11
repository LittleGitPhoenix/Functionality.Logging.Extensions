#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// A <see cref="EventIdLogger"/> that can resolve messages from resource files.
/// </summary>
/// <remarks> Consider not inheriting from this class anymore. Instead, use the extension methods <b>LoggerExtensions.Log</b>. </remarks>
//[Obsolete($"Do not inherit from this class anymore. Instead, use the extension methods {nameof(LoggerExtensions.Log)}.")]
public abstract class EventIdResourceLogger : EventIdLogger
{
    #region Delegates / Events
    #endregion

    #region Constants
    #endregion

    #region Fields

    /// <summary> Collection of <see cref="ResourceManager"/>s used for resolving log messages. </summary>
    private readonly ICollection<ResourceManager> _resourceManagers;

    /// <summary> The <see cref="CultureInfo"/> used for logging. </summary>
    private readonly CultureInfo _logCulture;
	
    #endregion

    #region Properties
    #endregion

    #region (De)Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"> The underlying <see cref="ILogger"/>. </param>
    /// <param name="resourceManagers"> A collection of <see cref="ResourceManager"/>s that are used to resolve log messages. </param>
    /// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from <paramref name="resourceManagers"/>. Default value is the culture <b>lo</b>. </param>
    protected EventIdResourceLogger(ILogger logger, ICollection<ResourceManager> resourceManagers, CultureInfo? logCulture = null)
        : base (logger)
    {
        // Save parameters.
        _resourceManagers = resourceManagers;
        _logCulture = logCulture ?? CultureInfo.CreateSpecificCulture("lo");

        // Initialize fields.
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"> The underlying <see cref="ILogger"/>. </param>
    /// <param name="name"> The name of the logger. Will be added as scope with the property name <paramref name="propertyName"/>. </param>
    /// <param name="resourceManagers"> A collection of <see cref="ResourceManager"/>s that are used to resolve log messages. </param>
    /// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from <paramref name="resourceManagers"/>. Default value is the culture <b>lo</b>. </param>
    /// <param name="propertyName"> The name of the property under which the logger <paramref name="name"/> will be scoped. Default is <see cref="EventIdLogger.LoggerNameProperty"/>. </param>
    protected EventIdResourceLogger(ILogger logger, string name, ICollection<ResourceManager> resourceManagers, CultureInfo? logCulture = null, string propertyName = LoggerNameProperty)
        : base (logger, name, propertyName)
    {
        // Save parameters.
        _resourceManagers = resourceManagers;
        _logCulture = logCulture ?? CultureInfo.CreateSpecificCulture("lo");

        // Initialize fields.
    }

	#endregion

	#region Methods

	#region Log Functions
	
	/// <inheritdoc cref="LoggerExtensions.Log(ILogger, int, LogLevel, ResourceManager, string, object[], object[], CultureInfo)"/>
	//[Obsolete($"Use the {nameof(ILogger)} extension method {nameof(LoggerExtensions.LogEventFromResource)} instead.")]
	protected internal string LogEventFromResource(int eventId, LogLevel logLevel, string resourceName, object[]? logArgs = null, object[]? messageArgs = null)
        => this.LogEventFromResource((EventId) eventId, null, logLevel, resourceName, logArgs, messageArgs);

	/// <inheritdoc cref="LoggerExtensions.Log(ILogger, int, Exception, LogLevel, ResourceManager, string, object[], object[], CultureInfo)"/>
	//[Obsolete($"Use the {nameof(ILogger)} extension method {nameof(LoggerExtensions.LogEventFromResource)} instead.")]
	protected internal string LogEventFromResource(int eventId, Exception exception, LogLevel logLevel, string resourceName, object[]? logArgs = null, object[]? messageArgs = null)
        => this.LogEventFromResource((EventId) eventId, exception, logLevel, resourceName, logArgs, messageArgs);

	#endregion

    #region Helper

    /// <summary>
    /// Logs an event with a message resolved from a resource file and returns this message translated into the current ui culture (or its nearest fallback).
    /// </summary>
    /// <param name="eventId"> The id of the event. </param>
    /// <param name="exception"> An optional <see cref="Exception"/> to log. Default is null. </param>
    /// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
    /// <param name="resourceName"> The name of the resource that is the log message. </param>
    /// <param name="logArgs"> Optional arguments passed to the log message. Those arguments are directly passed to the underlying logger instance. </param>
    /// <param name="messageArgs"> Optional arguments merged into the returned output message via <see cref="String.Format(string,object?[])"/>. If this is omitted, then <paramref name="logArgs"/> will be used. </param>
    /// <returns> The translated log message. </returns>
    private string LogEventFromResource(EventId eventId, Exception? exception, LogLevel logLevel, string resourceName, object[]? logArgs = null, object[]? messageArgs = null)
    {
		logArgs ??= Array.Empty<object>();
        messageArgs ??= logArgs;
        var (logMessage, unformattedOutput) = LoggerExtensions.GetMessages(_resourceManagers, resourceName, _logCulture, logArgs, messageArgs, eventId);

        // Log
        LoggerExtensions.Log(this, eventId, exception, logLevel, logMessage, payload: null, logArgs);

		// Format output message
		try
        {
            return String.Format(unformattedOutput, messageArgs);
        }
        catch (FormatException)
        {
            var arguments = messageArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", messageArgs);
            return $"Could not format the message '{unformattedOutput}' because of a mismatch with the format arguments '{arguments}'.";
        }
    }

    #endregion

    #endregion
}