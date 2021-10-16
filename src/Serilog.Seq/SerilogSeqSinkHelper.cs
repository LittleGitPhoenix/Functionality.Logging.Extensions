#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	internal class SerilogSeqSinkHelper
	{
		internal delegate bool EvaluationFunction(LogEvent logEvent);

		/// <summary>
		/// Creates an <see cref="ILogEventSink"/> capable to write to a seq server.
		/// </summary>
		/// <param name="seqHost"> The seq host (e.g. https://localhost or https://localhost:5341). </param>
		/// <param name="seqPort"> An optional port of the seq server. The port can also be specified in <paramref name="seqHost"/>. </param>
		/// <param name="applicationTitle"> The title of the application that will log to seq. </param>
		/// <param name="configurationApiKey"> Api key used for configuration. </param>
		/// <param name="retryOnError"> Should registering the application with the seq server automatically be retried, if it initially failed. </param>
		/// <param name="retryCount"> The amount of attempts made to register the application after the initial one failed. If this is null, then endless attempts will be made. Is only used if <paramref name="retryOnError"/> is true. </param>
		/// <param name="controlLevelSwitch"> If provided, the switch will be updated based on the Seq server's level setting for the corresponding API key. Passing the same key to MinimumLevel.ControlledBy() will make the whole pipeline dynamically controlled. Do not specify restrictedToMinimumLevel with this setting. </param>
		/// <param name="batchPostingLimit"> The maximum number of events to post in a single batch. </param>
		/// <param name="period"> The time to wait between checking for event batches. </param>
		/// <param name="eventBodyLimitBytes"> The maximum size, in bytes, that the JSON representation of an event may take before it is dropped rather than being sent to the Seq server. Specify null for no limit. The default is 265 KB. </param>
		/// <param name="messageHandler"> Used to construct the HttpClient that will send the log messages to Seq. </param>
		/// <param name="queueSizeLimit"> The maximum number of events that will be held in-memory while waiting to ship them to Seq. Beyond this limit, events will be dropped. The default is 5000. Has no effect on durable log shipping. </param>
		/// <returns>
		/// <para> A <see cref="ValueTuple"/> </para>
		/// <para> <see cref="Nullable{ILogEventSink}"/> Sink: The sink capable to emit <see cref="LogEvent"/>s to a seq server. </para>
		/// <para> <see cref="Nullable{EvaluationFunction}"/> EvaluationFunction: The evaluation function that should be used with <see cref="global::Serilog.Configuration.LoggerSinkConfiguration.Conditional"/>. </para>
		/// </returns>
		internal static (ILogEventSink? Sink, EvaluationFunction? EvaluationFunction) CreateSink
		(
			string seqHost,
			ushort? seqPort,
			string applicationTitle,
			string? configurationApiKey = null,
			bool retryOnError = true,
			byte? retryCount = null,
			LoggingLevelSwitch? controlLevelSwitch = null,
			int batchPostingLimit = 1000,
			TimeSpan? period = null,
			long? eventBodyLimitBytes = 256 * 1024,
			HttpMessageHandler? messageHandler = null,
			int queueSizeLimit = 5000
		) =>
			SerilogSeqSinkHelper.CreateSink
			(
				new SeqServer(seqHost, seqPort, configurationApiKey),
				applicationTitle,
				retryOnError,
				retryCount,
				controlLevelSwitch,
				batchPostingLimit,
				period,
				eventBodyLimitBytes,
				messageHandler,
				queueSizeLimit
			);

		/// <summary>
		/// Creates an <see cref="ILogEventSink"/> capable to write to a seq server.
		/// </summary>
		/// <param name="seqServer"> The <see cref="SeqServer"/> used to register the application. </param>
		/// <param name="applicationTitle"> The title of the application that will log to seq. </param>
		/// <param name="retryOnError"> Should registering the application with the seq server automatically be retried, if it initially failed. </param>
		/// <param name="retryCount"> The amount of attempts made to register the application after the initial one failed. If this is null, then endless attempts will be made. Is only used if <paramref name="retryOnError"/> is true. </param>
		/// <param name="controlLevelSwitch"> If provided, the switch will be updated based on the Seq server's level setting for the corresponding API key. Passing the same key to MinimumLevel.ControlledBy() will make the whole pipeline dynamically controlled. Do not specify restrictedToMinimumLevel with this setting. </param>
		/// <param name="batchPostingLimit"> The maximum number of events to post in a single batch. </param>
		/// <param name="period"> The time to wait between checking for event batches. </param>
		/// <param name="eventBodyLimitBytes"> The maximum size, in bytes, that the JSON representation of an event may take before it is dropped rather than being sent to the Seq server. Specify null for no limit. The default is 265 KB. </param>
		/// <param name="messageHandler"> Used to construct the HttpClient that will send the log messages to Seq. </param>
		/// <param name="queueSizeLimit"> The maximum number of events that will be held in-memory while waiting to ship them to Seq. Beyond this limit, events will be dropped. The default is 5000. Has no effect on durable log shipping. </param>
		/// <returns>
		/// <para> A <see cref="ValueTuple"/> </para>
		/// <para> <see cref="Nullable{ILogEventSink}"/> Sink: The sink capable to emit <see cref="LogEvent"/>s to a seq server. </para>
		/// <para> <see cref="Nullable{EvaluationFunction}"/> EvaluationFunction: The evaluation function that should be used with <see cref="global::Serilog.Configuration.LoggerSinkConfiguration.Conditional"/>. </para>
		/// </returns>
		internal static (ILogEventSink? Sink, EvaluationFunction? EvaluationFunction) CreateSink
		(
			SeqServer seqServer,
			string applicationTitle,
			bool retryOnError = true,
			byte? retryCount = null,
			LoggingLevelSwitch? controlLevelSwitch = null,
			int batchPostingLimit = 1000,
			TimeSpan? period = null,
			long? eventBodyLimitBytes = 256 * 1024,
			HttpMessageHandler? messageHandler = null,
			int queueSizeLimit = 5000
		)
		{
			// Directly try to register the token in the seq server.
			string apiKey;
			bool couldRegisterApplication;
			try
			{
				// Automatically cancel the initial attempt to register the application after some seconds if it didn't succeed until then.
				// This helps keeping setup times low in cases where the server may (temporarily) be unavailable.
				using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
				apiKey = seqServer.RegisterApplication(applicationTitle, cancellationTokenSource.Token);
				couldRegisterApplication = true;
			}
			catch (SeqServerApplicationRegisterException ex)
			{
				if (!retryOnError)
				{
					SelfLog.WriteLine($"Could not register the application '{applicationTitle}' with the seq server '{seqServer.Url}'. Since '{nameof(retryOnError)}' is disabled, no logs will be written. Exception was {ex}.");
					return default;
				}
				else
				{
					apiKey = ex.ApiKey;
					couldRegisterApplication = false;
				}
			}

			// Get the seq requirements.
			var couldGetSeqRequirements = SerilogSeqSinkHelper.TryGetSeqRequirements(out var seqSink, out var evaluationFunction, seqServer.Url, apiKey, controlLevelSwitch, eventBodyLimitBytes, messageHandler);
			if (!couldGetSeqRequirements)
			{
				SelfLog.WriteLine($"Could not create the required seq objects.");
				return default;
			}

			// Build the periodic batching sink that wraps the seq sink.
			var periodicBatchingSink = SerilogSeqSinkHelper.GetPeriodicBatchingSink(seqSink!, batchPostingLimit, period, queueSizeLimit);

			if (couldRegisterApplication)
			{
				return (periodicBatchingSink, evaluationFunction);
			}
			else
			{
				return (new SeqBufferSink(seqServer, applicationTitle, periodicBatchingSink, retryCount, queueSizeLimit), evaluationFunction);
			}
		}

#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
		internal static bool TryGetSeqRequirements(out IBatchedLogEventSink? seqSink, out EvaluationFunction? evaluationFunction, string serverUrl, string? apiKey = null, LoggingLevelSwitch? controlLevelSwitch = null, long? eventBodyLimitBytes = 256 * 1024, System.Net.Http.HttpMessageHandler? messageHandler = null)
#else
		internal static bool TryGetSeqRequirements([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IBatchedLogEventSink? seqSink, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EvaluationFunction? evaluationFunction, string serverUrl, string? apiKey = null, LoggingLevelSwitch? controlLevelSwitch = null, long? eventBodyLimitBytes = 256 * 1024, System.Net.Http.HttpMessageHandler? messageHandler = null)
#endif
		{
			evaluationFunction = null;
			seqSink = null;

			var controlledLevelSwitch = SerilogSeqSinkHelper.GetControlledLevelSwitch(controlLevelSwitch);
			if (controlledLevelSwitch is null) return false;
			evaluationFunction = SerilogSeqSinkHelper.GetEvaluationFunction(controlledLevelSwitch!);
			if (evaluationFunction is null) return false;
			seqSink = SerilogSeqSinkHelper.GetSeqSink(controlledLevelSwitch!, serverUrl, apiKey, eventBodyLimitBytes, messageHandler);
			if (seqSink is null) return false;

			return true;
		}

		private static object? GetControlledLevelSwitch(LoggingLevelSwitch? controlLevelSwitch = null)
		{
			//! This has to be done via reflection, as the 'ControlledLevelSwitch' class is internal and is normally only used via the 'LoggerSinkConfiguration.Seq' extension method.
			var controlledLevelSwitchClassName = "Serilog.Sinks.Seq.ControlledLevelSwitch";

			try
			{
				// Get the seq assembly.
				var seqAssembly = typeof(SeqLoggerConfigurationExtensions).Assembly;

				// Get and build the ControlledLevelSwitch class.
				var controlledLevelSwitchType = seqAssembly.GetType(controlledLevelSwitchClassName);
				var controlledLevelSwitchConstructorParameterTypes = new Type[]
				{
					typeof(LoggingLevelSwitch)
				};
				var controlledLevelSwitchConstructor = controlledLevelSwitchType?.GetConstructor(controlledLevelSwitchConstructorParameterTypes);
				if (controlledLevelSwitchType is null || controlledLevelSwitchConstructor is null)
				{
					SelfLog.WriteLine($"Could not get the constructor of the '{controlledLevelSwitchClassName}' class needed to build the sink.");
					return null;
				}
				var controlledLevelSwitch = controlledLevelSwitchConstructor.Invoke(new object?[] { controlLevelSwitch });
				return controlledLevelSwitch;
			}
			catch (Exception ex)
			{
				SelfLog.WriteLine($"An error occurred while getting the constructor of the '{controlledLevelSwitchClassName}' class needed to build the sink. ({ex})");
				return null;
			}
		}

		private static EvaluationFunction? GetEvaluationFunction(object controlledLevelSwitch)
		{
			//! This has to be done via reflection, as the 'ControlledLevelSwitch' class is internal and is normally only used via the 'LoggerSinkConfiguration.Seq' extension method.
			var evaluationFunctionName = "IsIncluded";

			try
			{
				// Get and build the IsIncluded function.
				var evaluationFunctionParameterTypes = new Type[]
				{
					typeof(LogEvent)
				};
				var controlledLevelSwitchType = controlledLevelSwitch.GetType();
				var evaluationFunctionType = controlledLevelSwitchType.GetMethod(evaluationFunctionName, evaluationFunctionParameterTypes);
				if (evaluationFunctionType is null)
				{
					SelfLog.WriteLine($"Could not get the evaluation function '{evaluationFunctionName}' class needed to operate the sink.");
					return null;
				}

				bool EvaluationFunction(LogEvent logEvent)
				{
					var result = evaluationFunctionType.Invoke(controlledLevelSwitch, new object[] { logEvent });
					return (bool?)result ?? false;
				}

				return EvaluationFunction;
			}
			catch (Exception ex)
			{
				SelfLog.WriteLine($"An error occurred while getting the evaluation function '{evaluationFunctionName}' class needed to operate the sink. ({ex})");
				return null;
			}
		}

		private static IBatchedLogEventSink? GetSeqSink(object controlledLevelSwitch, string serverUrl, string? apiKey = null, long? eventBodyLimitBytes = 256 * 1024, System.Net.Http.HttpMessageHandler? messageHandler = null)
		{
			//! This has to be done via reflection, as the 'SeqSink' class is internal and is normally only used via the 'LoggerSinkConfiguration.Seq' extension method.
			var seqSinkClassName = "Serilog.Sinks.Seq.SeqSink";

			try
			{
				// Get the seq assembly.
				var seqAssembly = typeof(SeqLoggerConfigurationExtensions).Assembly;

				// Get and build the SeqSink class.
				var seqSinkConstructorParameterTypes = new Type[]
				{
					typeof(string),
					typeof(string),
					typeof(long?),
					controlledLevelSwitch.GetType(),
					typeof(HttpMessageHandler)
				};

				var seqSinkType = seqAssembly.GetType(seqSinkClassName);
				var seqSinkConstructor = seqSinkType?.GetConstructor(seqSinkConstructorParameterTypes);
				if (seqSinkType is null || seqSinkConstructor is null)
				{
					SelfLog.WriteLine($"Could not get the constructor of the '{seqSinkClassName}' class needed to build the sink.");
					return null;
				}
				var seqSink = seqSinkConstructor.Invoke(new object?[] { serverUrl, apiKey, eventBodyLimitBytes, controlledLevelSwitch, messageHandler });

				return (IBatchedLogEventSink)seqSink;
			}
			catch (Exception ex)
			{
				SelfLog.WriteLine($"An error occurred while getting the constructor of the '{seqSinkClassName}' class needed to build the sink. ({ex})");
				return null;
			}
		}

		internal static ILogEventSink GetPeriodicBatchingSink(IBatchedLogEventSink seqSink, int batchPostingLimit = 1000, TimeSpan? period = null, int queueSizeLimit = 100000)
		{
			// Build a periodic batching sync as wrapper.
			var options = new PeriodicBatchingSinkOptions
			{
				BatchSizeLimit = batchPostingLimit,
				Period = period ?? TimeSpan.FromSeconds(2),
				QueueLimit = queueSizeLimit
			};
			return new PeriodicBatchingSink(seqSink, options);
		}
	}
}