using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class DisposableExtensionsTest
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
	public void AfterAllTests() { }

	#endregion

	#region Data
	#endregion

	#region Tests

	[Test]
	public void DisposingCombinedDisposablesSucceeds()
	{
		// Arrange
		var disposableMocks = _fixture.CreateMany<Mock<IDisposable>>(count: 3).ToArray();
		foreach (var disposableMock in disposableMocks)
		{
			disposableMock.Setup(mock => mock.Dispose()).Verifiable();
		}

		// Act
		var disposable = disposableMocks.Select(mock => mock.Object).Combine();
		disposable.Dispose();
		
		// Assert
		Assert.Multiple
		(
			() =>
			{
				foreach (var disposableMock in disposableMocks)
				{
					disposableMock.Verify(mock => mock.Dispose(), Times.Once);
				}
			}
		);
	}

	#endregion
}