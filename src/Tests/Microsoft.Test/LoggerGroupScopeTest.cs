using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class LoggerGroupScopeTest
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
	/// Checks that the dispose callback is invoked if a <see cref="LoggerGroupScope"/> is disposed. Additionally, checks that dispose is only executed once.
	/// </summary>
	[Test]
	public void DisposingLoggerGroupScopeInvokesCallback()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		//var scopes = _fixture.Create<Dictionary<string, object?>>();
		var scope = _fixture.Create<ILogScope>();
		var disposedCallback = Mock.Of<Action<LoggerGroupScope>>();
		Mock.Get(disposedCallback).Setup(_ => _(It.IsAny<LoggerGroupScope>())).Verifiable();
		var loggerGroupScope = new LoggerGroupScope(loggers, scope, disposedCallback);
		
		// Act
		loggerGroupScope.Dispose();
		loggerGroupScope.Dispose();
		loggerGroupScope.Dispose();
		
		// Assert
		Mock.Get(disposedCallback).Verify(_ => _(It.IsAny<LoggerGroupScope>()), Times.Once);
		Assert.That(loggerGroupScope._scope, Is.Empty);
		Assert.That(loggerGroupScope._disposables, Is.Empty);
	}

	/// <summary>
	/// Checks that removing a logger from <see cref="LoggerGroupScope"/> implicitly disposes its scopes.
	/// </summary>
	[Test]
	public void RemovingLoggerDisposesScopes()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		var scopes =
#if NETCOREAPP3_0_OR_GREATER
			new Dictionary<string, object?>(_fixture.CreateMany<KeyValuePair<string, object?>>(4));
#else
			new Dictionary<string, object?>();
			var pairs = _fixture.CreateMany<KeyValuePair<string, object?>>(4);
			foreach (var pair in pairs)
			{
				scopes[pair.Key] = pair.Value;
			}
#endif
		var scope = LogScope.CreateIndependent(scopes);

		var disposedCallback = Mock.Of<Action<LoggerGroupScope>>();
		var loggerGroupScope = new LoggerGroupScope(loggers, scope, disposedCallback);
		var originalScopesAmount = loggerGroupScope._scope.Count();
		var originalDisposableAmount = loggerGroupScope._disposables.Count;

		// Act
		loggerGroupScope.RemoveLogger(loggers.First());
		
		// Assert
		Assert.That(originalScopesAmount, Is.EqualTo(scopes.Count));            //* One scope per...well...scope.
		Assert.That(loggerGroupScope._scope, Has.Count.EqualTo(scopes.Count)); //! Should be the same, as the scopes still exist, while one disposable was removed.

		Assert.That(originalDisposableAmount, Is.EqualTo(loggers.Length));                 //* One disposable per logger.
		Assert.That(loggerGroupScope._disposables, Has.Count.EqualTo(loggers.Length - 1)); //! Should be one less as before, because the logger was removed.
	}

	/// <summary>
	/// Checks that adding a logger to a <see cref="LoggerGroupScope"/> applies the scope to the new logger and tracks it.
	/// </summary>
	[Test]
	public void AddingLoggerAppliesScopeAndTracksLogger()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		var scope = LogScope.CreateIndependent(("TestScope", "TestValue"));
		var disposedCallback = Mock.Of<Action<LoggerGroupScope>>();
		var loggerGroupScope = new LoggerGroupScope(loggers, scope, disposedCallback);
		var newLogger = _fixture.Create<Mock<ILogger>>().Object;

		// Act
		loggerGroupScope.AddLogger(newLogger);

		// Assert
		Mock.Get(newLogger).Verify(mock => mock.BeginScope(It.IsAny<It.IsAnyType>()), Times.Once);
		Assert.That(loggerGroupScope._disposables, Has.Count.EqualTo(loggers.Length + 1));
	}

	/// <summary>
	/// Checks that adding a logger to an already disposed <see cref="LoggerGroupScope"/> does nothing.
	/// </summary>
	[Test]
	public void AddingLoggerToDisposedScopeDoesNothing()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		var scope = LogScope.CreateIndependent(("TestScope", "TestValue"));
		var disposedCallback = Mock.Of<Action<LoggerGroupScope>>();
		var loggerGroupScope = new LoggerGroupScope(loggers, scope, disposedCallback);
		loggerGroupScope.Dispose();
		var newLogger = _fixture.Create<Mock<ILogger>>().Object;

		// Act
		loggerGroupScope.AddLogger(newLogger);

		// Assert
		Mock.Get(newLogger).Verify(mock => mock.BeginScope(It.IsAny<It.IsAnyType>()), Times.Never);
		Assert.That(loggerGroupScope._disposables, Is.Empty);
	}
	
	#endregion
}