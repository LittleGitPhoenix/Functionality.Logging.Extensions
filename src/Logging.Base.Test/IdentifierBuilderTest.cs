using AutoFixture;
using AutoFixture.AutoMoq;
using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class IdentifierBuilderTest
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

	#region Numeric Identifier
	
	/// <summary> Checks that identifiers remain identical between different runs, even if the application is restarted. Therefor the identifier has been generated once and is used to match all future test runs. </summary>
	[Test]
	[TestCase("", 1120186595)]
	[TestCase("SomeApplicationName", -199756240)]
	[TestCase("acc55406-a8c0-48de-86ad-2c0761ac6e9b", -586933896)]
	public void NumericIdentifierIsConsistent(string value, int target)
	{
		// Act
		var identifier = IdentifierBuilder.BuildNumericIdentifier(value);

		// Assert
		Assert.That(identifier, Is.EqualTo(target));
	}

	[Test]
	public void SameValuesProduceSameNumericIdentifier()
	{
		// Arrange
		var objects1 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};
		var objects2 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};

		// Act
		var identifier1 = IdentifierBuilder.BuildNumericIdentifier(objects1);
		var identifier2 = IdentifierBuilder.BuildNumericIdentifier(objects2);

		// Assert
		Assert.That(identifier1, Is.EqualTo(identifier2));
	}

	[Test]
	public void SameValuesInDifferentOrderProduceSameNumericIdentifier()
	{
		// Arrange
		var objects1 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};
		var objects2 = new String[]
		{
			"7",
			"MachineName",
			"ApplicationName"
		};

		// Act
		var identifier1 = IdentifierBuilder.BuildNumericIdentifier(objects1);
		var identifier2 = IdentifierBuilder.BuildNumericIdentifier(objects2);

		// Assert
		Assert.That(identifier1, Is.EqualTo(identifier2));
	}

	[Test]
	public void DifferentValuesProduceDifferentNumericIdentifiers()
	{
		// Arrange
		var objects1 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};
		var objects2 = new String[]
		{
			"Unknown",
			"Unknown",
			"0",
		};

		// Act
		var identifier1 = IdentifierBuilder.BuildNumericIdentifier(objects1);
		var identifier2 = IdentifierBuilder.BuildNumericIdentifier(objects2);

		// Assert
		Assert.That(identifier1, Is.Not.EqualTo(identifier2));
	}

	#endregion

	#region Alphanumeric Identifier

	[Test]
	[TestCase("")]
	[TestCase("SomeApplicationName")]
	[TestCase("acc55406-a8c0-48de-86ad-2c0761ac6e9b")]
	public void AlphanumericIdentifierIsLimitedTo20Chars(string value)
	{
		// Act
		var identifier = IdentifierBuilder.BuildAlphanumericIdentifier(value);

		// Assert
		Assert.That(identifier, Has.Length.EqualTo(20));
	}

	/// <summary> Checks that identifiers remain identical between different runs, even if the application is restarted. Therefor the identifier has been generated once and is used to match all future test runs. </summary>
	[Test]
	[TestCase("", "ZxlCi0LOMkoTT4qMK4a6")]
	[TestCase("SomeApplicationName", "AcYpzZ7Ps2RYKtz4AMGj")]
	[TestCase("acc55406-a8c0-48de-86ad-2c0761ac6e9b", "kCHNQw3YxfMRuZDpcX0U")]
	public void AlphanumericIdentifierIsConsistent(string value, string target)
	{
		// Act
		var identifier = IdentifierBuilder.BuildAlphanumericIdentifier(value);

		// Assert
		Assert.That(identifier, Is.EqualTo(target));
	}

	[Test]
	public void SameValuesProduceSameAlphanumericIdentifier()
	{
		// Arrange
		var objects1 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};
		var objects2 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};

		// Act
		var identifier1 = IdentifierBuilder.BuildAlphanumericIdentifier(objects1);
		var identifier2 = IdentifierBuilder.BuildAlphanumericIdentifier(objects2);

		// Assert
		Assert.That(identifier1, Is.EqualTo(identifier2));
	}

	[Test]
	public void SameValuesInDifferentOrderProduceSameAlphanumericIdentifier()
	{
		// Arrange
		var objects1 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};
		var objects2 = new String[]
		{
			"7",
			"MachineName",
			"ApplicationName"
		};

		// Act
		var identifier1 = IdentifierBuilder.BuildAlphanumericIdentifier(objects1);
		var identifier2 = IdentifierBuilder.BuildAlphanumericIdentifier(objects2);

		// Assert
		Assert.That(identifier1, Is.EqualTo(identifier2));
	}

	[Test]
	public void DifferentValuesProduceDifferentAlphanumericIdentifiers()
	{
		// Arrange
		var objects1 = new String[]
		{
			"ApplicationName",
			"MachineName",
			"7",
		};
		var objects2 = new String[]
		{
			"Unknown",
			"Unknown",
			"0",
		};

		// Act
		var identifier1 = IdentifierBuilder.BuildAlphanumericIdentifier(objects1);
		var identifier2 = IdentifierBuilder.BuildAlphanumericIdentifier(objects2);

		// Assert
		Assert.That(identifier1, Is.Not.EqualTo(identifier2));
	}

	#endregion

	#endregion
}