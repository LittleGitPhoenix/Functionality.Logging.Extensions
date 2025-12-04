using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

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
		var logScope =
#if NETCOREAPP3_0_OR_GREATER
			LogScope.CreateIndependent(scopeKey);
#else
			LogScope.CreateIndependent((nameof(scopeKey), scopeKey));
#endif

		// Assert
		Assert.That(logScope, Has.Count.EqualTo(1));
		Assert.That(logScope.First().Key, Is.EqualTo("ScopeKey"));
		Assert.That(logScope.First().Value, Is.EqualTo(scopeKey));
	}
	
//	/// <summary> Checks that creating a <see cref="LogScope{TIdentifier}"/> succeeds. </summary>
//	[Test]
//	public void GenericLogScopeCanBeCreated()
//	{
//		// Arrange
//		object groupIdentifier = Guid.NewGuid();
//		var scopeKey = "ScopeValue";

//		// Act
//		var logScope =
//#if NETCOREAPP3_0_OR_GREATER
//			new LogScope<object>(groupIdentifier, scopeKey);
//#else
//			new LogScope<object>(groupIdentifier, (nameof(scopeKey), scopeKey));
//#endif

//		// Assert
//		Assert.That(logScope.Identifier, Is.EqualTo(groupIdentifier));
//		Assert.That(logScope, Has.Count.EqualTo(1));
//		Assert.That(logScope.First().Key, Is.EqualTo("ScopeKey"));
//		Assert.That(logScope.First().Value, Is.EqualTo(scopeKey));
//	}
	
#endregion
}