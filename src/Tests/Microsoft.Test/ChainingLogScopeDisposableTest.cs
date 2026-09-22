using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class ChainingLogScopeDisposableTest
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
	public void ConstructorWithLoggerOnlySetsLoggerProperty()
	{
		// Arrange
		var logger = _fixture.Create<Mock<ILogger>>().Object;

		// Act
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(logger);

		// Assert
		Assert.That(chainingLogScopeDisposable.Logger, Is.SameAs(logger));
	}

	[Test]
	public void ConstructorWithLoggerAndScopeSetsLoggerProperty()
	{
		// Arrange
		var logger = _fixture.Create<Mock<ILogger>>().Object;
		var scope = _fixture.Create<Mock<IDisposable>>().Object;

		// Act
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(logger, scope);

		// Assert
		Assert.That(chainingLogScopeDisposable.Logger, Is.SameAs(logger));
	}

	[Test]
	public void ConstructorUnwrapsNestedChainingLoggerAndUsesInnerLogger()
	{
		// Arrange
		var innerLogger = _fixture.Create<Mock<ILogger>>().Object;
		var innerChaining = new ChainingLogScopeDisposable(innerLogger);

		// Act
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(innerChaining, _fixture.Create<Mock<IDisposable>>().Object);

		// Assert
		Assert.That(chainingLogScopeDisposable.Logger, Is.SameAs(innerLogger));
	}

	[Test]
	public void ConstructorUnwrapsDeeplyNestedChainingLoggerAndUsesInnermostLogger()
	{
		// Arrange
		var innermostLogger = _fixture.Create<Mock<ILogger>>().Object;
		var level1 = new ChainingLogScopeDisposable(innermostLogger);
		var level2 = new ChainingLogScopeDisposable(level1, _fixture.Create<Mock<IDisposable>>().Object);

		// Act
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(level2, _fixture.Create<Mock<IDisposable>>().Object);

		// Assert
		Assert.That(chainingLogScopeDisposable.Logger, Is.SameAs(innermostLogger));
	}

	[Test]
	public void DisposeDisposesOwnScope()
	{
		// Arrange
		var scopeMock = new Mock<IDisposable>();
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(_fixture.Create<Mock<ILogger>>().Object, scopeMock.Object);

		// Act
		chainingLogScopeDisposable.Dispose();

		// Assert
		scopeMock.Verify(mock => mock.Dispose(), Times.Once);
	}

	[Test]
	public void DisposeAlsoDisposesNestedScopeWhenLoggerWasAlreadyChained()
	{
		// Arrange
		var innerScopeMock = new Mock<IDisposable>();
		var outerScopeMock = new Mock<IDisposable>();
		var innerChaining = new ChainingLogScopeDisposable(_fixture.Create<Mock<ILogger>>().Object, innerScopeMock.Object);
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(innerChaining, outerScopeMock.Object);

		// Act
		chainingLogScopeDisposable.Dispose();

		// Assert
		outerScopeMock.Verify(mock => mock.Dispose(), Times.Once);
		innerScopeMock.Verify(mock => mock.Dispose(), Times.Once);
	}

	[Test]
	public void DisposeDoesNotThrowWhenNoNestedScope()
	{
		// Arrange
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(_fixture.Create<Mock<ILogger>>().Object);

		// Act + Assert
		Assert.That(() => chainingLogScopeDisposable.Dispose(), Throws.Nothing);
	}

	[Test]
	[Category("LoggerDelegation")]
	public void IsEnabledDelegatesToInnerLogger()
	{
		// Arrange
		var logger = new Mock<ILogger>().Object;
		Mock.Get(logger).Setup(mock => mock.IsEnabled(LogLevel.Warning)).Returns(true);
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(logger);

		// Act
		var result = chainingLogScopeDisposable.IsEnabled(LogLevel.Warning);

		// Assert
		Assert.That(result, Is.True);
		Mock.Get(logger).Verify(mock => mock.IsEnabled(LogLevel.Warning), Times.Once);
	}

	[Test]
	[Category("LoggerDelegation")]
	public void LogDelegatesToInnerLogger()
	{
		// Arrange
		var logger = new Mock<ILogger>().Object;
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(logger);
		var eventId = new EventId(1);
		Func<string, Exception?, string> formatter = (s, _) => s;

		// Act
		chainingLogScopeDisposable.Log(LogLevel.Information, eventId, "message", null, formatter);

		// Assert
		Mock.Get(logger).Verify(mock => mock.Log(LogLevel.Information, eventId, "message", null, formatter), Times.Once);
	}

	[Test]
	[Category("LoggerDelegation")]
	public void BeginScopeDelegatesToInnerLogger()
	{
		// Arrange
		var scopeDisposable = _fixture.Create<Mock<IDisposable>>().Object;
		var logger = new Mock<ILogger>().Object;
		Mock.Get(logger).Setup(mock => mock.BeginScope("state")).Returns(scopeDisposable);
		var chainingLogScopeDisposable = new ChainingLogScopeDisposable(logger);

		// Act
		var result = chainingLogScopeDisposable.BeginScope("state");

		// Assert
		Assert.That(result, Is.SameAs(scopeDisposable));
		Mock.Get(logger).Verify(mock => mock.BeginScope("state"), Times.Once);
	}

	#endregion
}