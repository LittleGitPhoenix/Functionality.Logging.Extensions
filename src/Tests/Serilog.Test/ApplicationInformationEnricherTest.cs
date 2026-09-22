using Phoenix.Functionality.Logging.Base;
using Phoenix.Functionality.Logging.Extensions.Serilog;
#if !NET462
using Serilog.Sinks.InMemory;
#endif

namespace Serilog.Test;

[Category("ApplicationInformation")]
public class ApplicationInformationEnricherTest
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

#if !NET462

	private Serilog.Events.LogEvent CreateLogEvent(LogApplicationInformation applicationInformation, ApplicationInformationEnricher.LogApplicationInformationParts propertiesToLog, Func<string?, string?>? versionModificationCallback = null)
	{
		var initialEventCount = InMemorySink.Instance.LogEvents.Count();
		var logger = new LoggerConfiguration()
			.Enrich.WithApplicationInformation(applicationInformation, propertiesToLog, versionModificationCallback)
			.WriteTo.InMemory()
			.CreateLogger()
			;

		logger.Information(String.Empty);

		return InMemorySink.Instance.LogEvents.Skip(initialEventCount).Single();
	}

#endif
	#endregion

	#region Tests

#if !NET462

	/// <summary>
	/// Verifies that the selected application information properties are added to the log event.
	/// </summary>
	[Test]
	public void Enrich_AddsSelectedApplicationInformation()
	{
		// Arrange
		var applicationInformation = LogApplicationInformation.Create().StartingWith("TestApplication").Build();
		var propertiesToLog = ApplicationInformationEnricher.LogApplicationInformationParts.Name
			| ApplicationInformationEnricher.LogApplicationInformationParts.NumericIdentifier
			| ApplicationInformationEnricher.LogApplicationInformationParts.AlphanumericIdentifier;

		// Act
		var logEvent = this.CreateLogEvent(applicationInformation, propertiesToLog);

		// Assert
		Assert.That(logEvent.Properties[ApplicationInformationEnricher.ApplicationNamePropertyName].ToString().Trim('"'), Is.EqualTo(applicationInformation.Name));
		Assert.That(logEvent.Properties[ApplicationInformationEnricher.ApplicationIdPropertyName].ToString(), Is.EqualTo(applicationInformation.NumericIdentifier.ToString()));
		Assert.That(logEvent.Properties[ApplicationInformationEnricher.ApplicationIdentifierPropertyName].ToString().Trim('"'), Is.EqualTo(applicationInformation.AlphanumericIdentifier));
		Assert.That(logEvent.Properties.ContainsKey(ApplicationInformationEnricher.ApplicationVersionPropertyName), Is.False);
	}

	/// <summary>
	/// Verifies that no properties are added when no application information parts are selected.
	/// </summary>
	[Test]
	public void Enrich_WithNoSelectedParts_DoesNotAddApplicationInformation()
	{
		// Arrange
		var applicationInformation = LogApplicationInformation.Create().StartingWith("TestApplication").Build();

		// Act
		var logEvent = this.CreateLogEvent(applicationInformation, ApplicationInformationEnricher.LogApplicationInformationParts.None);

		// Assert
		Assert.That(logEvent.Properties.ContainsKey(ApplicationInformationEnricher.ApplicationNamePropertyName), Is.False);
		Assert.That(logEvent.Properties.ContainsKey(ApplicationInformationEnricher.ApplicationIdPropertyName), Is.False);
		Assert.That(logEvent.Properties.ContainsKey(ApplicationInformationEnricher.ApplicationIdentifierPropertyName), Is.False);
		Assert.That(logEvent.Properties.ContainsKey(ApplicationInformationEnricher.ApplicationVersionPropertyName), Is.False);
	}

	/// <summary>
	/// Verifies that each version flag enriches the event with its corresponding application version.
	/// </summary>
	[Test]
	public void Enrich_WithVersionPart_AddsSelectedApplicationVersion()
	{
		// Arrange
		var applicationInformation = LogApplicationInformation.Create().StartingWith("TestApplication").Build();
		var versionParts = new[]
		{
			(ApplicationInformationEnricher.LogApplicationInformationParts.AssemblyVersion, applicationInformation.AssemblyVersion?.ToString() ?? "unknown"),
			(ApplicationInformationEnricher.LogApplicationInformationParts.FileVersion, applicationInformation.FileVersion?.ToString() ?? "unknown"),
			(ApplicationInformationEnricher.LogApplicationInformationParts.InformationalVersion, applicationInformation.InformationalVersion ?? "unknown"),
		};

		foreach (var (versionPart, expectedVersion) in versionParts)
		{
			// Act
			var logEvent = this.CreateLogEvent(applicationInformation, versionPart);

			// Assert
			Assert.That(logEvent.Properties[ApplicationInformationEnricher.ApplicationVersionPropertyName].ToString().Trim('"'), Is.EqualTo(expectedVersion));
		}
	}

	/// <summary>
	/// Verifies that the assembly version takes precedence when multiple version flags are selected.
	/// </summary>
	[Test]
	public void Enrich_WithMultipleVersionParts_UsesAssemblyVersion()
	{
		// Arrange
		var applicationInformation = LogApplicationInformation.Create().StartingWith("TestApplication").Build();
		var propertiesToLog = ApplicationInformationEnricher.LogApplicationInformationParts.AssemblyVersion
			| ApplicationInformationEnricher.LogApplicationInformationParts.FileVersion
			| ApplicationInformationEnricher.LogApplicationInformationParts.InformationalVersion;

		// Act
		var logEvent = this.CreateLogEvent(applicationInformation, propertiesToLog);

		// Assert
		Assert.That(logEvent.Properties[ApplicationInformationEnricher.ApplicationVersionPropertyName].ToString().Trim('"'), Is.EqualTo(applicationInformation.AssemblyVersion?.ToString() ?? "unknown"));
	}

	/// <summary>
	/// Verifies that the version callback is applied and that a missing version uses the unknown fallback.
	/// </summary>
	[Test]
	public void Enrich_WithVersionCallback_AppliesCallbackAndUnknownFallback()
	{
		// Arrange
		const string modifiedVersion = "modified-version";

		// Act
		var modifiedLogEvent = this.CreateLogEvent(LogApplicationInformation.None, ApplicationInformationEnricher.LogApplicationInformationParts.InformationalVersion, (string? _) => modifiedVersion);
		var fallbackLogEvent = this.CreateLogEvent(LogApplicationInformation.None, ApplicationInformationEnricher.LogApplicationInformationParts.InformationalVersion);

		// Assert
		Assert.That(modifiedLogEvent.Properties[ApplicationInformationEnricher.ApplicationVersionPropertyName].ToString().Trim('"'), Is.EqualTo(modifiedVersion));
		Assert.That(fallbackLogEvent.Properties[ApplicationInformationEnricher.ApplicationVersionPropertyName].ToString().Trim('"'), Is.EqualTo("unknown"));
	}

#endif
	#endregion

}
#pragma warning restore CS0618