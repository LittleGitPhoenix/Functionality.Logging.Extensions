using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

namespace Serilog.Microsoft.Test;

public class SerilogToMicrosoftLogLevelConverterTest
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
	public void InstanceShouldReturnSingleton()
	{
		// Arrange & Act
		var instance1 = SerilogToMicrosoftLogLevelConverter.Instance;
		var instance2 = SerilogToMicrosoftLogLevelConverter.Instance;

		// Assert
		Assert.That(instance1, Is.Not.Null);
		Assert.That(instance2, Is.Not.Null);
		Assert.That(instance1, Is.SameAs(instance2));
	}

	#region ToSerilogLevel Tests

	[TestCase(LogLevel.None, LogEventLevel.Fatal)]
	[TestCase(LogLevel.Trace, LogEventLevel.Verbose)]
	[TestCase(LogLevel.Debug, LogEventLevel.Debug)]
	[TestCase(LogLevel.Information, LogEventLevel.Information)]
	[TestCase(LogLevel.Warning, LogEventLevel.Warning)]
	[TestCase(LogLevel.Error, LogEventLevel.Error)]
	[TestCase(LogLevel.Critical, LogEventLevel.Fatal)]
	public void MicrosoftLevelToSerilogLevelConversion(LogLevel microsoftLevel, LogEventLevel expectedSerilogLevel)
	{
		// Arrange
		var converter = SerilogToMicrosoftLogLevelConverter.Instance;

		// Act
		var instanceResult = converter.ConvertTargetToSource(microsoftLevel);
		var staticResult = SerilogToMicrosoftLogLevelConverter.ToSerilogLevel(microsoftLevel);

		// Assert
		Assert.That(instanceResult, Is.EqualTo(expectedSerilogLevel).And.EqualTo(staticResult));
	}

	[Test]
	public void ToSerilogLevelShouldHandleInvalidLogLevelAsVerbose()
	{
		// Arrange
		var invalidLevel = (LogLevel) 999;

		// Act
		var result = SerilogToMicrosoftLogLevelConverter.ToSerilogLevel(invalidLevel);

		// Assert
		Assert.That(result, Is.EqualTo(LogEventLevel.Verbose));
	}

	#endregion

	#region ToMicrosoftLevel Tests

	[TestCase(LogEventLevel.Verbose, LogLevel.Trace)]
	[TestCase(LogEventLevel.Debug, LogLevel.Debug)]
	[TestCase(LogEventLevel.Information, LogLevel.Information)]
	[TestCase(LogEventLevel.Warning, LogLevel.Warning)]
	[TestCase(LogEventLevel.Error, LogLevel.Error)]
	[TestCase(LogEventLevel.Fatal, LogLevel.Critical)]
	public void SerilogLevelToMicrosoftLevelConversion(LogEventLevel serilogLevel, LogLevel expectedMicrosoftLevel)
	{
		// Arrange
		var converter = SerilogToMicrosoftLogLevelConverter.Instance;

		// Act
		var instanceResult = converter.ConvertSourceToTarget(serilogLevel);
		var staticResult = SerilogToMicrosoftLogLevelConverter.ToMicrosoftLevel(serilogLevel);

		// Assert
		Assert.That(instanceResult, Is.EqualTo(expectedMicrosoftLevel).And.EqualTo(staticResult));
	}

	[Test]
	public void ToMicrosoftLevelShouldHandleInvalidLogEventLevelAsTrace()
	{
		// Arrange
		var invalidLevel = (LogEventLevel) 999;

		// Act
		var result = SerilogToMicrosoftLogLevelConverter.ToMicrosoftLevel(invalidLevel);

		// Assert
		Assert.That(result, Is.EqualTo(LogLevel.Trace));
	}

	#endregion

	#region Round-Trip Tests

	[TestCase(LogLevel.Trace)]
	[TestCase(LogLevel.Debug)]
	[TestCase(LogLevel.Information)]
	[TestCase(LogLevel.Warning)]
	[TestCase(LogLevel.Error)]
	[TestCase(LogLevel.Critical)]
	public void RoundTripMicrosoftToSerilogAndBackShouldPreserveLevel(LogLevel originalLevel)
	{
		// Act
		var serilogLevel = SerilogToMicrosoftLogLevelConverter.ToSerilogLevel(originalLevel);
		var roundTripLevel = SerilogToMicrosoftLogLevelConverter.ToMicrosoftLevel(serilogLevel);

		// Assert
		Assert.That(roundTripLevel, Is.EqualTo(originalLevel));
	}

	[TestCase(LogEventLevel.Verbose)]
	[TestCase(LogEventLevel.Debug)]
	[TestCase(LogEventLevel.Information)]
	[TestCase(LogEventLevel.Warning)]
	[TestCase(LogEventLevel.Error)]
	public void RoundTripSerilogToMicrosoftAndBackShouldPreserveLevel(LogEventLevel originalLevel)
	{
		// Act
		var microsoftLevel = SerilogToMicrosoftLogLevelConverter.ToMicrosoftLevel(originalLevel);
		var roundTripLevel = SerilogToMicrosoftLogLevelConverter.ToSerilogLevel(microsoftLevel);

		// Assert
		Assert.That(roundTripLevel, Is.EqualTo(originalLevel));
	}

	#endregion

	#endregion
}