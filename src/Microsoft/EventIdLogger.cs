#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using ILogger = global::Microsoft.Extensions.Logging.ILogger;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft
{
	/// <summary>
	/// An <see cref="ILogger"/> decorator that logs messages along with an event id.
	/// </summary>
	public abstract class EventIdLogger : ILogger
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary> The underlying <see cref="ILogger"/>. </summary>
		protected internal ILogger Logger { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"> The underlying <see cref="ILogger"/>. </param>
		protected EventIdLogger(ILogger logger)
		{
			// Save parameters.
			this.Logger = logger;

			// Initialize fields.
		}

		#endregion

		#region Methods

		#region Implementation of ILogger

		/// <inheritdoc />
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
			=> this.Logger.Log(logLevel, eventId, state, exception, formatter);

		/// <inheritdoc />
		public bool IsEnabled(LogLevel logLevel)
			=> this.Logger.IsEnabled(logLevel);

		/// <inheritdoc />
		public IDisposable BeginScope<TState>(TState state)
			=> this.Logger.BeginScope(state);

		#endregion

		#region Log Functions

		/// <summary>
		/// Logs an event with a given <paramref name="logMessage"/>.
		/// </summary>
		/// <param name="eventId"> The id of the event. </param>
		/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
		/// <param name="logMessage"> The message to log. </param>
		/// <param name="args"> Arguments passed to the log message. </param>
		protected internal void LogEvent(int eventId, LogLevel logLevel, string logMessage, params object[] args)
			=> this.LogEvent((EventId) eventId, null, logLevel, logMessage, args);

		/// <summary>
		/// Logs an event with a given <paramref name="logMessage"/> and <paramref name="exception"/>.
		/// </summary>
		/// <param name="eventId"> The id of the event. </param>
		/// <param name="exception"> The <see cref="Exception"/> to log. </param>
		/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
		/// <param name="logMessage"> The message to log. </param>
		/// <param name="args"> Arguments passed to the log message. </param>
		protected internal void LogEvent(int eventId, Exception exception, LogLevel logLevel, string logMessage, params object[] args)
			=> this.LogEvent((EventId) eventId, exception, logLevel, logMessage, args);

		#endregion

		#region Helper
		
		/// <summary>
		/// Logs messages while catching format exceptions.
		/// </summary>
		/// <param name="eventId"> The <see cref="EventId"/> of the event. </param>
		/// <param name="exception"> An optional <see cref="Exception"/> to log. Default is null. </param>
		/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
		/// <param name="logMessage"> The message to log. </param>
		/// <param name="args"> Arguments passed to the log message. </param>
		private protected void LogEvent(EventId eventId, Exception? exception, LogLevel logLevel, string logMessage, params object[] args)
		{
			// Log
			try
			{
				this.Log(logLevel, eventId, exception, logMessage, args);
			}
			catch (AggregateException ex) when (ex.Flatten().InnerExceptions.Select(exception => exception.GetType()).Contains(typeof(IndexOutOfRangeException)))
			{
				var arguments = args.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", args);
				this.Log(logLevel, eventId, exception, $"Could not format the message '{logMessage.Replace("{", "{{").Replace("}", "}}")}' because of a mismatch with the supplied arguments {arguments}.", args);
			}
			catch (Exception ex)
			{
				var arguments = args.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", args);
				Debug.WriteLine($"Could not write log for the message '{logMessage}' with arguments '{arguments}'.");
			}
		}

		#endregion
		
		#endregion
	}
}