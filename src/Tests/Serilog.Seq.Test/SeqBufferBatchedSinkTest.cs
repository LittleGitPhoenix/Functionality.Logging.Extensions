using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Seq.Test;

public class SeqBufferBatchedSinkTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Data
	#endregion

	#region Tests

	[Test]
	public async Task Check_Log_Events_Are_Directly_Forwarded_If_Application_Was_Registered()
	{
		// Arrange
		var batch = _fixture.CreateMany<LogEvent>(10).ToArray();
		var mockUnderlyingSink = Mock.Of<IBatchedLogEventSink>();
		Mock.Get(mockUnderlyingSink).Setup(sink => sink.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>())).Returns(Task.CompletedTask).Verifiable();
		_fixture.Inject(mockUnderlyingSink);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);
		_fixture.Inject(seqServerMock.Object);
		var sink = _fixture.Create<SeqBufferBatchedSink>();

		// Act
		await Task.Delay(TimeSpan.FromMilliseconds(100)); //! Give the background thread time to register before emitting.
		await sink.EmitBatchAsync(batch);

		// Assert
		Assert.That(sink.QueuedEvents, Is.Empty);
		Mock.Get(mockUnderlyingSink).Verify(s => s.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()), Times.Once);
	}

	[Test]
	public async Task Check_Log_Events_Are_Buffered_As_Long_As_Application_Is_Not_Registered()
	{
		// Arrange
		var batch = _fixture.CreateMany<LogEvent>(10).ToArray();
		var mockUnderlyingSink = Mock.Of<IBatchedLogEventSink>();
		Mock.Get(mockUnderlyingSink).Setup(sink => sink.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>())).Returns(Task.CompletedTask).Verifiable();
		_fixture.Inject(mockUnderlyingSink);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(_fixture.Create<SeqServerApplicationRegisterException>());
		_fixture.Inject(seqServerMock.Object);
		var sinkMock = _fixture.Create<Mock<SeqBufferBatchedSink>>();
		var sink = sinkMock.Object;

		// Act
		await sink.EmitBatchAsync(batch);

		// Assert
		Mock.Get(sink).Verify(s => s.RemoveElementFromQueue(), Times.Never);
		Mock.Get(sink).Verify(s => s.ForwardLogEventsAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()), Times.Never);
		Mock.Get(mockUnderlyingSink).Verify(s => s.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()), Times.Never);
		Assert.That(sink.QueuedEvents, Is.EqualTo(batch));
	}

	[Test]
	public async Task Check_Buffered_Log_Events_Are_Discarded_If_Limit_Is_Reached()
	{
		// Arrange
		var sizeLimit = 5;
		var batch = _fixture.CreateMany<LogEvent>(sizeLimit * 2).ToArray();
		_fixture.Inject(sizeLimit);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(_fixture.Create<SeqServerApplicationRegisterException>());
		_fixture.Inject(seqServerMock.Object);
		var sink = _fixture.Create<SeqBufferBatchedSink>();

		// Act
		foreach (var logEvent in batch) await sink.EmitBatchAsync([logEvent]);

		// Assert
		Assert.That(sink.QueuedEvents, Is.EqualTo(batch.Skip(sizeLimit)));
	}

	[Test]
	[Retry(2)]
	public async Task Check_Buffered_Log_Events_Are_Flushed_When_Application_Was_Registered()
	{
		// Arrange
		var emittedBatchCount = 0;
		var cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = cancellationTokenSource.Token;
		var mockUnderlyingSink = Mock.Of<IBatchedLogEventSink>();
		Mock.Get(mockUnderlyingSink).Setup(sink => sink.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>())).Returns(Task.CompletedTask).Verifiable();
		_fixture.Inject(mockUnderlyingSink);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock
			.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Returns
			(
				() =>
				{
					//! Let registration fail until a certain number of batches have been emitted.
					// ReSharper disable once AccessToModifiedClosure → This is desired behavior.
					if (emittedBatchCount <= 5) throw _fixture.Create<SeqServerApplicationRegisterException>();

					//! Cancel further emission.
					cancellationTokenSource.Cancel();
					return Task.FromResult(string.Empty);
				}
			);
		_fixture.Inject(seqServerMock.Object);
		var sink = _fixture.Create<SeqBufferBatchedSink>();

		// Act: emit batches until registration succeeds and cancels the loop.
		do
		{
			await sink.EmitBatchAsync([_fixture.Create<LogEvent>()]);
			emittedBatchCount++;
			try
			{
				await Task.Delay(500, cancellationToken);
			}
			catch (OperationCanceledException) { /* ignore */ }
		}
		while (!cancellationToken.IsCancellationRequested);

		// Wait for the flush triggered by the registration thread to complete.
		await Task.Delay(1000, CancellationToken.None);

		// Assert
		Assert.That(sink.QueuedEvents, Is.Empty);
		Mock.Get(mockUnderlyingSink).Verify(s => s.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()), Times.AtLeastOnce, "Maybe the wait time for flushing did not suffice.");
	}

	[Test]
	public async Task Check_Queue_Is_Cleared_If_Application_Could_Not_Be_Registered()
	{
		// Arrange
		byte? retryCount = 2;
		var failedRegistrations = 0;
		var mockUnderlyingSink = Mock.Of<IBatchedLogEventSink>();
		_fixture.Inject(mockUnderlyingSink);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock
			.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Returns
			(
				() =>
				{
					failedRegistrations++;
					throw _fixture.Create<SeqServerApplicationRegisterException>();
				}
			);
		_fixture.Inject(seqServerMock.Object);
		_fixture.Inject(retryCount);
		var sink = _fixture.Create<SeqBufferBatchedSink>();

		// Act: emit events while registration is still failing.
		do
		{
			await sink.EmitBatchAsync([_fixture.Create<LogEvent>()]);
			await Task.Delay(500, CancellationToken.None);
		}
		while (failedRegistrations < retryCount);

		// Wait for the queue clearance to complete.
		await Task.Delay(500, CancellationToken.None);

		// Assert
		Assert.That(sink.QueuedEvents, Is.Empty);
		seqServerMock.Verify(s => s.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount.Value));
	}

	[Test]
	public async Task Check_Log_Events_Are_Ignored_If_Application_Could_Not_Be_Registered()
	{
		// Arrange
		byte? retryCount = 2;
		var failedRegistrations = 0;
		var mockUnderlyingSink = Mock.Of<IBatchedLogEventSink>();
		Mock.Get(mockUnderlyingSink).Setup(sink => sink.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>())).Returns(Task.CompletedTask).Verifiable();
		_fixture.Inject(mockUnderlyingSink);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock
			.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Returns
			(
				() =>
				{
					failedRegistrations++;
					throw _fixture.Create<SeqServerApplicationRegisterException>();
				}
			);
		_fixture.Inject(seqServerMock.Object);
		_fixture.Inject(retryCount);
		var sink = _fixture.Create<SeqBufferBatchedSink>();

		// Act: wait until registration has ultimately failed.
		do
		{
			await Task.Delay(500, CancellationToken.None);
		}
		while (failedRegistrations < retryCount);

		// Emit a batch that must be silently dropped.
		await sink.EmitBatchAsync([_fixture.Create<LogEvent>()]);

		// Assert
		Assert.That(sink.QueuedEvents, Is.Empty);
		Mock.Get(mockUnderlyingSink).Verify(s => s.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()), Times.Never);
		seqServerMock.Verify(s => s.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(retryCount.Value));
	}

	#endregion
}