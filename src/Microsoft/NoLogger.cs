#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Null-object <see cref="ILogger"/> accessible via <see cref="NoLogger.Instance"/>.
/// </summary>
[Obsolete($"Please use {nameof(global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance)} instead. The 'Instance' property now only forwards to this anyway.")]
public class NoLogger : ILogger
{
	/// <inheritdoc cref="global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance" />
	public static ILogger Instance => global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel) => false;

	/// <inheritdoc />
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NoDisposable.Instance;

	private NoLogger() { }
}