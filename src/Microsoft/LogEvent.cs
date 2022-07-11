using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Wrapper containing log event data.
/// </summary>
public class LogEvent
{
	///// <summary> Null-object </summary>
	//public static LogEvent NoLogEvent { get; } = new(default, LogLevel.None, String.Empty);

	/// <summary> The id of the event. </summary>
	public int EventId { get; }

	/// <summary>
	/// The events <see cref="global::Microsoft.Extensions.Logging.LogLevel"/>.
	/// </summary>
	public LogLevel LogLevel { get; }

	/// <summary> The message to log. </summary>
	public string LogMessage { get; }

	/// <summary> Format arguments of <see cref="LogMessage"/>. </summary>
	public object?[] Args { get; }

	/// <summary> Optional <see cref="System.Exception"/>. Default is <b>null</b>. </summary>
	public Exception? Exception { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="Args"/> </param>
	public LogEvent(int eventId, LogLevel logLevel, string logMessage, params object?[] args)
	{
		this.EventId = eventId;
		this.LogLevel = logLevel;
		this.LogMessage = logMessage;
		this.Args = args;
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="Args"/> </param>
	public LogEvent(int eventId, Exception exception, LogLevel logLevel, string logMessage, params object?[] args)
		: this(eventId, logLevel, logMessage, args)
	{
		this.Exception = exception;
	}
}

/// <summary>
/// Wrapper containing log event data obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
public class LogResourceEvent
{
	///// <summary> Null-object </summary>
	//public static LogResourceEvent NoLogResourceEvent { get; } = new(default, LogLevel.None, new ResourceManager(typeof(LogResourceEvent)), String.Empty);

	/// <inheritdoc cref = "LogEvent.EventId" />
	public int EventId { get; }

	/// <inheritdoc cref = "LogEvent.LogLevel" />
	public LogLevel LogLevel { get; }

	/// <summary> The <see cref="System.Resources.ResourceManager"/> from where the log message is obtained. </summary>
	public ResourceManager ResourceManager { get; }

	/// <summary> The name of the resource in <see cref="ResourceManager"/>. </summary>
	public string ResourceName { get; }

	/// <summary> Format arguments of the log message. </summary>
	public object?[] LogArgs { get; }

	/// <summary> Format arguments of the returned message. </summary>
	public object?[] MessageArgs { get; }

	/// <inheritdoc cref = "LogEvent.Exception" />
	public Exception? Exception { get; }

	/// <summary> The <see cref="System.Globalization.CultureInfo"/> that contains the log message. </summary>
	public CultureInfo? LogCulture { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="resourceManager"> <inheritdoc cref="ResourceManager"/> </param>
	/// <param name="resourceName"> <inheritdoc cref="ResourceName"/> </param>
	/// <param name="logArgs"> <inheritdoc cref="LogArgs"/> </param>
	/// <param name="messageArgs"> <inheritdoc cref="MessageArgs"/> </param>
	/// <param name="logCulture"> <inheritdoc cref="LogCulture"/>. Default value is the culture <b>lo</b>. </param>
	public LogResourceEvent(int eventId, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? logArgs = null, object?[]? messageArgs = null, CultureInfo? logCulture = null)
	{
		this.EventId = eventId;
		this.LogLevel = logLevel;
		this.ResourceManager = resourceManager;
		this.ResourceName = resourceName;
		this.LogArgs = logArgs ?? Array.Empty<object?>();
		this.MessageArgs = messageArgs ?? Array.Empty<object?>();
		this.LogCulture = logCulture;
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="resourceManager"> <inheritdoc cref="ResourceManager"/> </param>
	/// <param name="resourceName"> <inheritdoc cref="ResourceName"/> </param>
	/// <param name="logArgs"> <inheritdoc cref="LogArgs"/> </param>
	/// <param name="messageArgs"> <inheritdoc cref="MessageArgs"/> </param>
	/// <param name="logCulture"> <inheritdoc cref="LogCulture"/>. Default value is the culture <b>lo</b>. </param>
	public LogResourceEvent(int eventId, Exception exception, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? logArgs = null, object?[]? messageArgs = null, CultureInfo? logCulture = null)
		: this(eventId, logLevel, resourceManager, resourceName, logArgs, messageArgs, logCulture)
	{
		this.Exception = exception;
	}
}