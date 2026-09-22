using System.Reflection;
using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class LogApplicationInformationTest
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

	#region None

	[Test]
	public void NoneHasExpectedValues()
	{
		// Arrange + Act
		var none = LogApplicationInformation.None;

		// Assert
		Assert.That(none.Name, Is.EqualTo(String.Empty));
		Assert.That(none.NumericIdentifier, Is.EqualTo(0));
		Assert.That(none.AlphanumericIdentifier, Is.EqualTo(String.Empty));
		Assert.That(none.AssemblyVersion, Is.Null);
		Assert.That(none.FileVersion, Is.Null);
		Assert.That(none.InformationalVersion, Is.Null);
	}

	#endregion

	#region Default

	[Test]
	public void DefaultIsBuiltFromEntryAssembly()
	{
#if !NETCOREAPP3_0_OR_GREATER
		Assert.Inconclusive("Assembly.GetEntryAssembly() returns null for tests in .NET Framework.");
#else
		// Arrange
		var expectedName = Assembly.GetEntryAssembly()!.GetName().Name;

		// Act
		var defaultInfo = LogApplicationInformation.Default;

		// Assert
		Assert.That(defaultInfo.Name, Is.Not.Empty.And.EqualTo(expectedName));
#endif
	}

	#endregion

	#region Identifiers

	[Test]
	public void IdentifiersAreComputedFromName()
	{
		// Arrange
		var name = _fixture.Create<string>();

		// Act
		var info = new LogApplicationInformation(name);

		// Assert
		Assert.That(info.Name, Is.EqualTo(name));
		Assert.That(info.AlphanumericIdentifier, Has.Length.EqualTo(20));
	}

	[Test]
	public void SameNameProducesSameIdentifiers()
	{
		// Arrange
		var name = _fixture.Create<string>();

		// Act
		var info1 = new LogApplicationInformation(name);
		var info2 = new LogApplicationInformation(name);

		// Assert
		Assert.That(info1.NumericIdentifier, Is.EqualTo(info2.NumericIdentifier));
		Assert.That(info1.AlphanumericIdentifier, Is.EqualTo(info2.AlphanumericIdentifier));
	}

	[Test]
	public void DifferentNamesProduceDifferentIdentifiers()
	{
		// Arrange
		var name1 = _fixture.Create<string>();
		var name2 = _fixture.Create<string>();

		// Act
		var info1 = new LogApplicationInformation(name1);
		var info2 = new LogApplicationInformation(name2);

		// Assert
		Assert.That(info1.NumericIdentifier, Is.Not.EqualTo(info2.NumericIdentifier));
		Assert.That(info1.AlphanumericIdentifier, Is.Not.EqualTo(info2.AlphanumericIdentifier));
	}

	#endregion

	#region Versions

	[Test]
	public void VersionsArePopulatedFromEntryAssembly()
	{
#if !NETCOREAPP3_0_OR_GREATER
		Assert.Inconclusive("Assembly.GetEntryAssembly() returns null for tests in .NET Framework.");
#else
		// Arrange
		var expectedAssemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

		// Act
		var info = new LogApplicationInformation(_fixture.Create<string>());

		// Assert
		Assert.That(info.AssemblyVersion, Is.EqualTo(expectedAssemblyVersion));
#endif
	}

	#endregion

	#region Factory

	[Test]
	public void CreateReturnsBuilder()
	{
		// Arrange + Act
		var builder = LogApplicationInformation.Create();

		// Assert
		Assert.That(builder, Is.Not.Null);
		Assert.That(builder, Is.InstanceOf<ILogApplicationInformationBuilder>());
	}

	#endregion

	#endregion
}
