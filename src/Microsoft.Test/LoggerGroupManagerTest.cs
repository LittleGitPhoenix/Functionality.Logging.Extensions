using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class LoggerGroupManagerTest
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

	#region Add Logger

	/// <summary>
	/// Checks that <see cref="ILogger"/>s with the same group identifier share the same group.
	/// </summary>
	[Test]
	public void LoggersWithSameIdentifierShareGroup()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		var groupIdentifier = _fixture.Create<string>();
			
		// Act
		foreach (var logger in loggers)
		{
			logger.AddToGroup(groupIdentifier);
		}
			
		// Assert
		Assert.That(LoggerGroupManager.GetAllGroups(), Has.Length.EqualTo(1));
		Assert.That(LoggerGroupManager.GetGroup(groupIdentifier), Has.Length.EqualTo(loggers.Length));
	}

	[Test]
    public void LoggersAreAddedToGroup()
    {
        // Arrange
        var loggers = _fixture.CreateMany<ILogger>(count: 6).ToArray();
        var firstUnevenLogger = loggers[1];
        var evenGroupIdentifier = _fixture.Create<string>();
        var unevenGroupIdentifier = _fixture.Create<int>();

        // Act
        for (var index = 0; index < loggers.Length; index++)
        {
            var logger = loggers[index];
            if (index == 5)
            {
                //! Last logger will be in both groups.
                LoggerGroupManager.AddLoggerToGroup(logger, evenGroupIdentifier, true);
                logger.AddToGroup(unevenGroupIdentifier);
            }
            else if (index % 2 == 0)
            {
                //! Even loggers will be in even-group.
                LoggerGroupManager.AddLoggerToGroup(logger, evenGroupIdentifier, true);
            }
            else
            {
                //! Un-even loggers will be in uneven-group.
                logger.AddToGroup(unevenGroupIdentifier);
            }
        }

        // Assert
        Assert.That(LoggerGroupManager.GetAllGroups(), Has.Length.EqualTo(2));
        Assert.That(LoggerGroupManager.GetGroup(evenGroupIdentifier), Has.Length.EqualTo(4));
        Assert.That(firstUnevenLogger.AsGroup(unevenGroupIdentifier), Has.Length.EqualTo(3));
		Assert.Multiple
		(
			() =>
			{
				for (var index = 0; index < loggers.Length; index++)
				{
					var logger = loggers[index];
					if (index == 5)
					{
						Assert.That(LoggerGroupManager.GetGroupsOfLogger(logger), Has.Length.EqualTo(2));
					}
					else
					{
						Assert.That(logger.GetGroups(), Has.Length.EqualTo(1));
					}
				}
			}
		);
	}

	#endregion

	#region Remove Loggers

	[Test]
	public void LoggersAreRemovedFromGroup()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 6).ToArray();
		var firstLogger = loggers.First();
		var groupIdentifier = _fixture.Create<string>();
		foreach (var logger in loggers) LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier, false);
		var loggerGroup = LoggerGroupManager.GetGroup(groupIdentifier);
		var originalAmount = loggerGroup.Count;

		// Act
		firstLogger.RemoveFromGroup(groupIdentifier);
		
		// Assert
		Assert.That(originalAmount, Is.EqualTo(loggers.Length));
		Assert.That(loggerGroup, Has.Length.EqualTo(loggers.Length - 1));
		Assert.That(loggerGroup.Contains(firstLogger), Is.False);
	}

	[Test]
	public void GroupIsRemovedIfAllLoggersAreRemoved()
	{
		// Arrange
		var loggers = _fixture.CreateMany<ILogger>(count: 3).ToArray();
		var groupIdentifier = _fixture.Create<string>();
		foreach (var logger in loggers) LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier, false);
		
		// Act + Assert
		Assert.That(LoggerGroupManager.Cache, Is.Not.Empty);
		foreach (var logger in loggers) LoggerGroupManager.RemoveLoggerFromGroup(logger, groupIdentifier);
		Assert.That(LoggerGroupManager.Cache, Is.Empty);
	}

	#endregion

	#region Scoping

	/// <summary>
	/// Checks that all loggers of a <see cref="ILoggerGroup"/> will get the same scope.
	/// </summary>
	[Test]
	public void ScopeIsAppliedToAllLoggers()
	{
		// Arrange
		var loggerScopes = new ConcurrentDictionary<ILogger, List<(string Key, object Value)>>();
		var groupIdentifier = _fixture.Create<string>();

		var firstLogger = _fixture.Create<Mock<ILogger>>().Object;
		Mock.Get(firstLogger)
			.Setup(mock => mock.BeginScope(It.IsAny<object>()))
			.Callback
			(
				(object value) =>
				{
					var pair = (value as Dictionary<string, object?>)!.Single();
					var internalScopes = loggerScopes.GetOrAdd(firstLogger, new List<(string Key, object Value)>());
					internalScopes.Add((pair.Key, pair.Value!));
				}
			)
			.Returns(_fixture.Create<IDisposable>())
			.Verifiable()
			;
		firstLogger.AddToGroup(groupIdentifier);
		
		var secondLogger = _fixture.Create<Mock<ILogger>>().Object;
		Mock.Get(secondLogger)
			.Setup(mock => mock.BeginScope(It.IsAny<object>()))
			.Callback
			(
				(object value) =>
				{
					var pair = (value as Dictionary<string, object?>)!.Single();
					var internalScopes = loggerScopes.GetOrAdd(secondLogger, new List<(string Key, object Value)>());
					internalScopes.Add((pair.Key, pair.Value!));
				}
			)
			.Returns(_fixture.Create<IDisposable>())
			.Verifiable()
			;
		secondLogger.AddToGroup(groupIdentifier);
		
		// Act
		firstLogger.AsGroup(groupIdentifier).CreateScope(("First", 1));
		secondLogger.AsGroup(groupIdentifier).CreateScope(("Second", 2));
		
		// Assert
		Mock.Get(firstLogger).Verify(mock => mock.BeginScope(It.IsAny<object>()), Times.Exactly(2));
		Mock.Get(secondLogger).Verify(mock => mock.BeginScope(It.IsAny<object>()), Times.Exactly(2));
		loggerScopes.TryGetValue(firstLogger, out var firstScopes);
		loggerScopes.TryGetValue(secondLogger, out var secondScopes);
		Assert.That(firstScopes, Has.Count.EqualTo(2));
		Assert.That(firstScopes, Has.Count.EqualTo(secondScopes!.Count));
		Assert.Multiple
		(
			() =>
			{
				for (var index = 0; index < firstScopes!.Count; index++)
				{
					Assert.That(firstScopes[index].Key, Is.EqualTo(secondScopes[index].Key));
					Assert.That(firstScopes[index].Value, Is.EqualTo(secondScopes[index].Value));
				}
			}
		);
	}

	/// <summary>
	/// Checks that any existing log-scope of a <see cref="ILoggerGroup"/> is applied to new loggers.
	/// </summary>
	[Test]
	public void ScopeIsAppliedToNewLogger()
	{
		// Arrange
		var loggerScopes = new ConcurrentDictionary<ILogger, List<(string Key, object Value)>>();
		var groupIdentifier = _fixture.Create<string>();
		var existingLoggers = _fixture.CreateMany<ILogger>(count: 6).ToArray();
		foreach (var logger in existingLoggers) LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier, false);
		var loggerGroup = LoggerGroupManager.GetGroup(groupIdentifier);
		loggerGroup.CreateScope(("Key", "Value"));

		var newLogger = _fixture.Create<Mock<ILogger>>().Object;
		Mock.Get(newLogger)
			.Setup(mock => mock.BeginScope(It.IsAny<object>()))
			.Callback
			(
				(object value) =>
				{
					var pair = (value as Dictionary<string, object?>)!.Single();
					var internalScopes = loggerScopes.GetOrAdd(newLogger, new List<(string Key, object Value)>());
					internalScopes.Add((pair.Key, pair.Value!));
				}
			)
			.Returns(_fixture.Create<IDisposable>())
			.Verifiable()
			;

		// Act
		newLogger.AddToGroup(groupIdentifier, applyExistingScope: true);

		// Assert
		Mock.Get(newLogger).Verify(mock => mock.BeginScope(It.IsAny<object>()), Times.Once);
		loggerScopes.TryGetValue(newLogger, out var scopes);
		Assert.That(scopes, Is.Not.Null);
		Assert.Multiple
		(
			() =>
			{
				Assert.That(scopes!.Single().Key, Is.EqualTo("Key"));
				Assert.That(scopes!.Single().Value, Is.EqualTo("Value"));
			}
		);
	}
	
	#endregion

	#region Group Retrieval

	/// <summary>
	/// Checks that <see cref="LoggerGroupManager.GetAllGroups"/> does not throw if no groups where found and instead returns an empty collection.
	/// </summary>
	[Test]
	public void GetAllGroupsReturnsEmpty()
	{
		// Arrange

		// Act
		var groups = LoggerGroupManager.GetAllGroups();

		// Assert
		Assert.That(groups, Is.Empty);
	}

	/// <summary>
	/// Checks that <see cref="LoggerGroupManager.GetGroup{TIdentifier}"/> does not throw if no loggers where found and instead returns a null <see cref="ILoggerGroup"/> instance.
	/// </summary>
	[Test]
	public void GetGroupOfLoggerReturnsEmpty()
	{
		// Arrange
		var groupIdentifier = Guid.NewGuid();

		// Act
		var loggerGroup = LoggerGroupManager.GetGroup(groupIdentifier);
			
		// Assert
		Assert.That(loggerGroup, Is.Empty);
	}

	/// <summary>
	/// Checks that <see cref="LoggerGroupManager.GetGroupsOfLogger"/> does not throw if no groups where found and instead returns an empty collection.
	/// </summary>
	[Test]
	public void GetGroupsOfLoggerReturnsEmpty()
	{
		// Arrange
		var logger = _fixture.Create<ILogger>();
			
		// Act
		var groups = LoggerGroupManager.GetGroupsOfLogger(logger);
			
		// Assert
		Assert.That(groups, Is.Empty);
	}

	#endregion

	#region GroupIdentifier

    [Test]
    public void GroupIdentifierIsSameForSameValueType()
    {
        // Arrange
        var groupIdentifier = Guid.NewGuid();

        // Act
        var identifier1 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier);
        var identifier2 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier);
			
        // Assert
        Assert.That(identifier1, Is.EqualTo(identifier2));
        Assert.That(identifier1, Is.Not.SameAs(identifier2));
    }

    [Test]
    public void GroupIdentifierIsSameForEqualValueType()
    {
        // Arrange
        var groupIdentifier1 = 10;
        var groupIdentifier2 = 10;

        // Act
        var identifier1 = new LoggerGroupManager.GroupIdentifier<int>(groupIdentifier1);
        var identifier2 = new LoggerGroupManager.GroupIdentifier<int>(groupIdentifier2);
			
        // Assert
        Assert.That(identifier1, Is.EqualTo(identifier2));
        Assert.That(identifier1, Is.Not.SameAs(identifier2));
    }

    [Test]
    public void GroupIdentifierIsSameForSameReferenceType()
    {
        // Arrange
        var groupIdentifier = new object();

        // Act
        var identifier1 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier);
        var identifier2 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier);
			
        // Assert
        Assert.That(identifier1, Is.EqualTo(identifier2));
        Assert.That(identifier1, Is.Not.SameAs(identifier2));
    }

    [Test]
    public void GroupIdentifierIsSameForEqualReferenceType()
    {
        // Arrange
        var groupIdentifier1 = "Equal";
        var groupIdentifier2 = "Equal";

        // Act
        var identifier1 = new LoggerGroupManager.GroupIdentifier<string>(groupIdentifier1);
        var identifier2 = new LoggerGroupManager.GroupIdentifier<string>(groupIdentifier2);
			
        // Assert
        Assert.That(identifier1, Is.EqualTo(identifier2));
        Assert.That(identifier1, Is.Not.SameAs(identifier2));
    }

    [Test]
    public void GroupIdentifierIsNotSameForValueType()
    {
        // Arrange
        var groupIdentifier1 = Guid.NewGuid();
        var groupIdentifier2 = Guid.NewGuid();

        // Act
        var identifier1 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier1);
        var identifier2 = new LoggerGroupManager.GroupIdentifier<Guid>(groupIdentifier2);

        // Assert
        Assert.That(identifier1, Is.Not.EqualTo(identifier2));
        Assert.That(identifier1, Is.Not.SameAs(identifier2));
    }

    [Test]
    public void GroupIdentifierIsNotSameForReferenceType()
    {
        // Arrange
        var groupIdentifier1 = new object();
        var groupIdentifier2 = new object();

        // Act
        var identifier1 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier1);
        var identifier2 = new LoggerGroupManager.GroupIdentifier<object>(groupIdentifier2);

        // Assert
        Assert.That(identifier1, Is.Not.EqualTo(identifier2));
        Assert.That(identifier1, Is.Not.SameAs(identifier2));
    }

	#endregion

	#endregion
}