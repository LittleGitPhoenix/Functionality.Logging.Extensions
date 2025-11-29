#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Interface for log event data.
/// </summary>
public interface ILogEvent
{
	/// <summary> The id of the event. </summary>
	int EventId { get; }

	/// <summary>
	/// The events <see cref="global::Microsoft.Extensions.Logging.LogLevel"/>.
	/// </summary>
	LogLevel LogLevel { get; }

	/// <summary> The message to log. </summary>
	string LogMessage { get; }

	/// <summary> Format arguments of <see cref="LogMessage"/>. </summary>
	object?[] Args { get; }

	/// <summary> Optional <see cref="System.Exception"/>. Default is <b>null</b>. </summary>
	Exception? Exception { get; }

	/// <summary> Optional payload that is applied to the log event as scope. Default is <b>null</b>. </summary>
	/// <remarks> Can be used to add additional key/value pairs to an event even though they are not part of the regular message. </remarks>
	LogScope? PayLoad { get; }

	/// <summary>
	/// Deconstructs the <see cref="ILogEvent"/> into its constituent properties.
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="Args"/> </param>
	/// <param name="payload"> <inheritdoc cref="PayLoad"/> </param>
	void Deconstruct(out int eventId, out Exception? exception, out LogLevel logLevel, out string logMessage, out object?[] args, out LogScope? payload);
}

/// <summary>
/// Wrapper containing log event data.
/// </summary>
public class LogEvent : ILogEvent
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <summary> Null-object </summary>
	public static ILogEvent NoLogEvent { get; } = new LogEvent(default, LogLevel.None, String.Empty);

	/// <inhertdoc />
	public int EventId { get; }

	/// <inhertdoc />
	public LogLevel LogLevel { get; }

	/// <inhertdoc />
	public string LogMessage { get; }

	/// <inhertdoc />
	public object?[] Args { get; }

	/// <inhertdoc />
	public Exception? Exception { get; }

	/// <inhertdoc />
	public LogScope? PayLoad { get; init; }

	#endregion

	#region (De)Constructors

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
		this.Args = args ?? [];
	}

	/// <summary>
	/// Constructor with exception
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

	#endregion

	#region Methods

	/// <inhertdoc />
	public void Deconstruct(out int eventId, out Exception? exception, out LogLevel logLevel, out string logMessage, out object?[] args, out LogScope? payload)
	{
		eventId = this.EventId;
		exception = this.Exception;
		logLevel = this.LogLevel;
		logMessage = this.LogMessage;
		args = this.Args;
		payload = this.PayLoad;
	}

	#endregion
}

/// <summary>
/// Represents a log event whose message is obtained from a <see cref="System.Resources.ResourceManager"/> thus supporting localization and formatting.
/// </summary>
public interface ILogResourceEvent : ILogEvent
{
	/// <summary> The translated output message. </summary>
	string OutputMessage{ get; }

	/// <summary>
	/// Deconstructs the <see cref="ILogResourceEvent"/> into its constituent properties.
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="ILogEvent.EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="ILogEvent.Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="ILogEvent.LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="ILogEvent.LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="ILogEvent.Args"/> </param>
	/// <param name="outputMessage"> <inheritdoc cref="OutputMessage"/> </param>	
	/// <param name="payload"> <inheritdoc cref="ILogEvent.PayLoad"/> </param>
	void Deconstruct(out int eventId, out Exception? exception, out LogLevel logLevel, out string logMessage, out object?[] args, out string outputMessage, out LogScope? payload);
}

