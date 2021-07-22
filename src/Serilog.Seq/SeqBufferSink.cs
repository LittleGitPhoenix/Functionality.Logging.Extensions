#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
#if !NETSTANDARD2_0 && !NETSTANDARD1_6 && !NETSTANDARD1_5 && !NETSTANDARD1_4 && !NETSTANDARD1_3 && !NETSTANDARD1_2 && !NETSTANDARD1_1 && !NETSTANDARD1_0
using System.Collections.Immutable;
#endif
using System.Threading;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Special seq sink that keeps buffering <see cref="LogEvent"/>s as long as registering an application with a seq server fails. Those events will be forwarded to an underlying <see cref="ILogEventSink"/> once registering succeeded.
	/// </summary>
	internal class SeqBufferSink : ILogEventSink, IDisposable
	{
		#region Delegates / Events
		#endregion

		#region Constants

		private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(5);

		#endregion

		#region Fields
		
		private readonly ILogEventSink _otherSink;
		
		private readonly ConcurrentQueue<LogEvent> _queue;

		private readonly int _queueSizeLimit;
		
		private bool _applicationHasBeenRegistered;

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
			string apiKey,
			ILogEventSink otherSink,
			int queueSizeLimit = 100000
		)
		{
			// Save parameters.
			_otherSink = otherSink;
			_queueSizeLimit = queueSizeLimit;

			// Initialize fields.
			_applicationHasBeenRegistered = false;
			_queue = new ConcurrentQueue<LogEvent>();

			this.StartPeriodicApplicationRegistering(seqServer, applicationTitle, apiKey);
		}

#endregion

#region Methods

		private void StartPeriodicApplicationRegistering(SeqServer seqServer, string applicationTitle, string apiKey)
		{
			// Directly try to register the application to prevent creating a new thread if not necessary.
			_applicationHasBeenRegistered = seqServer.RegisterApplicationAsync(applicationTitle, apiKey).Result;
			if (_applicationHasBeenRegistered) return;

			new Thread
				(
					() =>
					{
						// Endlessly try to register the application with the seq server.
						do
						{
							_applicationHasBeenRegistered = seqServer.RegisterApplicationAsync(applicationTitle, apiKey).Result;
							if (!_applicationHasBeenRegistered)
							{
								SelfLog.WriteLine($"Could not register the application '{applicationTitle}' with api key '{apiKey}' with the seq server '{seqServer.Url}'. Will be tried again in {WaitTime.TotalMilliseconds}ms seconds.");
								Thread.Sleep(WaitTime);
							}
						}
						while (!_applicationHasBeenRegistered);


						// Flush the queue.
						while (_queue.TryDequeue(out var logEvent))
						{
							_otherSink.Emit(logEvent);
						}
					}
				)
				{
					Name = "Application register thread",
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

		internal virtual void ForwardLogEventToOtherSink(LogEvent logEvent)
		{
			_otherSink.Emit(logEvent);
		}

		#endregion

		#endregion
	}
}