#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Interface for log event data where <see cref="LogLevel"/> and <see cref="EventId"/> are generic type.
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

	/// <summary> Optional <see cref="System.Exception"/>. Default is <b>null</b>. </summary>
	Exception? Exception { get; }

	/// <summary> Optional payload that is applied to the log event as scope. Default is <b>null</b>. </summary>
	/// <remarks> Can be used to add additional key/value pairs to an event even though they are not part of the regular message. </remarks>
	ILogScope? PayLoad { get; }

	/// <summary>
	/// Deconstructs the <see cref="eventId"/> into its constituent properties.
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="exception"/> </param>
	/// <param name="exception"> <inheritdoc cref="logLevel"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="logMessage"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="args"/> </param>
	/// <param name="args"> <inheritdoc cref="payload"/> </param>
	/// <param name="payload"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}"/> </param>
	void Deconstruct(out TEventId eventId, out Exception? exception, out TLogLevel logLevel, out string logMessage, out object?[] args, out ILogScope? payload);
}