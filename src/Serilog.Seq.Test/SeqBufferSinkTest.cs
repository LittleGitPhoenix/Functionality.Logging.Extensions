using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;
using Seq.Api;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Seq.Test
{
	public class SeqBufferSinkTest
	{
#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
		private IFixture _fixture;
#pragma warning restore 8618
		
		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
		}

		[Test]
		public void Check_Log_Events_Are_Directly_Emitted_If_Application_Was_Registered()
		{
			// Arrange
			var logEvents = _fixture.CreateMany<LogEvent>(10).ToArray();
			var mockSink = Mock.Of<ILogEventSink>();
			Mock.Get(mockSink).Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Verifiable();
			_fixture.Inject(mockSink);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
			_fixture.Inject(seqServerMock.Object);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Act
			foreach (var logEvent in logEvents) bufferSink.Emit(logEvent);

			// Assert
			Assert.That(bufferSink.QueuedEvents, Is.Empty);
			Mock.Get(mockSink).Verify(sink => sink.Emit(It.IsAny<LogEvent>()), Times.Exactly(logEvents.Length));
		}

		[Test]
		public void Check_Log_Events_Are_Buffered_As_Long_As_Application_Is_Not_Registered()
		{
			// Arrange
			var logEvents = _fixture.CreateMany<LogEvent>(10).ToArray();
			var mockSink = Mock.Of<ILogEventSink>();
			Mock.Get(mockSink).Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Verifiable();
			_fixture.Inject(mockSink);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));
			_fixture.Inject(seqServerMock.Object);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Act
			foreach (var logEvent in logEvents) {bufferSink.Emit(logEvent);}

			// Assert
			Assert.AreEqual(bufferSink.QueuedEvents, logEvents);
			Mock.Get(mockSink).Verify(sink => sink.Emit(It.IsAny<LogEvent>()), Times.Never);
		}

		[Test]
		public void Check_Buffered_Log_Events_Are_Discarded_If_Limit_Is_Reached()
		{
			// Arrange
			var sizeLimit = 5;
			var logEvents = _fixture.CreateMany<LogEvent>(sizeLimit * 2).ToArray();
			_fixture.Inject(sizeLimit);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));
			_fixture.Inject(seqServerMock.Object);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Act
			foreach (var logEvent in logEvents) bufferSink.Emit(logEvent);

			// Assert
			Assert.AreEqual(bufferSink.QueuedEvents, logEvents.Skip(sizeLimit));
		}

		[Test]
		public async Task Check_Log_Events_Are_Flushed_When_Application_Was_Registered()
		{
			// Arrange
			var emittedLogEventCount = 0;
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;
			var mockSink = Mock.Of<ILogEventSink>();
			Mock.Get(mockSink)
				.Setup(sink => sink.Emit(It.IsAny<LogEvent>()))
				.Verifiable()
				;
			_fixture.Inject(mockSink);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock
				.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<string>()))
				.Returns
				(
					() =>
					{
						//! Let application registering fail until a certain amount of queued log events is reached.
						// ReSharper disable once AccessToModifiedClosure → This is desired behavior, as this counter signals, when to stop emitting log events.
						if (emittedLogEventCount <= 5) return Task.FromResult(false);

						//! Cancel log event emission.
						cancellationTokenSource.Cancel();
						return Task.FromResult(true);
					}
				);
			_fixture.Inject(seqServerMock.Object);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Start emitting log events.
			do
			{
				bufferSink.Emit(_fixture.Create<LogEvent>());
				emittedLogEventCount++;
				try
				{
					await Task.Delay(500, cancellationToken);
				}
				catch (OperationCanceledException) { /* ignore */ }
			}
			while (!cancellationToken.IsCancellationRequested);
			
			// Act
			// Wait for flush to complete.
			await Task.Delay(500, CancellationToken.None);

			// Assert
			Assert.That(bufferSink.QueuedEvents, Is.Empty);
			Mock.Get(mockSink).Verify(sink => sink.Emit(It.IsAny<LogEvent>()), Times.Exactly(emittedLogEventCount));
		}
	}
}