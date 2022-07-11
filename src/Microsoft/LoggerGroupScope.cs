#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

internal sealed class LoggerGroupScope : IDisposable
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private int _disposed;

	internal readonly IDictionary<string, object?> _scopes;

	private readonly Action<LoggerGroupScope> _disposedCallback;

	/// <summary>
	/// Needed because a <see cref="ConcurrentDictionary{TKey,TValue}"/> has no thread safe option to delete all its items, which is required in <see cref="Dispose"/>.
	/// </summary>
	private readonly object _disposablesLock;

	internal readonly ConcurrentDictionary<WeakReference<ILogger>, List<IDisposable>> _disposables;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	public LoggerGroupScope(IReadOnlyCollection<ILogger> loggers, IDictionary<string, object?> scopes, Action<LoggerGroupScope> disposedCallback)
	{
		_scopes = scopes;
		_disposedCallback = disposedCallback;
		_disposablesLock = new ();
		_disposables = new ConcurrentDictionary<WeakReference<ILogger>, List<IDisposable>>
		(
			loggers.Select
			(
				logger => new KeyValuePair<WeakReference<ILogger>, List<IDisposable>>(new WeakReference<ILogger>(logger), new List<IDisposable>() {logger.BeginScope(scopes)})
			)
		);
	}

	#endregion

	#region Methods

	public void AddLogger(ILogger logger)
	{
		lock (_disposablesLock)
		{
			if (_disposed == 1) return;

			var disposable = logger.BeginScope(_scopes);
			var loggers = this.CleanLoggers();
			if (loggers.TryGetValue(logger, out var disposables))
			{
				disposables.Add(disposable);
			}
			else
			{
				_disposables.GetOrAdd(new WeakReference<ILogger>(logger), new List<IDisposable>() { disposable });
			}
		}
	}

	public void RemoveLogger(ILogger logger)
	{
		lock (_disposablesLock)
		{
			if (_disposed == 1) return;

			this.CleanLoggers(logger);
			this.TryDisposeThisScope();
		}
	}
	
	/// <summary>
	/// Cleans the weak references by removing loggers that are no longer alive.
	/// </summary>
	/// <param name="loggerToRemove"> Optional <see cref="ILogger"/> that should be removed, even it it is still alive. </param>
	/// <returns> A collection of <see cref="ILogger"/>s that where alive at the time clean-up executed. </returns>
	internal Dictionary<ILogger, ICollection<IDisposable>> CleanLoggers(ILogger? loggerToRemove = null)
	{
		lock (_disposablesLock)
		{
			var activeLoggers = new Dictionary<ILogger, ICollection<IDisposable>>();
			var deadLoggers = new List<WeakReference<ILogger>>();

#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
			foreach (var pair in _disposables)
			{
				var weakLogger = pair.Key;
				var disposables = pair.Value;
#else
			foreach (var (weakLogger, disposables) in _disposables)
			{
#endif
				var isAlive = weakLogger.TryGetTarget(out var logger);
				if (isAlive && logger is not null && !Object.ReferenceEquals(logger, loggerToRemove)) activeLoggers.Add(logger, disposables);
				else deadLoggers.Add(weakLogger);
			}
			deadLoggers.ForEach
			(
				weakLogger =>
				{
					_disposables.TryRemove(weakLogger, out var disposables);
					disposables?.ForEach(this.SaveDispose);
				}
			);
			return activeLoggers;
		}
	}

	/// <summary>
	/// Tries to dispose this instance (thus triggering the <see cref="_disposedCallback"/>) if it no longer contains any disposables.
	/// </summary>
	/// <returns> <b>True</b> on success, otherwise <b>false</b>. </returns>
	private void TryDisposeThisScope()
	{
		lock (_disposablesLock)
		{
			if (_disposables.Count == 0) this.Dispose();
		}
	}

	public void SaveDispose(IDisposable disposable)
	{
		try
		{
			disposable.Dispose();
		}
		catch (ObjectDisposedException) { /* ignore */ }
	}

	/// <inheritdoc />
	public void Dispose()
	{
		lock (_disposablesLock)
		{
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) return;
			
			_disposables
				.SelectMany(pair => pair.Value)
				.ToList()
				.ForEach(this.SaveDispose)
				;
			_disposables.Clear();
			_scopes.Clear();
		
			_disposedCallback.Invoke(this);
		}
	}

#endregion
}