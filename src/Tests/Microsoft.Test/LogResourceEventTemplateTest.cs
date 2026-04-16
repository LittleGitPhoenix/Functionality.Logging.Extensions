using System.Globalization;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

using l10nLocal = Microsoft.Test.Localization.l10n;

namespace Microsoft.Test;

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
	public void AfterAllTest() { }

	#endregion

	#region Data
	#endregion

	#region Tests

	[Test]
	public void LogResourceEventTemplateCanBeCreated()
	{
		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new LogResourceEventTemplate()
				{
					EventId = 1563058623,
					LogLevel = LogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
				};
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new LogResourceEventTemplate<(DateTime Now, Unit)>()
				{
					EventId = 985934159,
					LogLevel = LogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithOnePlaceholder),
				};
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new LogResourceEventTemplate<(int UserId, string UserName)>()
				{
					EventId = 1907215190,
					LogLevel = LogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithMatchingPlaceholders),
				};
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new LogResourceEventTemplate<(string UserName, int UserId), (string UserName, Unit)>()
				{
					EventId = 1207208689,
					LogLevel = LogLevel.Information,
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
		// Change the ui to english culuture to ensure resource lookup works as expected.
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = LogLevel.Information;
			var message = "The default message.";
			var outputMessage = "Hello World";
			var template = new LogResourceEventTemplate
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build();
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level
			var logLevel = LogLevel.Debug;
			logEvent = template.Build(logLevel);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload
			var payload = LogScope.CreateIndependent(("Property", "Value"));
			logEvent = template.Build(payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception
			var exception = new Exception();
			logEvent = template.Build(exception);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything		
			logEvent = template.Build(logLevel, exception, payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Is.Empty);
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
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
		// Change the ui to english culuture to ensure resource lookup works as expected.
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = LogLevel.Information;
			var message = "Current time is {Now}.";
			var now = DateTime.UtcNow;
			var outputMessage = String.Format("Current time is {0}.", now);
			var template = new LogResourceEventTemplate<(DateTime Now, Unit)>
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithOnePlaceholder),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build((now, Unit.Value));
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level
			var logLevel = LogLevel.Debug;
			logEvent = template.Build((now, Unit.Value), logLevel);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload
			var payload = LogScope.CreateIndependent(("Property", "Value"));
			logEvent = template.Build((now, Unit.Value), payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception
			var exception = new Exception();
			logEvent = template.Build((now, Unit.Value), exception);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything		
			logEvent = template.Build((now, Unit.Value), logLevel, exception, payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(1));
			Assert.That(logEvent.Args[0], Is.EqualTo(now));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
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
		// Change the ui to english culuture to ensure resource lookup works as expected.
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = LogLevel.Information;
			var message = "The id {UserId} belongs to user {UserName}.";
			var userId = _fixture.Create<int>();
			var userName = _fixture.Create<string>();
			var outputMessage = String.Format("The id {0} belongs to user {1}.", userId, userName);
			var template = new LogResourceEventTemplate<(int UserId, string UserName)>
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithMatchingPlaceholders),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build((userId, userName));
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level
			var logLevel = LogLevel.Debug;
			logEvent = template.Build((userId, userName), logLevel);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload
			var payload = LogScope.CreateIndependent(("Property", "Value"));
			logEvent = template.Build((userId, userName), payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception
			var exception = new Exception();
			logEvent = template.Build((userId, userName), exception);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything		
			logEvent = template.Build((userId, userName), logLevel, exception, payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
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
		// Change the ui to english culuture to ensure resource lookup works as expected.
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = LogLevel.Information;
			var message = "The user {UserName} with id {UserId} is a human.";
			var userId = _fixture.Create<int>();
			var userName = _fixture.Create<string>();
			var outputMessage = String.Format("The user {0} is a human.", userName);
			var template = new LogResourceEventTemplate<(int UserId, string UserName), (string UserName, Unit)>
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
			};

			// Act + Assert: Direct Build.
			var logEvent = template.Build((userId, userName), (userName, Unit.Value));
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Different log level
			var logLevel = LogLevel.Debug;
			logEvent = template.Build((userId, userName), (userName, Unit.Value), logLevel);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Payload
			var payload = LogScope.CreateIndependent(("Property", "Value"));
			logEvent = template.Build((userId, userName), (userName, Unit.Value), payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.Null);

			// Act + Assert: Exception
			var exception = new Exception();
			logEvent = template.Build((userId, userName), (userName, Unit.Value), exception);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(level));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.Null);
			Assert.That(logEvent.Exception, Is.EqualTo(exception));

			// Act + Assert: Everything		
			logEvent = template.Build((userId, userName), (userName, Unit.Value), logLevel, exception, payload);
			Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
			Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
			Assert.That(logEvent.LogMessage, Is.EqualTo(message));
			Assert.That(logEvent.Args, Has.Length.EqualTo(2));
			Assert.That(logEvent.Args[0], Is.EqualTo(userId));
			Assert.That(logEvent.Args[1], Is.EqualTo(userName));
			Assert.That(logEvent.OutputMessage, Is.EqualTo(outputMessage));
			Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
			Assert.That(logEvent.Exception, Is.EqualTo(exception));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	#endregion

	[Test]
	[Category("CustomLogResource")]
	public void LogResourceCanBeSpecifiedGlobally()
	{
		// Change the ui to english culuture to ensure resource lookup works as expected.
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = LogLevel.Information;
			var template = new LogResourceEventTemplate
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
			};

			// Act + Assert: No overriden culture.
			var logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));

			// Act + Assert: Other culture.
			LogResourceEvent.LogCulture = CultureInfo.GetCultureInfo("de");
			logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("Hallo Welt"));

			// Act + Assert: Explicitly use NUll.
			LogResourceEvent.LogCulture = null!;
			logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
		}
		finally
		{
			CultureInfo.CurrentUICulture = previousCulture;
		}
	}

	[Test]
	[Category("CustomLogResource")]
	public void LogResourceCanBeSpecifiedPerCall()
	{
		// Change the ui to english culuture to ensure resource lookup works as expected.
		var previousCulture = CultureInfo.CurrentUICulture;
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
		try
		{
			// Arrange
			var id = _fixture.Create<int>();
			var level = LogLevel.Information;
			var template = new LogResourceEventTemplate
			{
				EventId = id,
				LogLevel = level,
				ResourceManager = l10nLocal.ResourceManager,
				ResourceName = nameof(l10nLocal.MessageWithoutPlaceholders),
			};

			// Act + Assert: No overriden culture.
			var logEvent = template.Build();
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));

			// Act + Assert: Other culture.
			logEvent = template.Build(actualLogCulture: CultureInfo.GetCultureInfo("de"));
			Assert.That(logEvent.LogMessage, Is.EqualTo("Hallo Welt"));

			// Act + Assert: Explicit NUll as culture.
			logEvent = template.Build(actualLogCulture: null);
			Assert.That(logEvent.LogMessage, Is.EqualTo("The default message."));
		}
		finally
		{
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
				_ = new LogResourceEventTemplate<int>()
				{
					EventId = _fixture.Create<int>(),
					LogLevel = LogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithOnePlaceholder),
				};			
			}
		);

		Assert.Throws<InvalidOperationException>
		(
			() =>
			{
				_ = new LogResourceEventTemplate<int, (int Value, Unit)>()
				{
					EventId = _fixture.Create<int>(),
					LogLevel = LogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
				};
			}
		);

		Assert.Throws<InvalidOperationException>
		(
			() =>
			{
				_ = new LogResourceEventTemplate<(int UserId, string UserName), Guid>()
				{
					EventId = _fixture.Create<int>(),
					LogLevel = LogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
				};
			}
		);

		Assert.Throws<InvalidOperationException>
		(
			() =>
			{
				_ = new LogResourceEventTemplate<int, Guid>()
				{
					EventId = _fixture.Create<int>(),
					LogLevel = LogLevel.Information,
					ResourceManager = l10nLocal.ResourceManager,
					ResourceName = nameof(l10nLocal.MessageWithDifferentPlaceholders),
				};
			}
		);

	}

	#endregion
#endif
}