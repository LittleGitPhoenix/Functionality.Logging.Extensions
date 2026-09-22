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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Pure pass-through class.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class LogEvent : LogEvent<LogLevel, EventId>, ILogEvent
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Args"/> </param>
	public LogEvent(EventId eventId, LogLevel logLevel, string logMessage, params object?[] args)
		: base(eventId, logLevel, logMessage, args) { }

	/// <summary>
	/// Constructor with exception
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="LogLevel"/> </param>
	/// <param name="logMessage"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogMessage"/> </param>
	/// <param name="args"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Args"/> </param>
	public LogEvent(EventId eventId, Exception exception, LogLevel logLevel, string logMessage, params object?[] args)
		: base(eventId, exception, logLevel, logMessage, args) { }
}

/// <summary>
/// Represents a <see cref="global::Microsoft.Extensions.Logging"/>-based log event that performs no logging and contains no event data.
/// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Pure pass-through class.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class NoLogEvent : NoLogEvent<NoLogEvent, LogLevel, EventId>, ILogEvent
{
	/// <inheritdoc />
	public override EventId EventId => new(-1, nameof(NoLogEvent));

	/// <inheritdoc />
	public override LogLevel LogLevel => LogLevel.None;
}