#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// A combination of <see cref="ILogger"/> and <see cref="IDisposable"/> used to chain log scopes while still allowing proper disposal of all created scopes as well as continued logging.
/// </summary>
/// <remarks> ⚠️ This interface is not intended to be implemented externally. </remarks>
public interface IChainingLogScopeDisposable : IDisposable, ILogger;

internal class ChainingLogScopeDisposable : IChainingLogScopeDisposable
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly IDisposable _scope;

	private readonly IDisposable? _nestedScope;

	#endregion

	#region Properties

	internal ILogger Logger { get; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor using <see cref="NoDisposable.Instance"/> as scope.
	/// </summary>
	/// <param name="logger"> The logger to associate with this scope. If the <paramref name="logger"/> is already a <see cref="ChainingLogScopeDisposable"/>, its inner logger will be used so that always the original logger persists. Cannot be <see langword="null"/>. </param>
	public ChainingLogScopeDisposable(ILogger logger)
		: this(logger, NoDisposable.Instance) { }

	/// <summary>
	/// Constructor that ensures, that the innermost logger is always available via the <see cref="Logger"/> property and that nested scopes are also properly disposed.
	/// </summary>
	/// <remarks>
	/// This constructor unwraps nested ChainingLogScopeDisposable instances to ensure that the actual underlying logger is always used, even when scopes are nested.
	/// This allows consistent access to the original logger type and behavior across multiple nested scopes.
	/// Furthermore, it ensures that all nested scopes are properly disposed of when the outermost scope is disposed.
	/// </remarks>
	/// <param name="logger"> The logger to associate with this scope. If the <paramref name="logger"/> is already a <see cref="ChainingLogScopeDisposable"/>, its inner logger will be used so that always the original logger persists. Cannot be <see langword="null"/>. </param>
	/// <param name="scope"> The disposable logging scope to manage. Cannot be <see langword="null"/>. </param>
	public ChainingLogScopeDisposable(ILogger logger, IDisposable scope)
	{
		// If the logger is already wrapped within a chaining logger, use the inner logger.
		// This way the actual logger (and its type) is always available even if a ChainingLogScopeDisposable is nested multiple times.
		var chainingLogger = logger as ChainingLogScopeDisposable;

		this.Logger = chainingLogger?.Logger ?? logger;
		_scope = scope;
		_nestedScope = chainingLogger;
	}

	#endregion

	#region Methods
	
	public void Dispose()
	{
		_scope.Dispose();
		_nestedScope?.Dispose();
	}

	#region ILogger Implementation

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this.Logger.BeginScope(state);

	public bool IsEnabled(LogLevel logLevel) => this.Logger.IsEnabled(logLevel);
	
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => this.Logger.Log(logLevel, eventId, state, exception, formatter);

	#endregion

	#endregion
}