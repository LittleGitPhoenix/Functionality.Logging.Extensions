using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog;
#if !NET462
using Serilog.Sinks.InMemory;
#endif

namespace Serilog.Test;

public class ApplicationIdentifierEnricherTest
{
    [SetUp]
    public void Setup() { }

#if !NET462
    [Test]
    public void Check_If_Log_Has_Been_Enriched_With_ApplicationIdentifier()
    {
        // Arrange
        var identifier = Guid.NewGuid().ToString();
		var logger = Log.Logger = new LoggerConfiguration()
			.Enrich.WithApplicationIdentifier(identifier)
			.WriteTo.InMemory()
			.CreateLogger()
            ;

        // Act
        logger.Information(String.Empty);

        // Assert
        var logEvents = InMemorySink.Instance.LogEvents;
        Assert.That(logEvents, Has.Count.EqualTo(1), "Expected exactly one log event");        
        var logEvent = logEvents.First();
        Assert.That(logEvent.MessageTemplate.Text, Is.EqualTo(String.Empty), "Expected message to be empty");
        Assert.That(logEvent.Properties.ContainsKey(ApplicationIdentifierEnricher.PropertyName), Is.True, $"Expected log event to have property '{ApplicationIdentifierEnricher.PropertyName}'");
        Assert.That(logEvent.Properties[ApplicationIdentifierEnricher.PropertyName].ToString().Trim('"'), Is.EqualTo(identifier), "Expected property value to match identifier");
    }
#endif
}