/// <summary>
/// Wrapper containing log event data obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
public class LogResourceEvent : LogEvent, ILogResourceEvent
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	/// <inhertdoc />
	private readonly ResourceManager _resourceManager;

	/// <inhertdoc />
	private readonly string _resourceName;

	/// <inhertdoc />
	//public object?[] OutputArgs { get; }
	private readonly object?[] _outputArgs;

	#endregion

	#region Properties

	/// <summary> The <see cref="CultureInfo"/> used for logging. </summary>
	/// <remarks> Default value is the culture <b>lo</b>. </remarks>
	public static CultureInfo LogCulture
	{
		get;
		set => field = value ?? CultureInfo.CreateSpecificCulture("lo");
	} = CultureInfo.CreateSpecificCulture("lo");

	/// <summary> Null-object </summary>
	public static ILogResourceEvent NoLogResourceEvent { get; } = new LogResourceEvent(default, LogLevel.None, null!, String.Empty, [], null, null);

	/// <inhertdoc />
	/// <remarks> The message is build everyt time the property is accessed. Typically this is only done once. Not pre-loading it has the benefit to dynamically adapt to changes in the UI culture. </remarks>
	public string OutputMessage => this.GetOutputMessage();

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="resourceManager"> The <see cref="System.Resources.ResourceManager"/> from where log and output message is obtained. </param>
	/// <param name="resourceName"> The name of the resource in the <paramref name="resourceManager"/>. </param>
	/// <param name="args"> <inheritdoc cref="LogEvent.Args"/> </param>
	/// <param name="outputArgs"> Format arguments for the <see cref="OutputMessage"/>. If this is <see langword="null"/> <paramref name="args"/> will be used instead. </param>
	/// <param name="logCulture"> The culture of the log message. If this is <see langword="null"/> <see cref="LogCulture"/> will be used instead. </param>
	public LogResourceEvent(int eventId, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
		: base(eventId, logLevel, GetLogMessage(eventId, resourceManager, resourceName, args ?? [], logCulture), args ?? [])
	{
		_resourceManager = resourceManager;
		_resourceName = resourceName;
		_outputArgs = outputArgs ?? args ?? [];
	}

	/// <summary>
	/// Constructor with exception
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="resourceManager"> The <see cref="System.Resources.ResourceManager"/> from where log and output message is obtained. </param>
	/// <param name="resourceName"> The name of the resource in the <paramref name="resourceManager"/>. </param>
	/// <param name="args"> <inheritdoc cref="LogEvent.Args"/> </param>
	/// <param name="outputArgs"> Format arguments for the <see cref="OutputMessage"/>. If this is <see langword="null"/> <paramref name="args"/> will be used instead. </param>
	/// <param name="logCulture"> The culture of the log message. If this is <see langword="null"/> <see cref="LogCulture"/> will be used instead. </param>
	public LogResourceEvent(int eventId, Exception exception, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
		: base(eventId, exception, logLevel, GetLogMessage(eventId, resourceManager, resourceName, args ?? [], logCulture), args ?? [])
	{
		_resourceManager = resourceManager;
		_resourceName = resourceName;
		_outputArgs = outputArgs ?? args ?? [];
	}

	#endregion

	#region Methods	

	/// <summary>
	/// Obtains the unformatted log message identified by the resource name from the resource manager using <see cref="LogCulture"/>.
	/// </summary>
	/// <returns> The unformatted log message. On failure a message describing the error is returned. </returns>
	private static string GetLogMessage(int eventId, ResourceManager resourceManager, string resourceName, object?[] args, CultureInfo? logCulture)
	{
		return resourceManager.GetString(resourceName, logCulture ?? LogCulture)
			?? $"No log-message found for resource '{resourceName}' of event id {eventId}. Add the ressource to the resource manager {resourceManager.GetType().FullName}. Arguments where: {GetArgsAsString(args)}";
			;
	}

	/// <summary>
	/// Obtains the formatted output message identified by the resource name from the resource manager using the caller UI culture.
	/// </summary>
	/// <returns> The formatted output message. On failure a message describing the error is returned. </returns>
	private string GetOutputMessage()
	{
		// Get the unformatted output message from resource manager.
		var unformattedMessage = _resourceManager.GetString(_resourceName);

		if (unformattedMessage is null) return $"No output-message found for resource '{_resourceName}' of event id {base.EventId}. Add the ressource to the resource manager {_resourceManager.GetType().FullName}. Arguments where: {GetArgsAsString(_outputArgs)}";

		// Format the output message.
		try
		{
			return String.Format(unformattedMessage, _outputArgs);
		}
		catch (FormatException)
		{
			var arguments = GetArgsAsString(_outputArgs);
			return $"Could not format the output-message '{unformattedMessage}' for resource '{_resourceName}' of event id {base.EventId} because of a mismatch with the format arguments '{arguments}'.";
		}
	}

	private static string GetArgsAsString(object?[] args)
	{
		if (args is null || args.Length == 0) return "<NO ARGUMENTS>";
		return String.Join(",", args);
	}

	/// <inhertdoc />
	public void Deconstruct(out int eventId, out Exception? exception, out LogLevel logLevel, out string logMessage, out object?[] args, out string outputMessage, out LogScope? payload)
	{
		eventId = base.EventId;
		exception = base.Exception;
		logLevel = base.LogLevel;
		logMessage = base.LogMessage;
		args = base.Args;
		outputMessage = this.OutputMessage;
		payload = base.PayLoad;
	}

	#endregion
}