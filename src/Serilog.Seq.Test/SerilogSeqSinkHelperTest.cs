using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Base;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

namespace Serilog.Seq.Test;

public class SerilogSeqSinkHelperTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	#endregion

	[Test]
	public void GetSeqSinkViaReflectionSucceeds()
	{
		// Act
		var success = SerilogSeqSinkHelper.TryGetSeqRequirements(out _, out _, SelfLogger.DefaultSelfLogger, "http://localhost");

		// Assert
		Assert.That(success, Is.True);
	}

	[Test]
	public void SeqBufferSinkIsNotReturnedIfApplicationCouldBeRegistered()
	{
		// Arrange
		var seqHost = "http://nevermind";
		_fixture.Inject(seqHost);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<LogApplicationInformation>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		var seqServer = seqServerMock.Object;

		// Act
		var (sink, _) = SerilogSeqSinkHelper.CreateSink(seqServer, "Title");

		// Assert
		Assert.That(sink, Is.Not.TypeOf<SeqBufferSink>());
	}

	[Test]
	public void SeqBufferSinkIsReturnedIfApplicationCouldNotBeRegistered()
	{
		// Arrange
		var seqHost = "http://nevermind";
		_fixture.Inject(seqHost);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<LogApplicationInformation>(), It.IsAny<CancellationToken>())).Throws(_fixture.Create<SeqServerApplicationRegisterException>());
		var seqServer = seqServerMock.Object;

		// Act
		var (sink, _) = SerilogSeqSinkHelper.CreateSink(seqServer, "Title");

		// Assert
		Assert.That(sink, Is.TypeOf<SeqBufferSink>());
	}

	[Test]
	public void NullIsReturnedIfApplicationCouldNotBeRegisteredAndRetryIsDisabled()
	{
		// Arrange
		var seqHost = "http://nevermind";
		_fixture.Inject(seqHost);
		var seqServerMock = _fixture.Create<Mock<SeqServer>>();
		seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<LogApplicationInformation>(), It.IsAny<CancellationToken>())).Throws(_fixture.Create<SeqServerApplicationRegisterException>());
		var seqServer = seqServerMock.Object;

		// Act
		var (sink, _) = SerilogSeqSinkHelper.CreateSink(seqServer, "Title", retryOnError: false);

		// Assert
		Assert.That(sink, Is.Null);
	}
}