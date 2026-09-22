using System.Globalization;
using System.Resources;
using Phoenix.Functionality.Logging.Base;
using l10nLocal = Logging.Base.Test.Localization.l10n;

namespace Logging.Base.Test;

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

	enum MyLogLevel
	{
		Debug,
		Information,
		Warning,
		Error
	}

	interface IMyLogResourceEvent : ILogResourceEvent<MyLogLevel, int>;

	class MyLogResourceEvent : LogResourceEvent<MyLogLevel, int>, IMyLogResourceEvent
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="eventId"> <inheritdoc cref="ILogEvent{TLogLevel,int}.int"/> </param>
		/// <param name="logLevel"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogLevel"/> </param>
		/// <param name="resourceManager"> The <see cref="System.Resources.ResourceManager"/> from where log and output message is obtained. </param>
		/// <param name="resourceName"> The name of the resource in the <paramref name="resourceManager"/>. </param>
		/// <param name="args"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Args"/> </param>
		/// <param name="outputArgs"> Format arguments for the <see cref="ILogResourceEvent{TLogLevel,TEventId}.OutputMessage"/>. If this is <see langword="null"/> <paramref name="args"/> will be used instead. </param>
		/// <param name="logCulture"> The culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
		public MyLogResourceEvent(int eventId, MyLogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
			: base(eventId, logLevel, resourceManager, resourceName, args, outputArgs, logCulture) { }

		/// <summary>
		/// Constructor with exception
		/// </summary>
		/// <param name="eventId"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.EventId"/> </param>
		/// <param name="exception"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Exception"/> </param>
		/// <param name="logLevel"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogLevel"/> </param>
		/// <param name="resourceManager"> The <see cref="System.Resources.ResourceManager"/> from where log and output message is obtained. </param>
		/// <param name="resourceName"> The name of the resource in the <paramref name="resourceManager"/>. </param>
		/// <param name="args"> <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.Args"/> </param>
		/// <param name="outputArgs"> Format arguments for the <see cref="ILogResourceEvent{TLogLevel,TEventId}.OutputMessage"/>. If this is <see langword="null"/> <paramref name="args"/> will be used instead. </param>
		/// <param name="logCulture"> The culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
		public MyLogResourceEvent(int eventId, Exception exception, MyLogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
			: base(eventId, exception, logLevel, resourceManager, resourceName, args, outputArgs, logCulture) { }
	}

	class MyNoLogResourceEvent : NoLogResourceEvent<MyNoLogResourceEvent, MyLogLevel, int>
	{
		/// <inheritdoc />
		public override int EventId => default;

		/// <inheritdoc />
		public override MyLogLevel LogLevel => default;
	}

	#endregion

	#region Tests

	#region LogResourceEvent

	[Test]
	[Category("LogResourceEvent")]
	public void LogResourceEventPropertiesAreInitializedCorrectly()
	{
		// Arrange
		var eventId = _fixture.Create<int>();
		var logLevel = MyLogLevel.Warning;
		var args = new object?[] { 42, "John" };

		// Act
		var logEvent = new MyLogResourceEvent(eventId, logLevel, l10nLocal.ResourceManager, nameof(l10nLocal.MessageWithMatchingPlaceholders), args);

		// Assert
		Assert.That(logEvent.EventId, Is.EqualTo(eventId));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.Args, Is.EqualTo(args));
		Assert.That(logEvent.Exception, Is.Null);
		Assert.That(logEvent.Payload, Is.Null);
	}

	[Test]
	[Category("LogResourceEvent")]
	public void LogResourceEventWithExceptionPropertiesAreInitializedCorrectly()
	{
		// Arrange
		var eventId = _fixture.Create<int>();
		var logLevel = MyLogLevel.Error;
		var args = new object?[] { 42, "John" };
		var exception = new Exception("Test exception");

		// Act
		var logEvent = new MyLogResourceEvent(eventId, exception, logLevel, l10nLocal.ResourceManager, nameof(l10nLocal.MessageWithMatchingPlaceholders), args);

		// Assert
		Assert.That(logEvent.EventId, Is.EqualTo(eventId));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.Args, Is.EqualTo(args));
		Assert.That(logEvent.Exception, Is.EqualTo(exception));
		Assert.That(logEvent.Payload, Is.Null);
	}

	[Test]
	[Category("LogResourceEvent")]
	public void LogResourceEventArgsDefaultsToEmptyArrayWhenNullIsPassed()
	{
		// Arrange + Act
		var logEvent = new MyLogResourceEvent(_fixture.Create<int>(), MyLogLevel.Information, l10nLocal.ResourceManager, nameof(l10nLocal.MessageWithoutPlaceholders), null);

		// Assert
		Assert.That(logEvent.Args, Is.Not.Null);
		Assert.That(logEvent.Args, Is.Empty);
	}

	[Test]
	[Category("LogResourceEvent")]
	public void LogResourceEventPayloadCanBeSetViaInit()
	{
		// Arrange
		var payload = Payload.Create(("Key", "Value"));

		// Act
		var logEvent = new MyLogResourceEvent(_fixture.Create<int>(), MyLogLevel.Information, l10nLocal.ResourceManager, nameof(l10nLocal.MessageWithoutPlaceholders)) { Payload = payload };

		// Assert
		Assert.That(logEvent.Payload, Is.EqualTo(payload));
	}

	[Test]
	[Category("LogResourceEvent")]
	public void LogResourceEventDeconstructsCorrectly()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
			var eventId = _fixture.Create<int>();
			var logLevel = MyLogLevel.Warning;
			var args = new object?[] { 42, "John" };
			var exception = new Exception("Test exception");
			var payload = Payload.Create(("Key", "Value"));
			var logEvent = new MyLogResourceEvent(eventId, exception, logLevel, l10nLocal.ResourceManager, nameof(l10nLocal.MessageWithMatchingPlaceholders), args) { Payload = payload };

			// Act
			var (deconstructedEventId, deconstructedException, deconstructedLogLevel, deconstructedLogMessage, deconstructedArgs, deconstructedOutputMessage, deconstructedPayload) = logEvent;

			// Assert
			Assert.That(deconstructedEventId, Is.EqualTo(eventId));
			Assert.That(deconstructedLogLevel, Is.EqualTo(logLevel));
			Assert.That(deconstructedArgs, Is.EqualTo(args));
			Assert.That(deconstructedException, Is.EqualTo(exception));
			Assert.That(deconstructedLogMessage, Is.EqualTo(logEvent.LogMessage));
			Assert.That(deconstructedOutputMessage, Is.EqualTo("The id 42 belongs to user John."));
			Assert.That(deconstructedPayload, Is.EqualTo(payload));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	public void ResourceIsResolvedBasedOnUiCulture()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			var logEvent = new MyLogResourceEvent
			(
				_fixture.Create<int>(),
				MyLogLevel.Information,
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
			var logEvent = new MyLogResourceEvent
			(
				_fixture.Create<int>(),
				MyLogLevel.Information,
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
	public void ParametersFromLogSubstituteMissingOutputParameters()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			var logEvent = new MyLogResourceEvent
			(
				_fixture.Create<int>(),
				MyLogLevel.Information,
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
			var logEvent = new MyLogResourceEvent
			(
				_fixture.Create<int>(),
				MyLogLevel.Information,
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

	#region NoLogResourceEvent

	[Test]
	[Category("NoLogResourceEvent")]
	public void NoLogResourceEventInstanceIsAccessible()
	{
		// Arrange + Act
		var instance = MyNoLogResourceEvent.Instance;

		// Assert
		Assert.That(instance, Is.Not.Null);
		Assert.That(instance, Is.InstanceOf<MyNoLogResourceEvent>());
	}

	[Test]
	[Category("NoLogResourceEvent")]
	public void NoLogResourceEventInstanceIsSingleton()
	{
		// Arrange + Act
		var first = MyNoLogResourceEvent.Instance;
		var second = MyNoLogResourceEvent.Instance;

		// Assert
		Assert.That(first, Is.SameAs(second));
	}

	[Test]
	[Category("NoLogResourceEvent")]
	public void NoLogResourceEventDefaultPropertyValuesAreCorrect()
	{
		// Arrange + Act
		var instance = MyNoLogResourceEvent.Instance;

		// Assert
		Assert.That(instance.LogMessage, Is.EqualTo(String.Empty));
		Assert.That(((ILogResourceEvent<MyLogLevel, int>) instance).OutputMessage, Is.EqualTo(String.Empty));
		Assert.That(instance.Args, Is.Not.Null);
		Assert.That(instance.Args, Is.Empty);
		Assert.That(instance.Exception, Is.Null);
		Assert.That(instance.Payload, Is.Null);
	}

	[Test]
	[Category("NoLogResourceEvent")]
	public void NoLogResourceEventDeconstructsCorrectly()
	{
		// Arrange
		var instance = (ILogResourceEvent<MyLogLevel, int>) MyNoLogResourceEvent.Instance;

		// Act
		var (eventId, exception, logLevel, logMessage, args, outputMessage, payload) = instance;

		// Assert
		Assert.That(eventId, Is.EqualTo(instance.EventId));
		Assert.That(logLevel, Is.EqualTo(instance.LogLevel));
		Assert.That(logMessage, Is.EqualTo(String.Empty));
		Assert.That(outputMessage, Is.EqualTo(String.Empty));
		Assert.That(args, Is.Not.Null);
		Assert.That(args, Is.Empty);
		Assert.That(exception, Is.Null);
		Assert.That(payload, Is.Null);
	}

	#endregion

	#endregion
}