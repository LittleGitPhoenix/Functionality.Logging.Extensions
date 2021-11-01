using System;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft
{
	/// <summary>
	/// Two-way converter for Microsoft.Extensions.Logging.<see cref="LogLevel"/> and Serilog.Events.<see cref="LogEventLevel"/>.
	/// </summary>
	static class LogLevelConverter
	{
		/// <summary>
		/// Convert <paramref name="logLevel"/> to the equivalent Serilog <see cref="LogEventLevel"/>.
		/// </summary>
		/// <param name="logLevel"> A Microsoft.Extensions.Logging.<see cref="LogLevel"/>. </param>
		/// <returns> The Serilog equivalent of <paramref name="logLevel"/>. </returns>
		/// <remarks> The <see cref="LogLevel.None"/> value has no Serilog equivalent. It is mapped to <see cref="LogEventLevel.Fatal"/> as the closest approximation, but this has entirely different semantics. </remarks>
		public static LogEventLevel ToSerilogLevel(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.None:
				case LogLevel.Critical:
					return LogEventLevel.Fatal;
				case LogLevel.Error:
					return LogEventLevel.Error;
				case LogLevel.Warning:
					return LogEventLevel.Warning;
				case LogLevel.Information:
					return LogEventLevel.Information;
				case LogLevel.Debug:
					return LogEventLevel.Debug;
				case LogLevel.Trace:
				default:
					return LogEventLevel.Verbose;
			}
		}

		/// <summary>
		/// Convert <paramref name="logEventLevel"/> to the equivalent Microsoft.Extensions.Logging <see cref="LogLevel"/>.
		/// </summary>
		/// <param name="logEventLevel"> A Serilog.Events.<see cref="LogEventLevel"/>. </param>
		/// <returns> The Microsoft.Extensions.Logging equivalent of <paramref name="logEventLevel"/>. </returns>
		public static LogLevel ToMicrosoftLevel(LogEventLevel logEventLevel)
		{
			switch (logEventLevel)
			{
				case LogEventLevel.Fatal:
					return LogLevel.Critical;
				case LogEventLevel.Error:
					return LogLevel.Error;
				case LogEventLevel.Warning:
					return LogLevel.Warning;
				case LogEventLevel.Information:
					return LogLevel.Information;
				case LogEventLevel.Debug:
					return LogLevel.Debug;
				case LogEventLevel.Verbose:
				default:
					return LogLevel.Trace;
			}
		}
	}
}