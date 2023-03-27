using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
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
	/// Checks that the dispose callback is invoked if a <see cref="LoggerGroupScope"/> is disposed. Additionally checks, that dispose is only executed once.
	/// </summary>
	[Test]
	public void DisposingLoggerGroupScopeInvokesCallback()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		var scopes = _fixture.Create<Dictionary<string, object?>>();
		var disposedCallback = Mock.Of<Action<LoggerGroupScope>>();
		Mock.Get(disposedCallback).Setup(_ => _(It.IsAny<LoggerGroupScope>())).Verifiable();
		var loggerGroupScope = new LoggerGroupScope(loggers, scopes, disposedCallback);
		
		// Act
		loggerGroupScope.Dispose();
		loggerGroupScope.Dispose();
		loggerGroupScope.Dispose();
		
		// Assert
		Mock.Get(disposedCallback).Verify(_ => _(It.IsAny<LoggerGroupScope>()), Times.Once);
		Assert.IsEmpty(loggerGroupScope._scopes);
		Assert.IsEmpty(loggerGroupScope._disposables);
	}

	/// <summary>
	/// Checks that removing a logger from <see cref="LoggerGroupScope"/> implicitly disposes its scopes.
	/// </summary>
	[Test]
	public void RemovingLoggerDisposesScopes()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		var scopes = new Dictionary<string, object?>(_fixture.CreateMany<KeyValuePair<string, object?>>(4));
		var disposedCallback = Mock.Of<Action<LoggerGroupScope>>();
		var loggerGroupScope = new LoggerGroupScope(loggers, scopes, disposedCallback);
		var originalScopesAmount = loggerGroupScope._scopes.Count;
		var originalDisposableAmount = loggerGroupScope._disposables.Count;

		// Act
		loggerGroupScope.RemoveLogger(loggers.First());
		
		// Assert
		Assert.That(originalScopesAmount, Is.EqualTo(scopes.Count));            //* One scope per...well...scope.
		Assert.That(loggerGroupScope._scopes, Has.Count.EqualTo(scopes.Count)); //! Should be the same, as the scopes still exist, while one disposable was removed.
		
		Assert.That(originalDisposableAmount, Is.EqualTo(loggers.Length));                 //* One disposable per logger.
		Assert.That(loggerGroupScope._disposables, Has.Count.EqualTo(loggers.Length - 1)); //! Should be one less as before, because the logger was removed.
	}
	
	#endregion
}