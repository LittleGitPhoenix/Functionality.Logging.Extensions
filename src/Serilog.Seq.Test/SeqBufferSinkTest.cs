using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		public async Task Check_Log_Events_Are_Directly_Emitted_If_Application_Was_Registered()
		{
			// Arrange
			var logEvents = _fixture.CreateMany<LogEvent>(10).ToArray();
			var mockSink = Mock.Of<ILogEventSink>();
			Mock.Get(mockSink).Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Verifiable();
			_fixture.Inject(mockSink);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			_fixture.Inject(seqServerMock.Object);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Act
			await Task.Delay(TimeSpan.FromMilliseconds(100)); //! Give the SeqBufferSink time to register. This is needed, as ALL registration attempts are executed within a separate thread that needs time to run at least once.
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
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(_fixture.Create<SeqServerApplicationRegisterException>());
			_fixture.Inject(seqServerMock.Object);

			var bufferSinkGenerator = new Check_Log_Events_Are_Buffered_As_Long_As_Application_Is_Not_Registered_SeqBufferSinkGenerator(logEvents.Length);
			_fixture.Customizations.Add(bufferSinkGenerator);
			var bufferSink = _fixture.Create<Mock<SeqBufferSink>>().Object;
			_fixture.Customizations.Remove(bufferSinkGenerator);

			// Act
			foreach (var logEvent in logEvents) bufferSink.Emit(logEvent);

			// Assert
			Mock.Get(bufferSink).Verify(sink => sink.RemoveElementFromQueue(), Times.Never);
			Mock.Get(bufferSink).Verify(sink => sink.ForwardLogEventToOtherSink(It.IsAny<LogEvent>()), Times.Never);
			Mock.Get(mockSink).Verify(sink => sink.Emit(It.IsAny<LogEvent>()), Times.Never);
			Assert.AreEqual(bufferSink.QueuedEvents, logEvents);
		}

		class Check_Log_Events_Are_Buffered_As_Long_As_Application_Is_Not_Registered_SeqBufferSinkGenerator : ISpecimenBuilder
		{
			private readonly int _logEventCount;

			public Check_Log_Events_Are_Buffered_As_Long_As_Application_Is_Not_Registered_SeqBufferSinkGenerator(int logEventCount)
			{
				_logEventCount = logEventCount;
			}

			public object Create(object request, ISpecimenContext context)
			{
				if
				(
					request is ParameterInfo parameter
					&& parameter.Member.DeclaringType == typeof(SeqBufferSink)
					&& parameter.ParameterType == typeof(int)
					&& parameter.IsOptional
					//&& parameter.Name == "queueSizeLimit"
				)
				{
					return parameter.DefaultValue ?? _logEventCount;
				}

				return new NoSpecimen();
			}
		}

		[Test]
		public void Check_Buffered_Log_Events_Are_Discarded_If_Limit_Is_Reached()
		{
			// Arrange
			var sizeLimit = 5;
			var logEvents = _fixture.CreateMany<LogEvent>(sizeLimit * 2).ToArray();
			_fixture.Inject(sizeLimit);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(_fixture.Create<SeqServerApplicationRegisterException>());
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
				.Setup(server => server.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns
				(
					() =>
					{
						//! Let application registering fail until a certain amount of queued log events is reached.
						// ReSharper disable once AccessToModifiedClosure → This is desired behavior, as this counter signals, when to stop emitting log events.
						if (emittedLogEventCount <= 5) throw _fixture.Create<SeqServerApplicationRegisterException>();

						//! Cancel log event emission.
						Console.WriteLine($"{DateTime.Now:hh:mm:ss:ffff}:: Log server is now registered.");
						cancellationTokenSource.Cancel();
						return Task.CompletedTask;
					}
				);
			_fixture.Inject(seqServerMock.Object);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Start emitting log events.
			do
			{
				bufferSink.Emit(_fixture.Create<LogEvent>());
				emittedLogEventCount++;
				Console.WriteLine($"{DateTime.Now:hh:mm:ss:ffff}:: Emitted log message #{emittedLogEventCount:D2}.");
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

		[Test]
		public async Task Check_Queue_Is_Cleared_If_Application_Could_Not_Be_Registered()
		{
			// Arrange
			byte? retryCount = 2;
			var emittedLogEventCount = 0;
			var failedRegistrations = 0;
			var mockSink = Mock.Of<ILogEventSink>();
			Mock.Get(mockSink)
				.Setup(sink => sink.Emit(It.IsAny<LogEvent>()))
				.Verifiable()
				;
			_fixture.Inject(mockSink);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock
				.Setup(mock => mock.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns
				(
					() =>
					{
						failedRegistrations++;
						throw _fixture.Create<SeqServerApplicationRegisterException>();
					}
				)
				;
			_fixture.Inject(seqServerMock.Object);
			_fixture.Inject(retryCount);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Act
			
			// Start emitting log events.
			do
			{
				bufferSink.Emit(_fixture.Create<LogEvent>());
				emittedLogEventCount++;
				await Task.Delay(500, CancellationToken.None);
			}
			while (failedRegistrations < retryCount);

			// Wait for queue clearance to complete.
			await Task.Delay(500, CancellationToken.None);
			
			// Assert
			Assert.That(bufferSink.QueuedEvents, Is.Empty);
			seqServerMock.Verify(mock => mock.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount.Value));
		}

		[Test]
		public async Task Check_Log_Events_Are_Ignored_If_Application_Could_Not_Be_Registered()
		{
			// Arrange
			byte? retryCount = 2;
			var failedRegistrations = 0;
			var mockSink = Mock.Of<ILogEventSink>();
			Mock.Get(mockSink)
				.Setup(sink => sink.Emit(It.IsAny<LogEvent>()))
				.Verifiable()
				;
			_fixture.Inject(mockSink);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock
				.Setup(mock => mock.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns
				(
					() =>
					{
						failedRegistrations++;
						throw _fixture.Create<SeqServerApplicationRegisterException>();
					}
				)
				;
			_fixture.Inject(seqServerMock.Object);
			_fixture.Inject(retryCount);
			var bufferSink = _fixture.Create<SeqBufferSink>();

			// Act
			
			// Wait until registration failed.
			do
			{
				await Task.Delay(500, CancellationToken.None);
			}
			while (failedRegistrations < retryCount);
			
			// Emit a log event that should be ignored.
			bufferSink.Emit(_fixture.Create<LogEvent>());
			
			// Assert
			Assert.That(bufferSink.QueuedEvents, Is.Empty);
			seqServerMock.Verify(mock => mock.RegisterApplicationAsync(It.IsAny<SeqServerApplicationInformation>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount.Value));
		}
	}
}