using System.Globalization;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;
using l10nLocal = Microsoft.Test.Localization.l10n;

namespace Microsoft.Test;

public class EventIdResourceLoggerTest
{
    #region Data

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
    private IFixture _fixture;
#pragma warning restore 8618
		
    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    #endregion

    static void ChangeCulture(string cultureIdentifier)
    {
        var culture = new CultureInfo(cultureIdentifier);
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    /// <summary>
    /// Checks that the output message from <see cref="EventIdResourceLogger.LogEventFromResource"/> is properly translated.
    /// </summary>
    /// <param name="culture"> The destination culture in which the output message must be translated. </param>
    [Test]
    [TestCase("en")]
    [TestCase("de")]
    public void Check_Output_Message_Is_Translated(string cultureIdentifier)
    {
        // Arrange
        var resourceManager = l10nLocal.ResourceManager;
        _fixture.Inject(resourceManager);
        var logger = _fixture.Create<Mock<ILogger>>().Object;
        _fixture.Inject(logger);
        var eventLogger = _fixture.Create<Mock<EventIdResourceLogger>>().Object;
        var user = _fixture.Create<string>();
        var dataSetId = _fixture.Create<ushort>();
        var resourceName = nameof(l10nLocal.StartIteration);
        var destinationCulture = CultureInfo.CreateSpecificCulture(cultureIdentifier);
        var targetOutputMessage = String.Format(resourceManager.GetString(resourceName, destinationCulture), dataSetId);
        ChangeCulture(cultureIdentifier);

        // Act
        var outputMessage = eventLogger.LogEventFromResource(_fixture.Create<int>(), LogLevel.Debug, resourceName, new object[] { user, dataSetId }, new object[] { dataSetId });

        // Assert
        Assert.That(outputMessage, Is.EqualTo(targetOutputMessage));
    }
}