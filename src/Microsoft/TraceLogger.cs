using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Trace and if available console <see cref="ILogger"/>.
/// </summary>
/// <remarks> Can be used in unit tests via the static <see cref="TraceLogger.Instance"/> property. </remarks>
public class TraceLogger : ILogger
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	
	private readonly TextWriter? _consoleOutput;

	#endregion

	#region Properties

	/// <summary>
	/// Singleton instance.
	/// </summary>
	public static ILogger Instance => LazyLogger.Value;
	private static readonly Lazy<ILogger> LazyLogger = new(() => new TraceLogger(), LazyThreadSafetyMode.ExecutionAndPublication);

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Static Constructor
	/// </summary>
	public TraceLogger()
	{
		// Save parameters.
		
		// Initialize fields.
		_consoleOutput = GetConsoleOutput();
	}

	#endregion

	#region Methods

	#region ILogger

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (logLevel == LogLevel.None) return;

		// Create the formatted message.
		var message = formatter.Invoke(state, exception);
		if (String.IsNullOrWhiteSpace(message)) return;

		this.TraceLog(message);
		this.ConsoleLog(message, logLevel);
	}

	internal virtual void TraceLog(string message)
	{
		// Log via trace.
		Trace.WriteLine(message);
	}

	internal virtual void ConsoleLog(string message, LogLevel logLevel)
	{
		if (_consoleOutput == null) return;
		
		// Log to console.
		var color = Console.ForegroundColor;
		try
		{
			if (logLevel == LogLevel.Trace) Console.ForegroundColor = ConsoleColor.DarkGray;
			if (logLevel == LogLevel.Debug) Console.ForegroundColor = ConsoleColor.DarkGray;
			if (logLevel == LogLevel.Information) Console.ForegroundColor = ConsoleColor.White;
			if (logLevel == LogLevel.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
			if (logLevel == LogLevel.Error) Console.ForegroundColor = ConsoleColor.Red;
			if (logLevel == LogLevel.Critical) Console.ForegroundColor = ConsoleColor.Red;
			_consoleOutput.WriteLine(message);
		}
		finally
		{
			Console.ForegroundColor = color;
		}
	}

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel) => true;

	/// <inheritdoc />
	public IDisposable BeginScope<TState>(TState state) => NoDisposable.Instance;

	#endregion

	#region Helper
	
	/// <summary>
	/// Checks if <see cref="Console"/> output is available.
	/// </summary>
	/// <returns> The <see cref="Console.Out"/> <see cref="TextWriter"/>. </returns>
	/// <remarks> This is based on <c>NLog.Targets.ConsoleTargetHelper.IsConsoleAvailable</c>. </remarks>
	private static TextWriter? GetConsoleOutput()
	{
		try
		{
			if (!Environment.UserInteractive)
			{
				// Extra bonus check for Mono, that doesn't support Environment.UserInteractive
				var isMono = Type.GetType("Mono.Runtime") != null;
				if (isMono && Console.In is System.IO.StreamReader) return Console.Out;

				return null;
			}
			else if (Console.OpenStandardInput(1) == System.IO.Stream.Null)
			{
				return null;
			}
		}
		catch (Exception)
		{
			return null;
		}

		return Console.Out;
	}
	
	#endregion

	#endregion
}