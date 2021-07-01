using System;
using System.Net.Http;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Provides extension methods for <see cref="LoggerSinkConfiguration"/>.
	/// </summary>
	public static class LoggerSinkConfigurationExtensions
	{
		/// <summary>
		/// Extends Serilog configuration to write events to Seq.
		/// </summary>
		/// <param name="writeTo"> The extended <see cref="LoggerSinkConfiguration"/>. </param>
		/// <param name="seqHost"> The seq host like https://localhost or https://localhost:5341. </param>
		/// <param name="seqPort"> An optional port of the seq server. The port can also be specified in <paramref name="seqHost"/>. </param>
		/// <param name="applicationTitle"> The title of the application that will log to seq. </param>
		/// <param name="configurationApiKey"> Api key used for configuration. </param>
		/// <param name="retryOnError"> Should registering the application with the seq server automatically be retried, if it initially failed. </param>
		/// <param name="restrictedToMinimumLevel"> The minimum log event level required in order to write an event to the sink. </param>
		/// <param name="batchPostingLimit"> The maximum number of events to post in a single batch. </param>
		/// <param name="period"> The time to wait between checking for event batches. </param>
		/// <param name="eventBodyLimitBytes"> The maximum size, in bytes, that the JSON representation of an event may take before it is dropped rather than being sent to the Seq server. Specify null for no limit. The default is 265 KB. </param>
		/// <param name="controlLevelSwitch"> If provided, the switch will be updated based on the Seq server's level setting for the corresponding API key. Passing the same key to MinimumLevel.ControlledBy() will make the whole pipeline dynamically controlled. Do not specify restrictedToMinimumLevel with this setting. </param>
		/// <param name="messageHandler"> Used to construct the HttpClient that will send the log messages to Seq. </param>
		/// <param name="queueSizeLimit"> The maximum number of events that will be held in-memory while waiting to ship them to Seq. Beyond this limit, events will be dropped. The default is 100,000. Has no effect on durable log shipping. </param>
		/// <returns> Logger configuration, allowing configuration to continue. </returns>
		/// <remarks> See https://docs.datalust.co/docs/using-serilog for further information about how to setup serilog for seq. </remarks>
		public static LoggerConfiguration Seq
			(
			this LoggerSinkConfiguration writeTo,
			string seqHost,
			ushort? seqPort,
			string applicationTitle,
			string? configurationApiKey = null,
			bool retryOnError = true,
			LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
			int batchPostingLimit = 1000,
			TimeSpan? period = null,
			long? eventBodyLimitBytes = 262144,
			LoggingLevelSwitch? controlLevelSwitch = null,
			HttpMessageHandler? messageHandler = null,
			int queueSizeLimit = 100000
			)
		{
			// Create the seq sink decorator.
			var (sink, evaluationFunction) = SerilogSeqSinkHelper.CreateSink(seqHost, seqPort, applicationTitle, configurationApiKey, retryOnError, controlLevelSwitch, batchPostingLimit, period, eventBodyLimitBytes, messageHandler, queueSizeLimit);
			if (sink is null || evaluationFunction is null)
			{
				SelfLog.WriteLine($"Could not create a proper instance of an '{nameof(ILogEventSink)}'. Therefore logging to the seq server is not possible. Please see the previous messages for further details.");
				return writeTo.Conditional(_ => false, _ => { });
			}

			// Add the sink to the configuration.
			return writeTo.Conditional(evaluationFunction.Invoke, wt => wt.Sink(sink, restrictedToMinimumLevel));
		}
	}
}