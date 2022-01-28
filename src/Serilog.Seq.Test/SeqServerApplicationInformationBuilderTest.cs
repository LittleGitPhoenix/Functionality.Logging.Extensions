using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

namespace Serilog.Seq.Test;

[TestFixture]
public class SeqServerApplicationInformationBuilderTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	#endregion

	[Test]
	public void Create_With_Application_Name()
	{
		// Arrange
		var targetIdentifier = Assembly.GetEntryAssembly()?.GetName().Name;

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWithApplicationName().Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}

	[Test]
	public void Create_With_Application_Name_And_Machine_Name()
	{
		// Arrange
		var separator = '_';
		var targetIdentifier = $"{Assembly.GetEntryAssembly()?.GetName().Name}{separator}{Environment.MachineName}";

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWithApplicationName().SeparatedBy(separator).AndMachineName().Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}

	[Test]
	public void Create_With_Application_Name_And_User()
	{
		// Arrange
		var separator = '_';
		var targetIdentifier = $"{Assembly.GetEntryAssembly()?.GetName().Name}{separator}{Environment.UserDomainName}{separator}{Environment.UserName}";

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWithApplicationName().SeparatedBy(separator).AndUserDomain().SeparatedBy(separator).AndUserName().Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}

	[Test]
	public void Create_With_Application_Name_And_Operating_System()
	{
		// Arrange
		var separator = '_';
		var targetIdentifier = $"{Assembly.GetEntryAssembly()?.GetName().Name}{separator}{Environment.OSVersion.ToString().Replace(' ', separator)}";

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWithApplicationName().SeparatedBy(separator).AndOperatingSystemInformation(separator).Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}

	[Test]
	public void Create_With_At_Separator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '@';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWith(start).SeparatedByAt().And(end).Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}

	[Test]
	public void Create_With_Dash_Separator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '-';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWith(start).SeparatedByDash().And(end).Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}

	[Test]
	public void Create_With_Hash_Separator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '#';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWith(start).SeparatedByHash().And(end).Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}

	[Test]
	public void Create_With_Underscore_Separator()
	{
		// Arrange
		var start = _fixture.Create<string>();
		var separator = '_';
		var end = _fixture.Create<string>();
		var targetIdentifier = $"{start}{separator}{end}";

		// Act
		var seqServerApplicationInformation = SeqServerApplicationInformation.Create().StartingWith(start).SeparatedByUnderscore().And(end).Build();

		// Assert
		Assert.That(seqServerApplicationInformation.Identifier, Is.EqualTo(targetIdentifier));
		Console.WriteLine($"Identifier: {seqServerApplicationInformation.Identifier}");
	}
}