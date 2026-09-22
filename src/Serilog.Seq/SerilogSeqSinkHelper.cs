#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Seq;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

internal class SerilogSeqSinkHelper
{
    internal delegate bool EvaluationFunction(LogEvent logEvent);
	
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
    internal static bool TryGetSeqRequirements(out IBatchedLogEventSink? seqSink, out EvaluationFunction? evaluationFunction, SelfLogger selfLogger, string serverUrl, string? apiKey = null, ITextFormatter? payloadFormatter = null, LoggingLevelSwitch? controlLevelSwitch = null, long? eventBodyLimitBytes = 256 * 1024, System.Net.Http.HttpMessageHandler? messageHandler = null)
#else
	internal static bool TryGetSeqRequirements([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IBatchedLogEventSink? seqSink, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EvaluationFunction? evaluationFunction, SelfLogger selfLogger, string serverUrl, string? apiKey = null, ITextFormatter? payloadFormatter = null, LoggingLevelSwitch? controlLevelSwitch = null, long? eventBodyLimitBytes = 256 * 1024, System.Net.Http.HttpMessageHandler? messageHandler = null)
#endif
    {
        evaluationFunction = null;
        seqSink = null;

        var controlledLevelSwitch = GetControlledLevelSwitch(selfLogger, controlLevelSwitch);
        if (controlledLevelSwitch is null) return false;
        
		evaluationFunction = GetEvaluationFunction(selfLogger, controlledLevelSwitch);
        if (evaluationFunction is null) return false;
        
		var ingestionApi = GetIngestionApiClient(selfLogger, serverUrl, apiKey, messageHandler);
        if (ingestionApi is null) return false;
        
		seqSink = GetSeqSink(selfLogger, ingestionApi, payloadFormatter ?? new SeqCompactJsonFormatter(), controlledLevelSwitch, eventBodyLimitBytes);
        if (seqSink is null) return false;

        return true;
    }

    private static object? GetControlledLevelSwitch(SelfLogger selfLogger, LoggingLevelSwitch? controlLevelSwitch = null)
    {
        //! This has to be done via reflection, as the 'ControlledLevelSwitch' class is internal and is normally only used via the 'LoggerSinkConfiguration.Seq' extension method.
        var className = "Serilog.Sinks.Seq.ControlledLevelSwitch";

        try
        {
            // Get the seq assembly.
            var seqAssembly = typeof(SeqLoggerConfigurationExtensions).Assembly;

            // Get and build the ControlledLevelSwitch class.
            var parameterTypes = new[]
            {
                typeof(LoggingLevelSwitch)
            };
            var type = seqAssembly.GetType(className);
            var constructor = type?.GetConstructor(parameterTypes);
            if (type is null || constructor is null)
            {
                selfLogger.Log($"Could not get the constructor of the '{className}' class needed to build the sink.");
                return null;
            }
            var instance = constructor.Invoke([controlLevelSwitch]);
            return instance;
        }
        catch (Exception ex)
        {
            selfLogger.Log($"An error occurred while getting the constructor of the '{className}' class needed to build the sink.", ex);
            return null;
        }
    }

    private static EvaluationFunction? GetEvaluationFunction(SelfLogger selfLogger, object controlledLevelSwitch)
    {
        //! This has to be done via reflection, as the 'ControlledLevelSwitch' class is internal and is normally only used via the 'LoggerSinkConfiguration.Seq' extension method.
        var functionName = "IsIncluded";

        try
        {
            // Get and build the IsIncluded function.
            var parameterTypes = new[]
            {
                typeof(LogEvent)
            };
            var type = controlledLevelSwitch.GetType();
            var function = type.GetMethod(functionName, parameterTypes);
            if (function is null)
            {
                selfLogger.Log($"Could not get the evaluation function '{functionName}' class needed to operate the sink.");
                return null;
            }

            bool EvaluationFunction(LogEvent logEvent)
            {
                var result = function.Invoke(controlledLevelSwitch, [logEvent]);
                return (bool?) result ?? false;
            }

            return EvaluationFunction;
        }
        catch (Exception ex)
        {
            selfLogger.Log($"An error occurred while getting the evaluation function '{functionName}' class needed to operate the sink.", ex);
            return null;
        }
    }

    private static object? GetIngestionApiClient(SelfLogger selfLogger, string serverUrl, string? apiKey = null, HttpMessageHandler? messageHandler = null)
    {
        //! This has to be done via reflection, as the 'SeqIngestionApiClient' class is internal.
        var className = "Serilog.Sinks.Seq.Http.SeqIngestionApiClient";

        try
        {
            // Get the seq assembly.
            var seqAssembly = typeof(SeqLoggerConfigurationExtensions).Assembly;

            // Get and build the SeqSink class.
            var parameterTypes = new[]
            {
                serverUrl.GetType(),
                apiKey?.GetType() ?? typeof(string),
                messageHandler?.GetType() ?? typeof(HttpMessageHandler),
            };

            var type = seqAssembly.GetType(className);
            var constructor = type?.GetConstructor(parameterTypes);
            if (type is null || constructor is null)
            {
                selfLogger.Log($"Could not get the constructor of the '{className}' class needed to build the sink.");
                return null;
            }
            var instance = constructor.Invoke([serverUrl, apiKey, messageHandler]);
            return instance;
        }
        catch (Exception ex)
        {
            selfLogger.Log($"An error occurred while getting the constructor of the '{className}' class needed to build the sink.", ex);
            return null;
        }
    }

    private static IBatchedLogEventSink? GetSeqSink(SelfLogger selfLogger, object ingestionApi, object payloadFormatter, object controlledLevelSwitch, long? eventBodyLimitBytes = 256 * 1024)
    {
        //! This has to be done via reflection, as the 'BatchedSeqSink' class is internal.
        var className = "Serilog.Sinks.Seq.Batched.BatchedSeqSink";

        try
        {
            // Get the seq assembly.
            var seqAssembly = typeof(SeqLoggerConfigurationExtensions).Assembly;

            // Get and build the SeqSink class.
            var parameterTypes = new[]
            {
                ingestionApi.GetType(),
				payloadFormatter.GetType(),
				eventBodyLimitBytes?.GetType() ?? typeof(long?),
                controlledLevelSwitch.GetType(),
            };

            var type = seqAssembly.GetType(className);
            var constructor = type?.GetConstructor(parameterTypes);
            if (type is null || constructor is null)
            {
                selfLogger.Log($"Could not get the constructor of the '{className}' class needed to build the sink.");
                return null;
            }
            var seqSink = constructor.Invoke([ingestionApi, payloadFormatter, eventBodyLimitBytes, controlledLevelSwitch]);
            return (IBatchedLogEventSink) seqSink;
        }
        catch (Exception ex)
        {
            selfLogger.Log($"An error occurred while getting the constructor of the '{className}' class needed to build the sink.", ex);
            return null;
        }
    }
}