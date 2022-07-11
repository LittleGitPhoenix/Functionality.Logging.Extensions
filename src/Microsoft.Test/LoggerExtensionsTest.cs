using System.Globalization;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

using l10nLocal = Microsoft.Test.Localization.l10n;
using LoggerExtensions = Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerExtensions;

namespace Microsoft.Test;

public class LoggerExtensionsTest
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

	/// <summary>
	/// Checks that logging f rom the extension methods does not throw exceptions.
	/// </summary>
	[Test]
	public void LogDoesNotThrowExceptionDueToArgumentMismatch()
	{
		// Arrange
		var logger = _fixture.Create<Mock<ILogger>>().Object;
		Mock.Get(logger)
			.Setup
			(
				mock => mock.Log
				(
					It.IsAny<LogLevel>(),
					It.IsAny<EventId>(),
					It.IsAny<It.IsAnyType>(),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception?, string>) It.IsAny<object>()
				)
			)
			.Throws<Exception>()
			;

		// Act + Assert
		Assert.DoesNotThrow(() => LoggerExtensions.Log(logger, (EventId) 0, null, LogLevel.Information, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()));
	}

	/// <summary>
	/// Checks that the output message from <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerExtensions.LogEventFromResource"/> is properly translated.
	/// </summary>
	/// <param name="culture"> The destination culture in which the output message must be translated. </param>
	[Test]
	[TestCase("en")]
	[TestCase("de")]
	public void OutputMessageIsTranslated(string cultureIdentifier)
	{
		void ChangeCulture(string ci)
		{
			var culture = new CultureInfo(ci);
			CultureInfo.CurrentUICulture = culture;
			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
		}

		// Arrange
		var resourceManager = l10nLocal.ResourceManager;
		var resourceName = nameof(l10nLocal.StartIteration);
		var destinationCulture = CultureInfo.CreateSpecificCulture(cultureIdentifier);
		var user = _fixture.Create<string>();
		var dataSetId = _fixture.Create<ushort>();
		var logger = _fixture.Create<Mock<ILogger>>().Object;
		var targetOutputMessage = String.Format(resourceManager.GetString(resourceName, destinationCulture), dataSetId);
		ChangeCulture(cultureIdentifier);

		// Act
		var outputMessage = logger.Log(_fixture.Create<int>(), LogLevel.Debug, resourceManager, resourceName, new object[] { user, dataSetId }, new object[] { dataSetId });

		// Assert
		Assert.That(outputMessage, Is.EqualTo(targetOutputMessage));
	}

	#endregion
}