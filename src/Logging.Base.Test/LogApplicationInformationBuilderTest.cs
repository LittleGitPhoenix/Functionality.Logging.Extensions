using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

[TestFixture]
public class LogApplicationInformationBuilderTest
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

	#region Name
	
	[Test]
	public void CreateWithApplicationName()
	{
		// Arrange
		var targetIdentifier = Assembly.GetEntryAssembly()?.GetName().Name;

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWithApplicationName().Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	[Test]
	public void CreateWithApplicationNameAndMachineName()
	{
		// Arrange
		var separator = '_';
		var targetIdentifier = $"{Assembly.GetEntryAssembly()?.GetName().Name}{separator}{Environment.MachineName}";

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWithApplicationName().SeparatedBy(separator).AndMachineName().Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	[Test]
	public void CreateWithApplicationNameAndUser()
	{
		// Arrange
		var separator = '_';
		var targetIdentifier = $"{Assembly.GetEntryAssembly()?.GetName().Name}{separator}{Environment.UserDomainName}{separator}{Environment.UserName}";

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWithApplicationName().SeparatedBy(separator).AndUserDomain().SeparatedBy(separator).AndUserName().Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	[Test]
	public void CreateWithApplicationNameAndOperatingSystem()
	{
		// Arrange
		var separator = '_';
		var targetIdentifier = $"{Assembly.GetEntryAssembly()?.GetName().Name}{separator}{Environment.OSVersion.ToString().Replace(' ', separator)}";

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWithApplicationName().SeparatedBy(separator).AndOperatingSystemInformation(separator).Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	[Test]
	public void CreateWithAtSeparator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '@';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWith(start).SeparatedByAt().And(end).Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	[Test]
	public void CreateWithDashSeparator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '-';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWith(start).SeparatedByDash().And(end).Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	[Test]
	public void CreateWithHashSeparator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '#';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWith(start).SeparatedByHash().And(end).Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	[Test]
	public void CreateWithUnderscoreSeparator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '_';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var applicationInformation = LogApplicationInformation.Create().StartingWith(start).SeparatedByUnderscore().And(end).Build();

		// Assert
		Assert.That(applicationInformation.Name, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {applicationInformation.Name}");
	}

	#endregion

	#region Numeric Identifier

	[Test]
	public void SameApplicationHasSameNumericIdentifier()
	{
		// Arrange
		var applicationName = _fixture.Create<string>();
		
		// Act
		var applicationInformation1 = LogApplicationInformation.Create().StartingWith(applicationName).Build();
		var applicationInformation2 = LogApplicationInformation.Create().StartingWith(applicationName).Build();

		// Assert
		Assert.That(applicationInformation1.NumericIdentifier, Is.EqualTo(applicationInformation2.NumericIdentifier));
	}

	[Test]
	public void DifferentApplicationsHaveDifferentNumericIdentifiers()
	{
		// Arrange
		var applicationName1 = _fixture.Create<string>();
		var applicationName2 = _fixture.Create<string>();
		
		// Act
		var applicationInformation1 = LogApplicationInformation.Create().StartingWith(applicationName1).Build();
		var applicationInformation2 = LogApplicationInformation.Create().StartingWith(applicationName2).Build();

		// Assert
		Assert.That(applicationInformation1.NumericIdentifier, Is.Not.EqualTo(applicationInformation2.NumericIdentifier));
	}

	#endregion

	#region Alphanumeric Identifier

	[Test]
	public void SameApplicationHasSameAlphanumericIdentifier()
	{
		// Arrange
		var applicationName = _fixture.Create<string>();

		// Act
		var applicationInformation1 = LogApplicationInformation.Create().StartingWith(applicationName).Build();
		var applicationInformation2 = LogApplicationInformation.Create().StartingWith(applicationName).Build();

		// Assert
		Assert.That(applicationInformation1.AlphanumericIdentifier, Is.EqualTo(applicationInformation2.AlphanumericIdentifier));
	}

	[Test]
	public void DifferentApplicationsHaveDifferentAlphanumericIdentifiers()
	{
		// Arrange
		var applicationName1 = _fixture.Create<string>();
		var applicationName2 = _fixture.Create<string>();

		// Act
		var applicationInformation1 = LogApplicationInformation.Create().StartingWith(applicationName1).Build();
		var applicationInformation2 = LogApplicationInformation.Create().StartingWith(applicationName2).Build();

		// Assert
		Assert.That(applicationInformation1.AlphanumericIdentifier, Is.Not.EqualTo(applicationInformation2.AlphanumericIdentifier));
	}

	#endregion

	#endregion
}