using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

namespace Serilog.Seq.Test
{
	public class SerilogSeqSinkHelperTest
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
		public void Check_Get_SeqSink_Via_Reflection_Succeeds()
		{
			// Act
			var success = SerilogSeqSinkHelper.TryGetSeqRequirements(out _, out _, "http://localhost");

			// Assert
			Assert.True(success);
		}

		[Test]
		public void Check_SeqBufferSink_Is_Not_Returned_If_Application_Could_Be_Registered()
		{
			// Arrange
			var seqHost = "http://nevermind";
			_fixture.Inject(seqHost);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
			var seqServer = seqServerMock.Object;

			// Act
			var (sink, _) = SerilogSeqSinkHelper.CreateSink(seqServer, "Title");
			
			// Assert
			Assert.That(sink, Is.Not.TypeOf<SeqBufferSink>());
		}

		[Test]
		public void Check_SeqBufferSink_Is_Returned_If_Application_Could_Not_Be_Registered()
		{
			// Arrange
			var seqHost = "http://nevermind";
			_fixture.Inject(seqHost);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));
			var seqServer = seqServerMock.Object;

			// Act
			var (sink, _) = SerilogSeqSinkHelper.CreateSink(seqServer, "Title");
			
			// Assert
			Assert.That(sink, Is.TypeOf<SeqBufferSink>());
		}

		[Test]
		public void Check_Null_Is_Returned_If_Application_Could_Not_Be_Registered_And_Retry_Is_Disabled()
		{
			// Arrange
			var seqHost = "http://nevermind";
			_fixture.Inject(seqHost);
			var seqServerMock = _fixture.Create<Mock<SeqServer>>();
			seqServerMock.Setup(server => server.RegisterApplicationAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));
			var seqServer = seqServerMock.Object;

			// Act
			var (sink, _) = SerilogSeqSinkHelper.CreateSink(seqServer, "Title", retryOnError: false);
			
			// Assert
			Assert.Null(sink);
		}
	}
}