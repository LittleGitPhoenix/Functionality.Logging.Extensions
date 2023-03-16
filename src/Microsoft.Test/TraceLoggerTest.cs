using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class TraceLoggerTest
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
	public void LogIsWrittenToTraceAndConsole()
	{
		// Arrange
		var logger = _fixture.Create<Mock<TraceLogger>>().Object;

		// Act
		logger.LogDebug("My Message");

		// Assert
		Mock.Get(logger).Verify(mock => mock.TraceLog(It.IsAny<string>()), Times.Once);
		Mock.Get(logger).Verify(mock => mock.ConsoleLog(It.IsAny<string>(), It.IsAny<LogLevel>()), Times.Once);
	}

	[Test]
	public void EmptyMessagesAreIgnored()
	{
		// Arrange
		var logger = _fixture.Create<Mock<TraceLogger>>().Object;

		// Act
		logger.LogDebug(String.Empty);

		// Assert
		Mock.Get(logger).Verify(mock => mock.TraceLog(It.IsAny<string>()), Times.Never);
		Mock.Get(logger).Verify(mock => mock.ConsoleLog(It.IsAny<string>(), It.IsAny<LogLevel>()), Times.Never);
	}

	#endregion
}