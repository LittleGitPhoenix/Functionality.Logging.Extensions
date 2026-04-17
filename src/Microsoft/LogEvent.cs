#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Interface for a <see cref="global::Microsoft.Extensions.Logging"/>-based log event data.
/// </summary>
public interface ILogEvent : ILogEvent<LogLevel, EventId>;

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
	
	/// <inheritdoc />
	public EventId EventId { get; }

	/// <inheritdoc />
	public LogLevel LogLevel { get; }

	/// <inheritdoc />
	public string LogMessage { get; }

	/// <inheritdoc />
	public object?[] Args { get; }

	/// <inheritdoc />
	public Exception? Exception { get; }

	/// <inheritdoc />
	public ILogScope? PayLoad { get; init; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="Args"/> </param>
	public LogEvent(EventId eventId, LogLevel logLevel, string logMessage, params object?[] args)
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
	public LogEvent(EventId eventId, Exception exception, LogLevel logLevel, string logMessage, params object?[] args)
		: this(eventId, logLevel, logMessage, args)
	{
		this.Exception = exception;
	}

	#endregion

	#region Methods

	///// <summary>
	///// Creates a new <see cref="LogEvent"/> instance.
	///// </summary>
	///// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	///// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	///// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	///// <param name="args"> <inheritdoc cref="Args"/> </param>
	///// <returns> A new <see cref="LogEvent"/> instance. </returns>
	//public static LogEvent Create(EventId eventId, LogLevel logLevel, string logMessage, params object?[] args)
	//{
	//	return new LogEvent(eventId, logLevel, logMessage, args);
	//}

	///// <summary>
	///// Creates a new <see cref="LogEvent"/> instance with exception.
	///// </summary>
	///// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	///// <param name="exception"> <inheritdoc cref="Exception"/> </param>
	///// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	///// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	///// <param name="args"> <inheritdoc cref="Args"/> </param>
	///// <returns> A new <see cref="LogEvent"/> instance. </returns>
	//public static LogEvent Create(EventId eventId, Exception exception, LogLevel logLevel, string logMessage, params object?[] args)
	//{
	//	return new LogEvent(eventId, exception, logLevel, logMessage, args);
	//}

	/// <inheritdoc />
	public void Deconstruct(out EventId eventId, out Exception? exception, out LogLevel logLevel, out string logMessage, out object?[] args, out ILogScope? payload)
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
/// Represents a <see cref="global::Microsoft.Extensions.Logging"/>-based log event that performs no logging and contains no event data.
/// </summary>
public class NoLogEvent : ILogEvent
{
	/// <summary> Singleton instance of the <see cref="NoLogEvent"/>. </summary>
	public static ILogEvent Instance { get; } = new NoLogEvent();

	/// <inheritdoc />
	public EventId EventId => new(-1, nameof(NoLogEvent));

	/// <inheritdoc />
	public LogLevel LogLevel => LogLevel.None;

	/// <inheritdoc />
	public string LogMessage => String.Empty;

	/// <inheritdoc />
	public object?[] Args => [];

	/// <inheritdoc />
	public Exception? Exception => null;

	/// <inheritdoc />
	public ILogScope? PayLoad => null;

	/// <inheritdoc />
	public void Deconstruct(out EventId eventId, out Exception? exception, out LogLevel logLevel, out string logMessage, out object?[] args, out ILogScope? payload)
	{
		eventId = this.EventId;
		exception = this.Exception;
		logLevel = this.LogLevel;
		logMessage = this.LogMessage;
		args = this.Args;
		payload = this.PayLoad;
	}
}