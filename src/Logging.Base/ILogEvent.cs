#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Interface for log event data where <see cref="LogLevel"/> and <see cref="EventId"/> are generic types.
/// </summary>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
public interface ILogEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
{
	/// <summary> The event id as <typeparamref name="TEventId"/>. </summary>
	TEventId EventId { get; }

	/// <summary> The log level as <typeparamref name="TLogLevel"/>. </summary>
	TLogLevel LogLevel { get; }

	/// <summary> The message to log. </summary>
	string LogMessage { get; }

	/// <summary> Format arguments of <see cref="LogMessage"/>. </summary>
	object?[] Args { get; }

	/// <summary> Optional <see cref="System.Exception"/>. Default is<see langword="null"/>. </summary>
	Exception? Exception { get; }

	/// <summary> Optional <see cref="IPayload"/> that is applied to the log event as scope. Default is <see langword="null"/>. </summary>
	/// <remarks> Can be used to add additional key/value pairs directly to an event, just not as part of the regular message. </remarks>
	IPayload? Payload { get; }

	/// <summary>
	/// Deconstructs the <see cref="ILogEvent{TLogLevel, TEventId}"/> into its constituent properties.
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="Args"/> </param>
	/// <param name="payload"> <inheritdoc cref="Payload"/> </param>
	void Deconstruct(out TEventId eventId, out Exception? exception, out TLogLevel logLevel, out string logMessage, out object?[] args, out IPayload? payload);
}

/// <summary>
/// Wrapper for log event data where <see cref="LogLevel"/> and <see cref="EventId"/> are generic types.
/// </summary>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Data carrier with constructors and deconstruction only.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class LogEvent<TLogLevel, TEventId> : ILogEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <inheritdoc />
	public TEventId EventId { get; }

	/// <inheritdoc />
	public TLogLevel LogLevel { get; }

	/// <inheritdoc />
	public string LogMessage { get; }

	/// <inheritdoc />
	public object?[] Args { get; }

	/// <inheritdoc />
	public Exception? Exception { get; }

	/// <inheritdoc />
	public IPayload? Payload { get; init; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="Args"/> </param>
	public LogEvent(TEventId eventId, TLogLevel logLevel, string logMessage, params object?[] args)
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
	public LogEvent(TEventId eventId, Exception exception, TLogLevel logLevel, string logMessage, params object?[] args)
		: this(eventId, logLevel, logMessage, args)
	{
		this.Exception = exception;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Deconstruct(out TEventId eventId, out Exception? exception, out TLogLevel logLevel, out string logMessage, out object?[] args, out IPayload? payload)
	{
		eventId = this.EventId;
		exception = this.Exception;
		logLevel = this.LogLevel;
		logMessage = this.LogMessage;
		args = this.Args;
		payload = this.Payload;
	}

	#endregion
}

/// <summary>
/// Represents log event data where <see cref="LogLevel"/> and <see cref="EventId"/> are generic types that performs no logging and contains no event data.
/// </summary>
/// <typeparam name="TActual"> The actual type. Used for the static <see cref="Instance"/> </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Null-object data carrier with boilerplate defaults.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public abstract class NoLogEvent<TActual, TLogLevel, TEventId> : ILogEvent<TLogLevel, TEventId>
	where TActual : NoLogEvent<TActual, TLogLevel, TEventId>, new()
	where TLogLevel : struct, Enum
	where TEventId : struct
{
	/// <summary> Singleton instance of the <see cref="NoLogEvent{TActual, TLogLevel,TEventId}"/>. </summary>
	public static ILogEvent<TLogLevel, TEventId> Instance { get; } = new TActual();

	/// <inheritdoc />
	public abstract TEventId EventId { get; }

	/// <inheritdoc />
	public abstract TLogLevel LogLevel { get; }

	/// <inheritdoc />
	public string LogMessage => String.Empty;

	/// <inheritdoc />
	public object?[] Args => [];

	/// <inheritdoc />
	public Exception? Exception => null;

	/// <inheritdoc />
	public IPayload? Payload => null;

	/// <inheritdoc />
	public void Deconstruct(out TEventId eventId, out Exception? exception, out TLogLevel logLevel, out string logMessage, out object?[] args, out IPayload? payload)
	{
		eventId = this.EventId;
		exception = this.Exception;
		logLevel = this.LogLevel;
		logMessage = this.LogMessage;
		args = this.Args;
		payload = this.Payload;
	}
}