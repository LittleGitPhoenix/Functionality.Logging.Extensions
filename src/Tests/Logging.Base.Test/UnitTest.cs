using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class UnitTest
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
	public void ValueIsDefaultInstance()
	{
		// Arrange + Act + Assert
		Assert.That(Unit.Value, Is.EqualTo(default(Unit)));
	}

	[Test]
	public void TwoUnitValuesAreEqual()
	{
		// Arrange
		var first = Unit.Value;
		var second = Unit.Value;

		// Act + Assert
		Assert.That(first, Is.EqualTo(second));
	}

	[Test]
	public void UnitValueEqualityIsSymmetric()
	{
		// Arrange
		var first = Unit.Value;
		var second = new Unit();

		// Act + Assert
		Assert.That(first, Is.EqualTo(second));
		Assert.That(second, Is.EqualTo(first));
	}

	#endregion
}