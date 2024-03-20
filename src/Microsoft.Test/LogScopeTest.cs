using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class LogScopeTest
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
	
	/// <summary> Checks that creating a <see cref="LogScope{TIdentifier}"/> succeeds. </summary>
	[Test]
	public void LogScopeCanBeCreated()
	{
		// Arrange
		var scopeKey = "ScopeValue";

		// Act
		var logScope = new LogScope(scopeKey);

		// Assert
		Assert.That(logScope, Has.Count.EqualTo(1));
		Assert.That(logScope.First().Key, Is.EqualTo("ScopeKey"));
		Assert.That(logScope.First().Value, Is.EqualTo(scopeKey));
	}
	
	/// <summary> Checks that creating a <see cref="LogScope{TIdentifier}"/> succeeds. </summary>
	[Test]
	public void GenericLogScopeCanBeCreated()
	{
		// Arrange
		object groupIdentifier = Guid.NewGuid();
		var scopeKey = "ScopeValue";

		// Act
		var logScope = new LogScope<object>(groupIdentifier, scopeKey);

		// Assert
		Assert.That(logScope.Identifier, Is.EqualTo(groupIdentifier));
		Assert.That(logScope, Has.Count.EqualTo(1));
		Assert.That(logScope.First().Key, Is.EqualTo("ScopeKey"));
		Assert.That(logScope.First().Value, Is.EqualTo(scopeKey));
	}
	
	#endregion
}