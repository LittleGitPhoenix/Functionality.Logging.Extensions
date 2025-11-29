using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class LoggerGroupTest
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
		LoggerGroupManager.Cache.Clear();
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTests() { }

	#endregion

	#region Tests
	
	/// <summary>
	/// Checks that <see cref="ILogger"/> instances are not kept alive when they are cached by a <see cref="ILoggerGroup"/>.
	/// </summary>
	[Test]
	public void LoggersAreNotKeptAliveByGroup()
	{
		// Arrange
		var logger = _fixture.Create<ILogger>();
		var loggerGroup = new LoggerGroup(logger);

		void AddMoreLoggers()
		{
			var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
			foreach (var otherLogger in loggers) loggerGroup.AddLogger(otherLogger, false);

			Assert.That(loggerGroup, Has.Count.EqualTo(4));
		}

		// Act
		AddMoreLoggers();
		GC.Collect();
		
		// Assert
		Assert.That(loggerGroup, Has.Count.EqualTo(1));
		Assert.That(loggerGroup.Single(), Is.EqualTo(logger));
	}

	/// <summary>
	/// Checks if an <see cref="ILogger"/> is added to a group all existing <see cref="LogScope"/>s are applied to the new logger based on the <b>applyExistingScope</b> parameter.
	/// </summary>
	[Test]
	[TestCase(true)]
	[TestCase(false)]
	public void ExistingLogScopesAreAppliedWhenLoggerIsAddedToExistingGroup(bool applyExistingScope)
	{
		// Arrange: Create logger group with existing scope.
		var logger = _fixture.Create<ILogger>();
		var loggerGroup = new LoggerGroup(logger);
		loggerGroup.CreateScope(new LogScope(("TestScope", "TestValue")));

		// Arrange: Create mocked logger, that can verify if BeginScope was called.
		var newLogger = _fixture.Create<Mock<ILogger>>().Object;

		// Act
		loggerGroup.AddLogger(newLogger, applyExistingScope);

		// Assert
		Mock.Get(newLogger).Verify(mock => mock.BeginScope(It.IsAny<It.IsAnyType>()), applyExistingScope ? Times.Once : Times.Never);
	}

	#endregion
}