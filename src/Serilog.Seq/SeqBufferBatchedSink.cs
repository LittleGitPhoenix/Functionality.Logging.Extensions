using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

internal class SeqBufferBatchedSink : IBatchedLogEventSink, IDisposable
{
	#region Delegates / Events
	#endregion

	#region Constants

	private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(10);

	#endregion

	#region Fields

	private readonly IBatchedLogEventSink _underlyingBatchedSink;

	private readonly byte? _retryCount;

	private readonly ConcurrentQueue<LogEvent> _queue;

	private readonly int _queueSizeLimit;

	private readonly SelfLogger _selfLogger;

	private readonly SemaphoreSlim _transitionLock;

	private volatile bool _applicationHasBeenRegistered;

	private volatile bool _applicationRegisteringFailed;

	#endregion

	#region Properties

#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
	internal IReadOnlyList<LogEvent> QueuedEvents => _queue.ToArray();
#else
	internal IReadOnlyList<LogEvent> QueuedEvents => System.Collections.Immutable.ImmutableList.CreateRange(_queue);
#endif

	#endregion

	#region (De)Constructors

	public SeqBufferBatchedSink
	(
		SeqServer seqServer,
		string applicationTitle,
		IBatchedLogEventSink underlyingBatchedSink,
		byte? retryCount = null,
		int queueSizeLimit = 100000,
		SelfLogger? selfLogger = null
	)
	{
		// Save parameters.
		_underlyingBatchedSink = underlyingBatchedSink;
		_retryCount = retryCount;
		_queueSizeLimit = queueSizeLimit;
		_selfLogger = selfLogger ?? SelfLogger.DefaultSelfLogger;

		// Initialize fields.
		_applicationHasBeenRegistered = false;
		_applicationRegisteringFailed = false;
		_queue = new ConcurrentQueue<LogEvent>();
		_transitionLock = new SemaphoreSlim(1, 1);

		this.StartPeriodicApplicationRegistering(seqServer, applicationTitle);
	}

	#endregion

	#region Methods

	private void StartPeriodicApplicationRegistering(SeqServer seqServer, string applicationTitle)
	{
		new Thread
			(
				async void () =>
				{
					try
					{
						// Endlessly try to register the application with the seq server.
						var iteration = 0;
						do
						{
							try
							{
								iteration++;

								// Automatically cancel the attempt to register the application after some seconds if it didn't succeed until then.
								using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
								await seqServer.RegisterApplicationAsync(applicationTitle, cancellationTokenSource.Token);

								// Atomically flush the buffered queue and mark the application as registered, so that no new log events are enqueued between the flush and the flag being set.
								await _transitionLock.WaitAsync();
								try
								{
									await this.FlushQueueAsync();
									_applicationHasBeenRegistered = true;
								}
								finally
								{
									_transitionLock.Release();
								}
								break;
							}
							catch (SeqServerApplicationRegisterException ex)
							{
								if (iteration >= _retryCount)
								{
									// Atomically clear the queue and mark registration as failed, so that no new log events are enqueued between the clear and the flag being set.
									await _transitionLock.WaitAsync();
									try
									{
										this.ClearQueue();
										_applicationRegisteringFailed = true;
									}
									finally
									{
										_transitionLock.Release();
									}
									_selfLogger.Log($"Could not register the application '{applicationTitle}' with the seq server '{seqServer.ConnectionData.Url}'. The maximum amount of {_retryCount.Value} retries has been reached. No further attempts will be made.", ex);
									break;
								}

								_selfLogger.Log($"Could not register the application '{applicationTitle}' with the seq server '{seqServer.ConnectionData.Url}'. Will be tried again in {WaitTime.TotalMilliseconds}ms seconds.", ex);
								Thread.Sleep(WaitTime);
							}
						}
						while (true);
					}
					catch (Exception ex)
					{
						_selfLogger.Log($"The application '{applicationTitle}' could not be registered with the seq server '{seqServer.ConnectionData.Url}' due to an unexpected exception. No further attempts will be made.", ex);
					}
				}
			)
			{
				Name = "Seq registration thread",
				IsBackground = true,
				Priority = ThreadPriority.BelowNormal,
			}
			.Start()
			;
	}

	#region Implementation of IBatchedLogEventSink

	/// <inheritdoc />
	public Task OnEmptyBatchAsync() => Task.CompletedTask;

	/// <inheritdoc />
	public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
	{
		// Fast path: If registering ultimately failed, do nothing anymore.
		if (_applicationRegisteringFailed) return;

		// Fast path: If the application is already registered, forward directly without acquiring the lock.
		if (_applicationHasBeenRegistered)
		{
			await this.ForwardLogEventsAsync(batch);
			return;
		}

		await _transitionLock.WaitAsync();
		try
		{
			// Re-check inside the lock to avoid a race with the registration thread.
			if (_applicationRegisteringFailed) return;

			if (!_applicationHasBeenRegistered)
			{
				if (this.IsQueueSizeLimitReached()) this.RemoveElementFromQueue();
				foreach (var logEvent in batch) _queue.Enqueue(logEvent);
			}
			else
			{
				await this.ForwardLogEventsAsync(batch);
			}
		}
		finally
		{
			_transitionLock.Release();
		}
	}

	#endregion

	#region Helper

	internal virtual bool IsQueueSizeLimitReached()
	{
		return _queue.Count >= _queueSizeLimit;
	}

	internal virtual void RemoveElementFromQueue()
	{
		_queue.TryDequeue(out _);
	}

	internal virtual async Task FlushQueueAsync()
	{
		if (!_queue.IsEmpty)
		{
			await _underlyingBatchedSink.EmitBatchAsync(_queue);
		}
		this.ClearQueue();
	}

	internal virtual void ClearQueue()
	{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
		while (_queue.TryDequeue(out _)) { }
#else
		_queue.Clear();
#endif
	}

	internal virtual Task ForwardLogEventsAsync(IReadOnlyCollection<LogEvent> batch) => _underlyingBatchedSink.EmitBatchAsync(batch);

	#endregion

	#region Implementation of IDisposable

	/// <inheritdoc />
	public void Dispose()
	{
		try
		{
			(_underlyingBatchedSink as IDisposable)?.Dispose();
		}
		catch (ObjectDisposedException) { /* ignored */ }

		try
		{
			_transitionLock.Dispose();
		}
		catch (ObjectDisposedException) { /* ignored */ }
	}

	#endregion

	#endregion
}