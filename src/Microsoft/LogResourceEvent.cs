#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Interface for a <see cref="global::Microsoft.Extensions.Logging"/>-based log event data whose message is obtained from a <see cref="System.Resources.ResourceManager"/> thus supporting localization and formatting.
/// </summary>
public interface ILogResourceEvent : ILogResourceEvent<LogLevel, EventId>, ILogEvent;

/// <summary>
/// Wrapper containing log event data obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
public class LogResourceEvent : LogResourceEvent<LogLevel, EventId>, ILogResourceEvent
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.EventId"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogLevel"/> </param>
	/// <param name="resourceManager"> The <see cref="System.Resources.ResourceManager"/> from where log and output message is obtained. </param>
	/// <param name="resourceName"> The name of the resource in the <paramref name="resourceManager"/>. </param>
	/// <param name="args"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Args"/> </param>
	/// <param name="outputArgs"> Format arguments for the <see cref="ILogResourceEvent{TLogLevel,TEventId}.OutputMessage"/>. If this is <see langword="null"/> <paramref name="args"/> will be used instead. </param>
	/// <param name="logCulture"> The culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	public LogResourceEvent(EventId eventId, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
		: base(eventId, logLevel, resourceManager, resourceName, args, outputArgs, logCulture) { }

	/// <summary>
	/// Constructor with exception
	/// </summary>
	/// <param name="eventId"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.EventId"/> </param>
	/// <param name="exception"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Exception"/> </param>
	/// <param name="logLevel"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogLevel"/> </param>
	/// <param name="resourceManager"> The <see cref="System.Resources.ResourceManager"/> from where log and output message is obtained. </param>
	/// <param name="resourceName"> The name of the resource in the <paramref name="resourceManager"/>. </param>
	/// <param name="args"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Args"/> </param>
	/// <param name="outputArgs"> Format arguments for the <see cref="ILogResourceEvent{TLogLevel,TEventId}.OutputMessage"/>. If this is <see langword="null"/> <paramref name="args"/> will be used instead. </param>
	/// <param name="logCulture"> The culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	public LogResourceEvent(EventId eventId, Exception exception, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
		: base(eventId, exception, logLevel, resourceManager, resourceName, args, outputArgs, logCulture) { }
}

/// <summary>
/// Represents a log resource event that performs no logging and contains no event data.
/// </summary>
public class NoLogResourceEvent : NoLogResourceEvent<NoLogResourceEvent, LogLevel, EventId>, ILogResourceEvent
{
	/// <inheritdoc />
	public override EventId EventId => new(-1, nameof(NoLogEvent));

	/// <inheritdoc />
	public override LogLevel LogLevel => LogLevel.None;
}