#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft
{
	/// <summary>
	/// A <see cref="EventIdLogger"/> that can resolve messages from resource files.
	/// </summary>
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

		/// <summary> Cache for already resolved messages. </summary>
		private readonly ConcurrentDictionary<EventId, LogData> _cache;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"> The underlying <see cref="ILogger"/>. </param>
		/// <param name="resourceManagers"> A collection of <see cref="ResourceManager"/>s that are used to resolve log messages. </param>
		/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from <paramref name="resourceManagers"/>. Default value is the culture 'lo'. </param>
		protected EventIdResourceLogger(ILogger logger, ICollection<ResourceManager> resourceManagers, CultureInfo? logCulture = null)
			: base (logger)
		{
			// Save parameters.
			_resourceManagers = resourceManagers;
			_logCulture = logCulture ?? CultureInfo.CreateSpecificCulture("lo");

			// Initialize fields.
			_cache = new ConcurrentDictionary<EventId, LogData>();
		}

		#endregion

		#region Methods

		#region Log Functions
		
		/// <summary>
		/// Logs an event with a message resolved from a resource file and returns this message translated into the current ui culture (or its nearest fallback).
		/// </summary>
		/// <param name="eventId"> The id of the event. </param>
		/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
		/// <param name="resourceName"> The name of the resource that is the log message. </param>
		/// <param name="logArgs"> Optional arguments passed to the log message. Those arguments are directly passed to the underlying logger instance. </param>
		/// <param name="messageArgs"> Optional arguments merged into the returned output message via <see cref="String.Format(string,object?[])"/>. If this is omitted, then <paramref name="logArgs"/> will be used. </param>
		/// <returns> The translated log message. </returns>
		protected internal string LogEventFromResource(int eventId, LogLevel logLevel, string resourceName, object[]? logArgs = null, object[]? messageArgs = null)
		{
			logArgs ??= Array.Empty<object>();
			messageArgs ??= logArgs;

			var @event = (EventId)eventId;
			var (logMessage, unformattedOutput) = this.GetLogData(@event, resourceName, logArgs, messageArgs);

			// Log
			this.LogEvent(@event, logLevel, logMessage, logArgs);

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

		#region Helper

		/// <summary>
		/// Gets the <see cref="LogData"/> from the internal cache or creates a new instance.
		/// </summary>
		/// <param name="eventId"> The <see cref="EventId"/>. </param>
		/// <param name="resourceName"> The name of the resource that represents the log message. </param>
		/// <param name="logArgs"> Arguments passed to the log message. </param>
		/// <param name="messageArgs"> Arguments merged into the returned output message. </param>
		/// <returns></returns>
		private LogData GetLogData(EventId eventId, string resourceName, object[] logArgs, object[] messageArgs)
		{
			return _cache.GetOrAdd(eventId, _ => EventIdResourceLogger.BuildLogData(eventId, _resourceManagers, resourceName, logArgs, messageArgs, _logCulture));
		}

		/// <summary>
		/// Creates <see cref="LogData"/>.
		/// </summary>
		/// <param name="eventId"> The <see cref="EventId"/>. </param>
		/// <param name="resourceManagers"> A collection of <see cref="ResourceManager"/>s that are queried one by one to find the proper messages for <paramref name="resourceName"/>. </param>
		/// <param name="resourceName"> The name of the resource that represents the log message. </param>
		/// <param name="logArgs"> Arguments passed to the log message. </param>
		/// <param name="messageArgs"> Arguments merged into the returned output message. </param>
		/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. Default value is the culture 'lo'. </param>
		/// <returns> A new <see cref="LogData"/> object. </returns>
		private static LogData BuildLogData(EventId eventId, ICollection<ResourceManager> resourceManagers, string resourceName, object[] logArgs, object[] messageArgs, CultureInfo? logCulture = null)
		{
			logCulture ??= CultureInfo.CreateSpecificCulture("lo");
			string? logMessage = null;
			string? outputMessage = null;
			foreach (var resourceManager in resourceManagers)
			{
				logMessage = resourceManager.GetString(resourceName, logCulture);
				if (logMessage is not null)
				{
					outputMessage = resourceManager.GetString(resourceName);
					break;
				}
			}
			logMessage ??= $"No log-message found for resource '{resourceName}' of event id {eventId}. Arguments where: {(logArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", logArgs))}";
			outputMessage ??= $"No output-message found for resource '{resourceName}' of event id {eventId}. Arguments where: {(messageArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", messageArgs))}";

			return new LogData(eventId, logMessage!, outputMessage!);
		}

		#endregion

		#endregion
	}
}