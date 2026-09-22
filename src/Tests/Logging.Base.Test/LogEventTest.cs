using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class LogEventTest
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

	enum MyLogLevel
	{
		Debug,
		Information,
		Warning,
		Error
	}

	class MyNoLogEvent : NoLogEvent<MyNoLogEvent, MyLogLevel, int>
	{
		/// <inheritdoc />
		public override int EventId => 0;

		/// <inheritdoc />
		public override MyLogLevel LogLevel => default;
	}

	#endregion

	#region Tests

	#region LogEvent

	[Test]
	[Category("LogEvent")]
	public void LogEventPropertiesAreInitializedCorrectly()
	{
		// Arrange
		var eventId = _fixture.Create<int>();
		var logLevel = MyLogLevel.Warning;
		var logMessage = "Test message {Parameter}";
		var args = new object?[] { "value" };

		// Act
		var logEvent = new LogEvent<MyLogLevel, int>(eventId, logLevel, logMessage, args);

		// Assert
		Assert.That(logEvent.EventId, Is.EqualTo(eventId));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
		Assert.That(logEvent.Args, Is.EqualTo(args));
		Assert.That(logEvent.Exception, Is.Null);
		Assert.That(logEvent.Payload, Is.Null);
	}

	[Test]
	[Category("LogEvent")]
	public void LogEventWithExceptionPropertiesAreInitializedCorrectly()
	{
		// Arrange
		var eventId = _fixture.Create<int>();
		var logLevel = MyLogLevel.Error;
		var logMessage = "Error message {Parameter}";
		var args = new object?[] { "value" };
		var exception = new Exception("Test exception");

		// Act
		var logEvent = new LogEvent<MyLogLevel, int>(eventId, exception, logLevel, logMessage, args);

		// Assert
		Assert.That(logEvent.EventId, Is.EqualTo(eventId));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
		Assert.That(logEvent.Args, Is.EqualTo(args));
		Assert.That(logEvent.Exception, Is.EqualTo(exception));
		Assert.That(logEvent.Payload, Is.Null);
	}

	[Test]
	[Category("LogEvent")]
	public void LogEventArgsDefaultsToEmptyArrayWhenNullIsPassed()
	{
		// Arrange + Act
		var logEvent = new LogEvent<MyLogLevel, int>(_fixture.Create<int>(), MyLogLevel.Information, "Message", null!);

		// Assert
		Assert.That(logEvent.Args, Is.Not.Null);
		Assert.That(logEvent.Args, Is.Empty);
	}

	[Test]
	[Category("LogEvent")]
	public void LogEventPayloadCanBeSetViaInit()
	{
		// Arrange
		var payload = Payload.Create(("Key", "Value"));

		// Act
		var logEvent = new LogEvent<MyLogLevel, int>(_fixture.Create<int>(), MyLogLevel.Information, "Message") { Payload = payload };

		// Assert
		Assert.That(logEvent.Payload, Is.EqualTo(payload));
	}

	[Test]
	[Category("LogEvent")]
	public void LogEventDeconstructsCorrectly()
	{
		// Arrange
		var eventId = _fixture.Create<int>();
		var logLevel = MyLogLevel.Warning;
		var logMessage = "Test message {Parameter}";
		var args = new object?[] { "value" };
		var exception = new Exception("Test exception");
		var payload = Payload.Create(("Key", "Value"));
		var logEvent = new LogEvent<MyLogLevel, int>(eventId, exception, logLevel, logMessage, args) { Payload = payload };

		// Act
		var (deconstructedEventId, deconstructedException, deconstructedLogLevel, deconstructedLogMessage, deconstructedArgs, deconstructedPayload) = logEvent;

		// Assert
		Assert.That(deconstructedEventId, Is.EqualTo(eventId));
		Assert.That(deconstructedLogLevel, Is.EqualTo(logLevel));
		Assert.That(deconstructedLogMessage, Is.EqualTo(logMessage));
		Assert.That(deconstructedArgs, Is.EqualTo(args));
		Assert.That(deconstructedException, Is.EqualTo(exception));
		Assert.That(deconstructedPayload, Is.EqualTo(payload));
	}

	#endregion

	#region NoLogEvent

	[Test]
	[Category("NoLogEvent")]
	public void NoLogEventInstanceIsAccessible()
	{
		// Arrange + Act
		var instance = MyNoLogEvent.Instance;

		// Assert
		Assert.That(instance, Is.Not.Null);
		Assert.That(instance, Is.InstanceOf<MyNoLogEvent>());
	}

	[Test]
	[Category("NoLogEvent")]
	public void NoLogEventInstanceIsSingleton()
	{
		// Arrange + Act
		var first = MyNoLogEvent.Instance;
		var second = MyNoLogEvent.Instance;

		// Assert
		Assert.That(first, Is.SameAs(second));
	}

	[Test]
	[Category("NoLogEvent")]
	public void NoLogEventDefaultPropertyValuesAreCorrect()
	{
		// Arrange + Act
		var instance = MyNoLogEvent.Instance;

		// Assert
		Assert.That(instance.LogMessage, Is.EqualTo(String.Empty));
		Assert.That(instance.Args, Is.Not.Null);
		Assert.That(instance.Args, Is.Empty);
		Assert.That(instance.Exception, Is.Null);
		Assert.That(instance.Payload, Is.Null);
	}

	[Test]
	[Category("NoLogEvent")]
	public void NoLogEventDeconstructsCorrectly()
	{
		// Arrange
		var instance = MyNoLogEvent.Instance;

		// Act
		var (eventId, exception, logLevel, logMessage, args, payload) = instance;

		// Assert
		Assert.That(eventId, Is.EqualTo(instance.EventId));
		Assert.That(logLevel, Is.EqualTo(instance.LogLevel));
		Assert.That(logMessage, Is.EqualTo(String.Empty));
		Assert.That(args, Is.Not.Null);
		Assert.That(args, Is.Empty);
		Assert.That(exception, Is.Null);
		Assert.That(payload, Is.Null);
	}

	#endregion

	#endregion
}