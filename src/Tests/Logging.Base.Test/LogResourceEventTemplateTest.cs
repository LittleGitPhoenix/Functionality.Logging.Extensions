using System.Globalization;
using System.Resources;
using Phoenix.Functionality.Logging.Base;
using l10nLocal = Logging.Base.Test.Localization.l10n;

namespace Logging.Base.Test;

public class LogResourceEventTemplateTest
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

	internal enum MyLogLevel
	{
		Debug,
		Information,
		Warning,
		Error
	}

	internal interface IMyLogResourceEvent : ILogResourceEvent<MyLogLevel, int>;

	internal class MyLogResourceEvent : LogResourceEvent<MyLogLevel, int>, IMyLogResourceEvent
	{
		public MyLogResourceEvent(int eventId, MyLogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
			: base(eventId, logLevel, resourceManager, resourceName, args, outputArgs, logCulture) { }

		public MyLogResourceEvent(int eventId, Exception exception, MyLogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
			: base(eventId, exception, logLevel, resourceManager, resourceName, args, outputArgs, logCulture) { }
	}

	internal class MyLogResourceEventTemplate : LogResourceEventTemplate<IMyLogResourceEvent, MyLogLevel, int>
	{
		/// <inheritdoc />
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		protected internal override IMyLogResourceEvent CreateLogEvent(int eventId, ResourceManager resourceManager, string resourceName, MyLogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
			=> CreateLogEvent_Internal(eventId, resourceManager, resourceName, actualLogLevel, exception, payload, argsArray, outputArgs, actualLogCulture);

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		internal static IMyLogResourceEvent CreateLogEvent_Internal(int eventId, ResourceManager resourceManager, string resourceName, MyLogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
		{
			return exception is null
				? new MyLogResourceEvent(eventId, actualLogLevel, resourceManager, resourceName, argsArray, outputArgs, actualLogCulture) { Payload = payload }
				: new MyLogResourceEvent(eventId, exception, actualLogLevel, resourceManager, resourceName, argsArray, outputArgs, actualLogCulture) { Payload = payload }
				;
		}
	}

	internal class MyLogResourceEventTemplate<TArgs> : LogResourceEventTemplate<IMyLogResourceEvent, MyLogLevel, int, TArgs>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
		where TArgs : System.Runtime.CompilerServices.ITuple
#else
		where TArgs : struct
#endif
	{
		/// <inheritdoc />
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		protected internal override IMyLogResourceEvent CreateLogEvent(int eventId, ResourceManager resourceManager, string resourceName, MyLogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
			=> MyLogResourceEventTemplate.CreateLogEvent_Internal(eventId, resourceManager, resourceName, actualLogLevel, exception, payload, argsArray, outputArgs, actualLogCulture);
	}

	internal class MyLogResourceEventTemplate<TArgs, TOutputArgs> : LogResourceEventTemplate<IMyLogResourceEvent, MyLogLevel, int, TArgs, TOutputArgs>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
		where TArgs : System.Runtime.CompilerServices.ITuple
		where TOutputArgs : System.Runtime.CompilerServices.ITuple
#else
		where TArgs : struct
		where TOutputArgs : struct
#endif
	{
		/// <inheritdoc />
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		protected internal override IMyLogResourceEvent CreateLogEvent(int eventId, ResourceManager resourceManager, string resourceName, MyLogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
			=> MyLogResourceEventTemplate.CreateLogEvent_Internal(eventId, resourceManager, resourceName, actualLogLevel, exception, payload, argsArray, outputArgs, actualLogCulture);
	}

	#endregion

	#region Tests

	[Test]
	public void LogResourceEventTemplateCanBeCreated()
	{
		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate
				{
					EventId = 1563058623,
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
				};
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate<(DateTime Now, Unit)>
				{
					EventId = 985934159,
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithOnePlaceholder),
				};
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate<(int UserId, string UserName)>
				{
					EventId = 1907215190,
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithMatchingPlaceholders),
				};
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate<(int UserId, string UserName), (string UserName, Unit)>
				{
					EventId = 1207208689,
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
				};
			}
		);
	}

	#region Log Event Building

	[Test]
	[Category("LogEventBuilding")]
	public void LogResourceEventCanBeBuild()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = MyLogLevel.Information;
			var logMessage = "The default message.";
			var outputMessage = "Hello World";
			var template = new MyLogResourceEventTemplate
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build();
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level.
			var logLevel = MyLogLevel.Debug;
			logEvent = template.Build(logLevel);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload.
			var payload = Payload.Create(("Property", "Value"));
			logEvent = template.Build(payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception.
			var exception = new Exception();
			logEvent = template.Build(exception);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything.
			logEvent = template.Build(logLevel, exception, payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.EqualTo(exception));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	[Category("LogEventBuilding")]
	public void LogResourceEventWithSingleParameterCanBeBuild()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = MyLogLevel.Information;
			var logMessage = "Current time is {Now}.";
			var now = new DateTime(2024, 6, 15, 10, 30, 0);
			var outputMessage = $"Current time is {now}.";
			var template = new MyLogResourceEventTemplate<(DateTime Now, Unit)>
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithOnePlaceholder),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build((now, Unit.Value));
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level.
			var logLevel = MyLogLevel.Debug;
			logEvent = template.Build((now, Unit.Value), logLevel);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload.
			var payload = Payload.Create(("Property", "Value"));
			logEvent = template.Build((now, Unit.Value), payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception.
			var exception = new Exception();
			logEvent = template.Build((now, Unit.Value), exception);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything.
			logEvent = template.Build((now, Unit.Value), logLevel, exception, payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.EqualTo(exception));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	[Category("LogEventBuilding")]
	public void LogResourceEventWithMultipleMatchingParametersCanBeBuild()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = MyLogLevel.Information;
			var logMessage = "The id {UserId} belongs to user {UserName}.";
			var userId = _fixture.Create<int>();
			var userName = _fixture.Create<string>();
			var outputMessage = $"The id {userId} belongs to user {userName}.";
			var template = new MyLogResourceEventTemplate<(int UserId, string UserName)>
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithMatchingPlaceholders),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build((userId, userName));
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level.
			var logLevel = MyLogLevel.Debug;
			logEvent = template.Build((userId, userName), logLevel);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload.
			var payload = Payload.Create(("Property", "Value"));
			logEvent = template.Build((userId, userName), payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception.
			var exception = new Exception();
			logEvent = template.Build((userId, userName), exception);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything.
			logEvent = template.Build((userId, userName), logLevel, exception, payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.EqualTo(exception));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	[Category("LogEventBuilding")]
	public void LogResourceEventWithMultipleDifferentParametersCanBeBuild()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = MyLogLevel.Information;
			var logMessage = "The user {UserName} with id {UserId} is a human.";
			var userId = _fixture.Create<int>();
			var userName = _fixture.Create<string>();
			var outputMessage = $"The user {userName} is a human.";
			var template = new MyLogResourceEventTemplate<(int UserId, string UserName), (string UserName, Unit)>
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build((userId, userName), (userName, Unit.Value));
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level.
			var logLevel = MyLogLevel.Debug;
			logEvent = template.Build((userId, userName), (userName, Unit.Value), logLevel);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload.
			var payload = Payload.Create(("Property", "Value"));
			logEvent = template.Build((userId, userName), (userName, Unit.Value), payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception.
			var exception = new Exception();
			logEvent = template.Build((userId, userName), (userName, Unit.Value), exception);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything.
			logEvent = template.Build((userId, userName), (userName, Unit.Value), logLevel, exception, payload);
			Assert.That(logEvent.EventId, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(logMessage));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.Payload, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.EqualTo(exception));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	#endregion

	#region Culture

	[Test]
	[Category("Culture")]
	public void LogMessageUsesLogCultureAndOutputMessageUsesUiCulture()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		try
		{
			// Arrange
			var template = new MyLogResourceEventTemplate
			{
				EventId = _fixture.Create<int>(),
				LogLevel = MyLogLevel.Information,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
			};

			// Act + Assert: English UI culture.
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
			var logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
			Assert.That(logEvent.OutputMessage, Is.EqualTo("Hello World"));

			// Act + Assert: German UI culture.
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de");
			logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
			Assert.That(logEvent.OutputMessage, Is.Not.EqualTo("Hello World"));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	[Category("Culture")]
	public void LogCultureCanBeOverriddenPerCall()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var template = new MyLogResourceEventTemplate
			{
				EventId = _fixture.Create<int>(),
				LogLevel = MyLogLevel.Information,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
			};

			// Act + Assert: Default log culture (lo).
			var logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));

			// Act + Assert: Override log culture to German.
			logEvent = template.Build(actualLogCulture: CultureInfo.GetCultureInfo("de"));
			Assert.That(logEvent.LogMessage, Is.Not.EqualTo("The default message."));

			// Act + Assert: Explicit null as culture.
			logEvent = template.Build(actualLogCulture: null);
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	[Category("Culture")]
	public void LogCultureCanBeSpecifiedGlobally()
	{
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var template = new MyLogResourceEventTemplate
			{
				EventId = _fixture.Create<int>(),
				LogLevel = MyLogLevel.Information,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
			};

			// Act + Assert: No overridden culture.
			var logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));

			// Act + Assert: Other culture.
			LogResourceEventSettings.LogCulture = CultureInfo.GetCultureInfo("de");
			logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.Not.EqualTo("The default message."));

			// Act + Assert: Explicitly use null.
			LogResourceEventSettings.LogCulture = null!;
			logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
		}
		finally
		{
			LogResourceEventSettings.LogCulture = null!;
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	#endregion

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER
	#region Constructor Validation

	[Test]
	[Category("ConstructorValidation")]
	public void LogResourceEventThrowsForInvalidStructTypes()
	{
		Assert.Throws<InvalidOperationException>
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate<int>
				{
					EventId = _fixture.Create<int>(),
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithOnePlaceholder),
				};
			}
		);

		Assert.Throws<InvalidOperationException>
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate<int, (int Value, Unit)>
				{
					EventId = _fixture.Create<int>(),
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
				};
			}
		);

		Assert.Throws<InvalidOperationException>
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate<(int UserId, string UserName), Guid>
				{
					EventId = _fixture.Create<int>(),
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
				};
			}
		);

		Assert.Throws<InvalidOperationException>
		(
			() =>
			{
				_ = new MyLogResourceEventTemplate<int, Guid>
				{
					EventId = _fixture.Create<int>(),
					LogLevel = MyLogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
				};
			}
		);
	}

	#endregion
#endif

	#endregion
}
