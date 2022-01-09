#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Collections.Concurrent;
#if !NETSTANDARD2_0 && !NETSTANDARD1_6 && !NETSTANDARD1_5 && !NETSTANDARD1_4 && !NETSTANDARD1_3 && !NETSTANDARD1_2 && !NETSTANDARD1_1 && !NETSTANDARD1_0
using System.Collections.Immutable;
#endif
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// Special seq sink that keeps buffering <see cref="LogEvent"/>s as long as registering an application with a seq server fails. Those events will be forwarded to an underlying <see cref="ILogEventSink"/> once registering succeeded.
/// </summary>
internal class SeqBufferSink : ILogEventSink, IDisposable
{
    #region Delegates / Events

    #endregion

    #region Constants

    private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(10);

    #endregion

    #region Fields

    private readonly ILogEventSink _otherSink;

    private readonly byte? _retryCount;

    private readonly ConcurrentQueue<LogEvent> _queue;

    private readonly int _queueSizeLimit;

    private readonly SelfLogger _selfLogger;

    private bool _applicationHasBeenRegistered;

    private bool _applicationRegisteringFailed;

    #endregion

    #region Properties

#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
	internal IReadOnlyList<LogEvent> QueuedEvents => _queue.ToArray();
#else
    internal IReadOnlyList<LogEvent> QueuedEvents => _queue.ToImmutableList();
#endif

    #endregion
		
    #region (De)Constructors

    public SeqBufferSink
    (
        SeqServer seqServer,
        string applicationTitle,
        ILogEventSink otherSink,
        byte? retryCount = null,
        int queueSizeLimit = 100000,
        SelfLogger? selfLogger = null
    )
    {
        // Save parameters.
        _otherSink = otherSink;
        _retryCount = retryCount;
        _queueSizeLimit = queueSizeLimit;
        _selfLogger = selfLogger ?? SelfLogger.DefaultSelfLogger;

        // Initialize fields.
        _applicationHasBeenRegistered = false;
        _applicationRegisteringFailed = false;
        _queue = new ConcurrentQueue<LogEvent>();

        this.StartPeriodicApplicationRegistering(seqServer, applicationTitle);
    }

    #endregion

    #region Methods

    private void StartPeriodicApplicationRegistering(SeqServer seqServer, string applicationTitle)
    {
        new Thread
            (
                () =>
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
                            seqServer.RegisterApplication(applicationTitle, cancellationTokenSource.Token);
                            _applicationHasBeenRegistered = true;
                            break;
                        }
                        catch (SeqServerApplicationRegisterException ex)
                        {
                            if (_retryCount is not null && iteration >= _retryCount.Value)
                            {
                                _applicationRegisteringFailed = true;
                                this.ClearQueue();
                                _selfLogger.Log($"Could not register the application '{applicationTitle}' with the seq server '{seqServer.ConnectionData.Url}'. The maximum amount of {_retryCount.Value} retries has been reached. No further attempts will be made.", ex);
                                break;
                            }
								
                            _selfLogger.Log($"Could not register the application '{applicationTitle}' with the seq server '{seqServer.ConnectionData.Url}'. Will be tried again in {WaitTime.TotalMilliseconds}ms seconds.", ex);
                            Thread.Sleep(WaitTime);
                        }
                    }
                    while (true);

                    // Flush the queue.
                    this.FlushQueue();
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

    #region Implementation of ILogEventSink

    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        // If registering ultimately failed, do nothing anymore.
        if (_applicationRegisteringFailed) return;

        if (!_applicationHasBeenRegistered)
        {
            if (this.IsQueueSizeLimitReached()) this.RemoveElementFromQueue();
            _queue.Enqueue(logEvent);
        }
        else
        {
            this.ForwardLogEventToOtherSink(logEvent);
        }
    }

    #endregion

    #region Implementation of IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            (_otherSink as IDisposable)?.Dispose();
        }
        catch (ObjectDisposedException) { /* ignored */ }
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

    internal virtual void ClearQueue()
    {
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
		while (_queue.TryDequeue(out _)) { }
#else
        _queue.Clear();
#endif
    }

	internal virtual void FlushQueue()
	{
		var index = 0;
		while (_queue.TryDequeue(out var logEvent))
		{
			//Console.WriteLine($"{DateTime.Now:hh:mm:ss:ffff}:: Flushing log message #{++index:D2}.");
			_otherSink.Emit(logEvent);
		}
    }

    internal virtual void ForwardLogEventToOtherSink(LogEvent logEvent)
    {
        _otherSink.Emit(logEvent);
    }

    #endregion

    #endregion
}