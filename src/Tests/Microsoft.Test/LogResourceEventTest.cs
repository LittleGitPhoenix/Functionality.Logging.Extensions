using System.Globalization;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

using l10nLocal = Microsoft.Test.Localization.l10n;

namespace Microsoft.Test;

public class LogResourceEventTest
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
	public void ResourceIsResolvedBasedOnUiCulture()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			var logEvent = new LogResourceEvent
			(
				_fixture.Create<int>(),
				LogLevel.Information,
				l10nLocal.ResourceManager,
				nameof(l10nLocal.MessageWithoutPlaceholders),				
				[],
				[],
				null
			);

			// Act + Assert
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
			Assert.That(logEvent.OutputMessage, Is.EqualTo("Hello World"));

			// Act + Assert
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de");
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
			Assert.That(logEvent.OutputMessage, Is.EqualTo("Hallo Welt"));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	public void InvalidResourceNameIsHandled()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			var logEvent = new LogResourceEvent
			(
				_fixture.Create<int>(),
				LogLevel.Information,
				l10nLocal.ResourceManager,
				"INVALID_RESOURCE_NAME_" + Guid.NewGuid().ToString().ToUpper(),
				[],
				[],
				null
			);

			// Act + Assert
			Assert.That(logEvent.LogMessage, Does.StartWith("No log-message found for resource"));
			Assert.That(logEvent.OutputMessage, Does.StartWith("No output-message found for resource"));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	public void ParametersFromLogSubstitudeMissingOutputParameters()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			var logEvent = new LogResourceEvent
			(
				_fixture.Create<int>(),
				LogLevel.Information,
				l10nLocal.ResourceManager,
				nameof(l10nLocal.MessageWithMatchingPlaceholders),
				[46, "John"],
				null, //! This is forcibly set to null to test that parameters from the log-message are used as fallback.
				null
			);

			// Act + Assert
			Assert.That(logEvent.LogMessage, Does.StartWith("The id {UserId} belongs to user {UserName}."));
			Assert.That(logEvent.OutputMessage, Does.StartWith("The id 46 belongs to user John."));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	public void MismatchingParametersAreHandled()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			var logEvent = new LogResourceEvent
			(
				_fixture.Create<int>(),
				LogLevel.Information,
				l10nLocal.ResourceManager,
				nameof(l10nLocal.MessageWithMatchingPlaceholders), //! This resource expects 2 parameters.
				[46], //! Only 1 parameter is provided for the log message.
				[46], //! Only 1 parameter is provided for the output message.
				null
			);

			// Act + Assert
			Assert.That(logEvent.LogMessage, Does.StartWith("The id {UserId} belongs to user {UserName}.")); //! The log-message is not affected by the parameter mismatch as it is not formatted.
			Assert.That(logEvent.OutputMessage, Does.StartWith("Could not format the output-message"));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	#endregion
}