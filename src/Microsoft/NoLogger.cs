#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Null-object <see cref="ILogger"/> accessible via <see cref="NoLogger.Instance"/>.
/// </summary>
public class NoLogger : ILogger
{
	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel) => false;

	/// <inheritdoc />
	public IDisposable BeginScope<TState>(TState state) => NoDisposable.Instance;

	/// <summary>
	/// Singleton instance.
	/// </summary>
	public static ILogger Instance => LazyLogger.Value;
	private static readonly Lazy<ILogger> LazyLogger = new(() => new NoLogger(), LazyThreadSafetyMode.ExecutionAndPublication);

	private NoLogger() { }
}