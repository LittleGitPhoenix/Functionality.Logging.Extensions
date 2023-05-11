#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Functionality.Logging.Base;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog;

/// <summary>
/// No-converter for Serilog.Events.<see cref="LogEventLevel"/>.
/// </summary>
/// <remarks> Can be used via <see cref="SerilogLogLevelConverter.Instance"/>. </remarks>
public class SerilogLogLevelConverter : INoLogLevelConverter<LogEventLevel>
{
	/// <summary>
	/// Singleton instance.
	/// </summary>
	public static SerilogLogLevelConverter Instance => Lazy.Value;
	private static Lazy<SerilogLogLevelConverter> Lazy { get; } = new (() => new SerilogLogLevelConverter(), LazyThreadSafetyMode.ExecutionAndPublication);

	private SerilogLogLevelConverter() { }
	
	#region Implementation of ILogLevelConverter<LogEventLevel,LogLevel>

	/// <inheritdoc />
	public LogEventLevel ConvertSourceToTarget(LogEventLevel sourceLogLevel) => sourceLogLevel;

	/// <inheritdoc />
	public LogEventLevel ConvertTargetToSource(LogEventLevel targetLogLevel) => targetLogLevel;

	#endregion
}