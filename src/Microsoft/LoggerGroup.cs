#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

internal sealed class LoggerGroup : ILoggerGroup
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly List<WeakReference<ILogger>> _loggers;
	
	private readonly object _loggersLock;

	private readonly List<LoggerGroupScope> _groupScopes;

	private readonly object _groupScopesLock;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Null-object constructor
	/// </summary>
	internal LoggerGroup()
	{
		// Initialize fields
		_loggers = new();
		_loggersLock = new();
		_groupScopes = new();
		_groupScopesLock = new();
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="logger"> The initial logger. </param>
	internal LoggerGroup(ILogger logger)
		: this()
	{
		_loggers.Add(new WeakReference<ILogger>(logger));
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void AddLogger(ILogger logger, bool applyExistingScope)
	{
		lock (_loggersLock)
		{
			var loggers = this.CleanLoggers();
			if (loggers.Contains(logger)) return;
			_loggers.Add(new WeakReference<ILogger>(logger));
		}
		if (applyExistingScope) lock (_groupScopesLock) _groupScopes.ForEach(groupScope => groupScope.AddLogger(logger));
	}

	/// <inheritdoc />
	public void RemoveLogger(ILogger logger)
	{
		lock (_loggersLock)
		{
			this.CleanLoggers(logger);
			_groupScopes.ForEach(scope => scope.RemoveLogger(logger));
		}
	}

	/// <summary>
	/// Cleans the weak references by removing loggers that are no longer alive.
	/// </summary>
	/// <param name="loggerToRemove"> Optional <see cref="ILogger"/> that should be removed, even it it is still alive. </param>
	/// <returns> A collection of <see cref="ILogger"/>s that where alive at the time clean-up executed. </returns>
	private IReadOnlyCollection<ILogger> CleanLoggers(ILogger? loggerToRemove = null)
	{
		lock (_loggersLock)
		{
			var activeLoggers = new List<ILogger>();
			var deadLoggers = new List<WeakReference<ILogger>>();
			foreach (var weakLogger in _loggers)
			{
				var isAlive = weakLogger.TryGetTarget(out var logger);
				if (isAlive && logger is not null && !Object.ReferenceEquals(logger, loggerToRemove)) activeLoggers.Add(logger);
				else deadLoggers.Add(weakLogger);
			}
			deadLoggers.ForEach
			(
				weakLogger =>
				{
					_loggers.Remove(weakLogger);
					lock (_groupScopesLock) _groupScopes.ForEach(scope => scope.CleanLoggers());
				}
			);
			return activeLoggers;
		}
	}

	/// <inheritdoc />
	public IDisposable CreateScope(LogScope scope)
	{
		return this.CreateScope((IDictionary<string, object?>) scope);
	}

	/// <inheritdoc />
	public IDisposable CreateScope(params (string Identifier, object? Value)[] scopedValues)
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary(scopedValues);
		return this.CreateScope(scopes);
	}

	/// <inheritdoc />
	public IDisposable CreateScope(params Expression<Func<object>>[] scopedValues)
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary(scopedValues);
		return this.CreateScope(scopes);
	}

#if NETCOREAPP3_0_OR_GREATER

	/// <inheritdoc />
	public IDisposable CreateScope
	(
		object? value1,
		object? value2 = default,
		object? value3 = default,
		object? value4 = default,
		object? value5 = default,
		object? value6 = default,
		object? value7 = default,
		object? value8 = default,
		object? value9 = default,
		object? value10 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default,
		bool cleanCallerArgument = true
	)
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		);
		return this.CreateScope(scopes);
	}

#endif

	private IDisposable CreateScope(IDictionary<string, object?> scopes)
	{
		lock (_loggersLock)
		{
			var loggers = this.CleanLoggers();
			var groupScope = new LoggerGroupScope(loggers, scopes, disposedCallback: this.RemoveLoggerGroupScope);
			lock (_groupScopesLock) _groupScopes.Add(groupScope);
			return groupScope;
		}
	}

	private void RemoveLoggerGroupScope(LoggerGroupScope groupScope)
	{
		lock (_groupScopesLock) _groupScopes.Remove(groupScope);
	}

	#region IReadOnlyCollection

	/// <inheritdoc />
	public IEnumerator<ILogger> GetEnumerator() => this.CleanLoggers().GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	
	/// <inheritdoc />
	public int Count => this.CleanLoggers().Count;
	
	/// <inheritdoc />
	public int Length => this.Count;
	
	#endregion

	#region IDisposable

	/// <inheritdoc />
	public void Dispose()
	{
		lock (_groupScopesLock)
		{
			_groupScopes.ForEach(scope => scope.Dispose());
			_groupScopes.Clear();
		}
	}

	#endregion
	
	#endregion
}