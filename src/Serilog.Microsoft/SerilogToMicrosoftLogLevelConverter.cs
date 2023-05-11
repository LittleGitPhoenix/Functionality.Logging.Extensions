#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

/// <summary>
/// Two-way converter for Microsoft.Extensions.Logging.<see cref="LogLevel"/> and Serilog.Events.<see cref="LogEventLevel"/>.
/// </summary>
/// <remarks> Can be used via <see cref="SerilogToMicrosoftLogLevelConverter.Instance"/>. </remarks>
public class SerilogToMicrosoftLogLevelConverter : ILogLevelConverter<LogEventLevel, LogLevel>
{
	/// <summary>
	/// Singleton instance.
	/// </summary>
	public static SerilogToMicrosoftLogLevelConverter Instance => Lazy.Value;
	private static Lazy<SerilogToMicrosoftLogLevelConverter> Lazy { get; } = new (() => new SerilogToMicrosoftLogLevelConverter(), LazyThreadSafetyMode.ExecutionAndPublication);

	private SerilogToMicrosoftLogLevelConverter() { }

    /// <summary>
    /// Convert <paramref name="logLevel"/> to the equivalent Serilog <see cref="LogEventLevel"/>.
    /// </summary>
    /// <param name="logLevel"> A Microsoft.Extensions.Logging.<see cref="LogLevel"/>. </param>
    /// <returns> The Serilog equivalent of <paramref name="logLevel"/>. </returns>
    /// <remarks> The <see cref="LogLevel.None"/> value has no Serilog equivalent. It is mapped to <see cref="LogEventLevel.Fatal"/> as the closest approximation, but this has entirely different semantics. </remarks>
    public static LogEventLevel ToSerilogLevel(LogLevel logLevel) =>
		logLevel switch
		{
			LogLevel.None => LogEventLevel.Fatal,
			LogLevel.Critical => LogEventLevel.Fatal,
			LogLevel.Error => LogEventLevel.Error,
			LogLevel.Warning => LogEventLevel.Warning,
			LogLevel.Information => LogEventLevel.Information,
			LogLevel.Debug => LogEventLevel.Debug,
			LogLevel.Trace => LogEventLevel.Verbose,
			_ => LogEventLevel.Verbose
		};

	/// <summary>
    /// Convert <paramref name="logEventLevel"/> to the equivalent Microsoft.Extensions.Logging <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="logEventLevel"> A Serilog.Events.<see cref="LogEventLevel"/>. </param>
    /// <returns> The Microsoft.Extensions.Logging equivalent of <paramref name="logEventLevel"/>. </returns>
    public static LogLevel ToMicrosoftLevel(LogEventLevel logEventLevel) =>
		logEventLevel switch
		{
			LogEventLevel.Fatal => LogLevel.Critical,
			LogEventLevel.Error => LogLevel.Error,
			LogEventLevel.Warning => LogLevel.Warning,
			LogEventLevel.Information => LogLevel.Information,
			LogEventLevel.Debug => LogLevel.Debug,
			LogEventLevel.Verbose => LogLevel.Trace,
			_ => LogLevel.Trace
		};

	#region Implementation of ILogLevelConverter<LogEventLevel,LogLevel>

	/// <inheritdoc />
	public LogLevel ConvertSourceToTarget(LogEventLevel sourceLogLevel) => ToMicrosoftLevel(sourceLogLevel);

	/// <inheritdoc />
	public LogEventLevel ConvertTargetToSource(LogLevel targetLogLevel) => ToSerilogLevel(targetLogLevel);

	#endregion
}