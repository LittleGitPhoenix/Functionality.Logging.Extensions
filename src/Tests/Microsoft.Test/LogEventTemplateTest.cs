using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class LogEventTemplateTest
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
	[Category("PropertyInitialization")]
	public void LogEventTemplatePropertiesAreInitializedCorrectly()
	{
		// Arrange
		var eventId = new EventId(12345, "TestEvent");
		var logLevel = LogLevel.Warning;
		var logMessage = "Test message with {Parameter}";

		// Act
		var template = new LogEventTemplate
		{
			EventId = eventId,
			LogLevel = logLevel,
			LogMessage = logMessage,
		};

		// Assert
		Assert.That(template.EventId, Is.EqualTo(eventId));
		Assert.That(template.EventId.Id, Is.EqualTo(12345));
		Assert.That(template.EventId.Name, Is.EqualTo("TestEvent"));
		Assert.That(template.LogLevel, Is.EqualTo(logLevel));
		Assert.That(template.LogMessage, Is.EqualTo(logMessage));
	}

	[Test]
	[Category("PropertyInitialization")]
	public void LogEventTemplateGenericPropertiesAreInitializedCorrectly()
	{
		// Arrange
		var eventId = new EventId(67890, "GenericTestEvent");
		var logLevel = LogLevel.Error;
		var logMessage = "User {UserId} performed action {Action}";

		// Act
		var template = new LogEventTemplate<(int UserId, string Action)>
		{
			EventId = eventId,
			LogLevel = logLevel,
			LogMessage = logMessage,
		};

		// Assert
		Assert.That(template.EventId, Is.EqualTo(eventId));
		Assert.That(template.EventId.Id, Is.EqualTo(67890));
		Assert.That(template.EventId.Name, Is.EqualTo("GenericTestEvent"));
		Assert.That(template.LogLevel, Is.EqualTo(logLevel));
		Assert.That(template.LogMessage, Is.EqualTo(logMessage));
	}

	[Test]
	[Category("NullHandling")]
	public void LogEventWithNullStringParameterCanBeBuild()
	{
		// Arrange
		var template = new LogEventTemplate<(int UserId, string? UserName)>
		{
			EventId = 11111,
			LogLevel = LogLevel.Information,
			LogMessage = "User {UserId} with name {UserName}",
		};

		// Act
		var args = (UserId: 42, UserName: (string?) null);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.EventId.Id, Is.EqualTo(11111));
		Assert.That(logEvent.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Args[1], Is.Null);
	}

	[Test]
	[Category("NullHandling")]
	public void LogEventWithMultipleNullParametersCanBeBuild()
	{
		// Arrange
		var template = new LogEventTemplate<(string? FirstName, string? LastName, int? Age)>
		{
			EventId = 22222,
			LogLevel = LogLevel.Warning,
			LogMessage = "Person: {FirstName} {LastName}, Age: {Age}",
		};

		// Act
		var args = (FirstName: (string?) null, LastName: (string?) null, Age: (int?) null);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.EventId.Id, Is.EqualTo(22222));
		Assert.That(logEvent.Args, Has.Length.EqualTo(3));
		Assert.That(logEvent.Args[0], Is.Null);
		Assert.That(logEvent.Args[1], Is.Null);
		Assert.That(logEvent.Args[2], Is.Null);
	}

	[Test]
	[Category("NullHandling")]
	public void LogEventWithMixedNullAndNonNullParametersCanBeBuild()
	{
		// Arrange
		var template = new LogEventTemplate<(int UserId, string? Email, string UserName, int? Age)>
		{
			EventId = 33333,
			LogLevel = LogLevel.Debug,
			LogMessage = "User {UserId}, Email: {Email}, Name: {UserName}, Age: {Age}",
		};

		// Act
		var args = (UserId: 123, Email: (string?) null, UserName: "John", Age: (int?) null);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.EventId.Id, Is.EqualTo(33333));
		Assert.That(logEvent.Args, Has.Length.EqualTo(4));
		Assert.That(logEvent.Args[0], Is.EqualTo(123));
		Assert.That(logEvent.Args[1], Is.Null);
		Assert.That(logEvent.Args[2], Is.EqualTo("John"));
		Assert.That(logEvent.Args[3], Is.Null);
	}

	[Test]
	[Category("ArgsArrayCorrectness")]
	public void ArgsArrayMaintainsTupleElementOrder()
	{
		// Arrange
		var template = new LogEventTemplate<(int First, string Second, bool Third, double Fourth)>
		{
			EventId = 44444,
			LogLevel = LogLevel.Information,
			LogMessage = "{First}, {Second}, {Third}, {Fourth}",
		};

		// Act
		var args = (First: 100, Second: "test", Third: true, Fourth: 3.14);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.Args, Has.Length.EqualTo(4));
		Assert.That(logEvent.Args[0], Is.EqualTo(100), "First element order mismatch");
		Assert.That(logEvent.Args[1], Is.EqualTo("test"), "Second element order mismatch");
		Assert.That(logEvent.Args[2], Is.EqualTo(true), "Third element order mismatch");
		Assert.That(logEvent.Args[3], Is.EqualTo(3.14), "Fourth element order mismatch");
	}

	[Test]
	[Category("ArgsArrayCorrectness")]
	public void ArgsArrayPreservesValueTypesCorrectly()
	{
		// Arrange
		var template = new LogEventTemplate<(int IntVal, long LongVal, byte ByteVal, short ShortVal, decimal DecimalVal)>
		{
			EventId = 55555,
			LogLevel = LogLevel.Information,
			LogMessage = "{IntVal}, {LongVal}, {ByteVal}, {ShortVal}, {DecimalVal}",
		};

		// Act
		var args = (IntVal: 42, LongVal: 9876543210L, ByteVal: (byte)255, ShortVal: (short)-32768, DecimalVal: 123.456m);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.Args, Has.Length.EqualTo(5));
		Assert.That(logEvent.Args[0], Is.EqualTo(42));
		Assert.That(logEvent.Args[1], Is.EqualTo(9876543210L));
		Assert.That(logEvent.Args[2], Is.EqualTo((byte)255));
		Assert.That(logEvent.Args[3], Is.EqualTo((short)-32768));
		Assert.That(logEvent.Args[4], Is.EqualTo(123.456m));
	}

	[Test]
	[Category("ArgsArrayCorrectness")]
	public void ArgsArrayPreservesReferenceTypesCorrectly()
	{
		// Arrange
		var template = new LogEventTemplate<(string StringVal, object ObjectVal, DateTime DateVal)>
		{
			EventId = 66666,
			LogLevel = LogLevel.Information,
			LogMessage = "{StringVal}, {ObjectVal}, {DateVal}",
		};

		// Act
		var expectedDate = new DateTime(2024, 1, 15, 10, 30, 0);
		var expectedObject = new { Id = 42, Name = "Test" };
		var args = (StringVal: "Hello World", ObjectVal: (object)expectedObject, DateVal: expectedDate);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.Args, Has.Length.EqualTo(3));
		Assert.That(logEvent.Args[0], Is.EqualTo("Hello World"));
		Assert.That(logEvent.Args[1], Is.SameAs(expectedObject), "Object reference should be preserved");
		Assert.That(logEvent.Args[2], Is.EqualTo(expectedDate));
	}

	[Test]
	[Category("ArgsArrayCorrectness")]
	public void ArgsArrayHandlesSingleParameterWithUnitCorrectly()
	{
		// Arrange
		var template = new LogEventTemplate<(string Message, Unit)>
		{
			EventId = 77777,
			LogLevel = LogLevel.Information,
			LogMessage = "{Message}",
		};

		// Act
		var args = (Message: "Single parameter test", Unit.Value);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.Args, Has.Length.EqualTo(1), "Unit should be filtered out");
		Assert.That(logEvent.Args[0], Is.EqualTo("Single parameter test"));
	}

	[Test]
	[Category("ArgsArrayCorrectness")]
	public void ArgsArrayHandlesMultipleUnitsCorrectly()
	{
		// Arrange: Using three Unit parameters should yield those three. This tests that Unit is only removed when it's the second and last element.
		var template = new LogEventTemplate<(Unit, Unit, Unit)>
		{
			EventId = 77777,
			LogLevel = LogLevel.Information,
			LogMessage = "{Message}",
		};

		// Act
		var args = (Unit.Value, Unit.Value, Unit.Value);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.Args, Has.Length.EqualTo(3), "Nothing should be filtered out");
		Assert.That(logEvent.Args[0], Is.EqualTo(logEvent.Args[1]).And.EqualTo(logEvent.Args[2]).And.EqualTo(Unit.Value));
	}

	[Test]
	[Category("ArgsArrayCorrectness")]
	public void ArgsArrayHandlesComplexObjectsCorrectly()
	{
		// Arrange
		var template = new LogEventTemplate<(List<int> Numbers, Dictionary<string, object> Metadata)>
		{
			EventId = 88888,
			LogLevel = LogLevel.Information,
			LogMessage = "{Numbers}, {Metadata}",
		};

		// Act
		var numbers = new List<int> { 1, 2, 3, 4, 5 };
		var metadata = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 };
		var args = (Numbers: numbers, Metadata: metadata);
		var logEvent = template.Build(args);

		// Assert
		Assert.That(logEvent.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent.Args[0], Is.SameAs(numbers), "List reference should be preserved");
		Assert.That(logEvent.Args[1], Is.SameAs(metadata), "Dictionary reference should be preserved");
		Assert.That((logEvent.Args[0] as List<int>)?.Count, Is.EqualTo(5));
		Assert.That((logEvent.Args[1] as Dictionary<string, object>)?.Count, Is.EqualTo(2));
	}

	[Test]
	[Category("ArgsArrayCorrectness")]
	public void MultipleCallsToSameTemplateProduceIndependentArgsArrays()
	{
		// Arrange
		var template = new LogEventTemplate<(int Id, string Name)>
		{
			EventId = 99999,
			LogLevel = LogLevel.Information,
			LogMessage = "{Id}, {Name}",
		};

		// Act
		var args1 = (Id: 1, Name: "First");
		var args2 = (Id: 2, Name: "Second");
		var logEvent1 = template.Build(args1);
		var logEvent2 = template.Build(args2);

		// Assert - Verify first log event
		Assert.That(logEvent1.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent1.Args[0], Is.EqualTo(1));
		Assert.That(logEvent1.Args[1], Is.EqualTo("First"));

		// Assert - Verify second log event
		Assert.That(logEvent2.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent2.Args[0], Is.EqualTo(2));
		Assert.That(logEvent2.Args[1], Is.EqualTo("Second"));

		// Assert - Verify independence
		Assert.That(logEvent1.Args, Is.Not.SameAs(logEvent2.Args), "Args arrays should be independent");
	}

	[Test]
	public void SuperflousGenericParameterIsDetected()
	{
		SuperflousGenericParameterIsDetected((123, "Something"), false);
		SuperflousGenericParameterIsDetected((123, Unit.Value), true);
	}

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
	private static void SuperflousGenericParameterIsDetected(System.Runtime.CompilerServices.ITuple tuple, bool target)

#else
	private static void SuperflousGenericParameterIsDetected<T>(T tuple, bool target) where T : struct
#endif
	{
		// Act
		var actual = LogTemplateHelper.ShouldFilterSecondElement(tuple.GetType());
		
		// Assert
		Assert.That(actual, Is.EqualTo(target));
	}

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER
	[Test]
	public void TupleTypeVerificationSucceeds()
	{
		Assert.DoesNotThrow
		(
			() =>
			{
				// This should succeed as (int, string) is a tuple.
				_ = new LogEventTemplate<(int UserId, string UserName)>
				{
					EventId = 69544843,
					LogLevel = LogLevel.Information,
					LogMessage = "This is an example log message. {Placeholder}, {Placeholder2}",
				};
			}
		);
	}

	[Test]
	public void TupleTypeVerificationThrows()
	{
		Assert.Catch<InvalidOperationException>
		(
			() =>
			{
				// An int is a struct but not a tuple, so this should throw.
				_ = new LogEventTemplate<int>
				{
					EventId = 69544843,
					LogLevel = LogLevel.Information,
					LogMessage = "This is an example log message. {Placeholder}, {Placeholder2}",
				};
			}
		);
	}
#endif

	[Test]
	public void LogEventTemplateCanBeCreated()
	{
		Assert.DoesNotThrow
		(
			() =>
			{
				var template = new LogEventTemplate
				{
					EventId = 69544843,
					LogLevel = LogLevel.Information,
					LogMessage = "This is an example log message",
				};

				var logEvent = template.Build();
				Assert.That(logEvent.EventId.Id, Is.EqualTo(69544843));
				Assert.That(logEvent.LogLevel, Is.EqualTo(LogLevel.Information));
				Assert.That(logEvent.LogMessage, Is.EqualTo("This is an example log message"));
				Assert.That(logEvent.PayLoad, Is.Null);
				Assert.That(logEvent.Exception, Is.Null);
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new LogEventTemplate<(int UserId, Unit)>
				{
					EventId = 242615004,
					LogLevel = LogLevel.Information,
					LogMessage = "This is an example log message. {Placeholder}",
				};
			}
		);

		Assert.DoesNotThrow
		(
			() =>
			{
				_ = new LogEventTemplate<(int UserId, string UserName)>
				{
					EventId = 1431823846,
					LogLevel = LogLevel.Information,
					LogMessage = "This is an example log message. {Placeholder}, {Placeholder2}",
				};
			}
		);
	}

	#region Log Event Building

	[Test]
	[Category("LogEventBuilding")]
	public void LogEventCanBeBuild()
	{
		// Arrange
		var id = _fixture.Create<int>();
		var level = LogLevel.Information;
		var message = "This is an example log message";
		var template = new LogEventTemplate
		{
			EventId = id,
			LogLevel = level,
			LogMessage = message,
		};

		// Act + Assert: Direct Build.
		var logEvent = template.Build();
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Is.Empty);
		Assert.That(logEvent.PayLoad, Is.Null);
		Assert.That(logEvent.Exception, Is.Null);

		// Act + Assert: Different log level
		var logLevel = LogLevel.Debug;
		logEvent = template.Build(logLevel);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Is.Empty);
		Assert.That(logEvent.PayLoad, Is.Null);
		Assert.That(logEvent.Exception, Is.Null);

		// Act + Assert: Payload
		var payload = LogScope.CreateIndependent(("Property", "Value"));
		logEvent = template.Build(payload);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Is.Empty);
		Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
		Assert.That(logEvent.Exception, Is.Null);

		// Act + Assert: Exception
		var exception = new Exception();
		logEvent = template.Build(exception);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Is.Empty);
		Assert.That(logEvent.PayLoad, Is.Null);
		Assert.That(logEvent.Exception, Is.EqualTo(exception));

		// Act + Assert: Everything		
		logEvent = template.Build(logLevel, exception, payload);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Is.Empty);
		Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
		Assert.That(logEvent.Exception, Is.EqualTo(exception));
	}

	[Test]
	[Category("LogEventBuilding")]
	public void LogEventWithSingleParameterCanBeBuild()
	{
		// Arrange
		var id = _fixture.Create<int>();
		var level = LogLevel.Information;
		var message = "User id: {UserId}";
		var template = new LogEventTemplate<(int UserId, Unit)>
		{
			EventId = id,
			LogLevel = level,
			LogMessage = message,
		};

		// Act + Assert: Direct Build.
		var args = (UserId: 42, Unit.Value);
		var logEvent = template.Build(args);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(1));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Exception, Is.Null);
		Assert.That(logEvent.PayLoad, Is.Null);

		// Act + Assert: Different log level
		var logLevel = LogLevel.Debug;
		logEvent = template.Build(args, logLevel);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(1));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.PayLoad, Is.Null);
		Assert.That(logEvent.Exception, Is.Null);

		// Act + Assert: Payload
		var payload = LogScope.CreateIndependent(("Property", "Value"));
		logEvent = template.Build(args, payload);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(1));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Exception, Is.Null);
		Assert.That(logEvent.PayLoad, Is.EqualTo(payload));

		// Act + Assert: Exception
		var exception = new Exception();
		logEvent = template.Build(args, exception);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(1));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Exception, Is.EqualTo(exception));
		Assert.That(logEvent.PayLoad, Is.Null);

		// Act + Assert: Everything		
		logEvent = template.Build(args, logLevel, exception, payload);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(1));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Exception, Is.EqualTo(exception));
		Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
	}

	[Test]
	[Category("LogEventBuilding")]
	public void LogEventWithMultipleParametersCanBeBuild()
	{
		// Arrange
		var id = _fixture.Create<int>();
		var level = LogLevel.Information;
		var message = "User id: {UserId}, User name: {UserName";
		var template = new LogEventTemplate<(int UserId, string UserName)>
		{
			EventId = id,
			LogLevel = level,
			LogMessage = message,
		};

		// Act + Assert: Direct Build.
		var args = (UserId: 42, UserName: "John");
		var logEvent = template.Build(args);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Args[1], Is.EqualTo(args.UserName));
		Assert.That(logEvent.Exception, Is.Null);
		Assert.That(logEvent.PayLoad, Is.Null);

		// Act + Assert: Different log level
		var logLevel = LogLevel.Debug;
		logEvent = template.Build(args, logLevel);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Args[1], Is.EqualTo(args.UserName));
		Assert.That(logEvent.PayLoad, Is.Null);
		Assert.That(logEvent.Exception, Is.Null);

		// Act + Assert: Payload
		var payload = LogScope.CreateIndependent(("Property", "Value"));
		logEvent = template.Build(args, payload);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Args[1], Is.EqualTo(args.UserName));
		Assert.That(logEvent.Exception, Is.Null);
		Assert.That(logEvent.PayLoad, Is.EqualTo(payload));

		// Act + Assert: Exception
		var exception = new Exception();
		logEvent = template.Build(args, exception);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(level));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Args[1], Is.EqualTo(args.UserName));
		Assert.That(logEvent.Exception, Is.EqualTo(exception));
		Assert.That(logEvent.PayLoad, Is.Null);

		// Act + Assert: Everything		
		logEvent = template.Build(args, logLevel, exception, payload);
		Assert.That(logEvent.EventId.Id, Is.EqualTo(id));
		Assert.That(logEvent.LogLevel, Is.EqualTo(logLevel));
		Assert.That(logEvent.LogMessage, Is.EqualTo(message));
		Assert.That(logEvent.Args, Has.Length.EqualTo(2));
		Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
		Assert.That(logEvent.Args[1], Is.EqualTo(args.UserName));
		Assert.That(logEvent.Exception, Is.EqualTo(exception));
		Assert.That(logEvent.PayLoad, Is.EqualTo(payload));
	}

	[Test]
	[Category("ThreadSafety")]
	public void LogEventTemplateCanBeUsedConcurrentlyFromMultipleThreads()
	{
		// Arrange
		var template = new LogEventTemplate
		{
			EventId = 12345,
			LogLevel = LogLevel.Information,
			LogMessage = "Concurrent test message",
		};

		var threadCount = 10;
		var iterationsPerThread = 100;
		var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
		var completedCount = 0;

		// Act
		var threads = Enumerable.Range(0, threadCount)
			.Select
			(
				threadIndex => new Thread
				(
					() =>
					{
						try
						{
							for (var i = 0; i < iterationsPerThread; i++)
							{
								var logEvent = template.Build();
								Assert.That(logEvent.EventId.Id, Is.EqualTo(12345));
								Assert.That(logEvent.LogLevel, Is.EqualTo(LogLevel.Information));
								Assert.That(logEvent.LogMessage, Is.EqualTo("Concurrent test message"));

								var logEventWithLevel = template.Build(LogLevel.Debug);
								Assert.That(logEventWithLevel.LogLevel, Is.EqualTo(LogLevel.Debug));

								var exception = new Exception($"Thread {threadIndex} iteration {i}");
								var logEventWithException = template.Build(exception);
								Assert.That(logEventWithException.Exception, Is.EqualTo(exception));
							}
							Interlocked.Increment(ref completedCount);
						}
						catch (Exception ex)
						{
							exceptions.Add(ex);
						}
					}
				)
			)
			.ToList()
			;

		threads.ForEach(thread => thread.Start());
		threads.ForEach(thread => thread.Join());

		// Assert
		Assert.That(exceptions, Is.Empty, $"Exceptions occurred: {string.Join(", ", exceptions.Select(exception => exception.Message))}");
		Assert.That(completedCount, Is.EqualTo(threadCount), "Not all threads completed successfully");
	}

	[Test]
	[Category("ThreadSafety")]
	public void LogEventTemplateWithSingleParameterCanBeUsedConcurrentlyFromMultipleThreads()
	{
		// Arrange
		var template = new LogEventTemplate<(int RequestId, Unit)>
		{
			EventId = 99999,
			LogLevel = LogLevel.Debug,
			LogMessage = "Processing request {RequestId}",
		};

		var threadCount = 10;
		var iterationsPerThread = 100;
		var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
		var completedCount = 0;

		// Act
		var threads = Enumerable.Range(0, threadCount)
			.Select
			(
				threadIndex => new Thread
				(
					() =>
					{
						try
						{
							for (var i = 0; i < iterationsPerThread; i++)
							{
								var args = (RequestId: threadIndex * 1000 + i, Unit.Value);

								var logEvent = template.Build(args);
								Assert.That(logEvent.EventId.Id, Is.EqualTo(99999));
								Assert.That(logEvent.Args, Has.Length.EqualTo(1));
								Assert.That(logEvent.Args[0], Is.EqualTo(args.RequestId));

								var payload = LogScope.CreateIndependent(("ThreadId", threadIndex));
								var logEventWithPayload = template.Build(args, payload);
								Assert.That(logEventWithPayload.PayLoad, Is.EqualTo(payload));
								Assert.That(logEventWithPayload.Args[0], Is.EqualTo(args.RequestId));
							}
							Interlocked.Increment(ref completedCount);
						}
						catch (Exception ex)
						{
							exceptions.Add(ex);
						}
					}
				)
			)
			.ToList()
			;

		threads.ForEach(thread => thread.Start());
		threads.ForEach(thread => thread.Join());

		// Assert
		Assert.That(exceptions, Is.Empty, $"Exceptions occurred: {string.Join(", ", exceptions.Select(exception => exception.Message))}");
		Assert.That(completedCount, Is.EqualTo(threadCount), "Not all threads completed successfully");
	}

	[Test]
	[Category("ThreadSafety")]
	public void LogEventTemplateWithMultipleParametersCanBeUsedConcurrentlyFromMultipleThreads()
	{
		// Arrange
		var template = new LogEventTemplate<(int UserId, string UserName)>
		{
			EventId = 67890,
			LogLevel = LogLevel.Warning,
			LogMessage = "User {UserId} with name {UserName}",
		};

		var threadCount = 10;
		var iterationsPerThread = 100;
		var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
		var completedCount = 0;

		// Act
		var threads = Enumerable.Range(0, threadCount)
			.Select
			(
				threadIndex => new Thread
				(
					() =>
					{
						try
						{
							for (var i = 0; i < iterationsPerThread; i++)
							{
								var args = (UserId: threadIndex * 1000 + i, UserName: $"User_{threadIndex}_{i}");
						
								var logEvent = template.Build(args);
								Assert.That(logEvent.EventId.Id, Is.EqualTo(67890));
								Assert.That(logEvent.LogLevel, Is.EqualTo(LogLevel.Warning));
								Assert.That(logEvent.Args, Has.Length.EqualTo(2));
								Assert.That(logEvent.Args[0], Is.EqualTo(args.UserId));
								Assert.That(logEvent.Args[1], Is.EqualTo(args.UserName));

								var logEventWithLevel = template.Build(args, LogLevel.Error);
								Assert.That(logEventWithLevel.LogLevel, Is.EqualTo(LogLevel.Error));
								Assert.That(logEventWithLevel.Args[0], Is.EqualTo(args.UserId));

								var exception = new Exception($"Thread {threadIndex} iteration {i}");
								var logEventWithException = template.Build(args, exception);
								Assert.That(logEventWithException.Exception, Is.EqualTo(exception));
								Assert.That(logEventWithException.Args[1], Is.EqualTo(args.UserName));
							}
							Interlocked.Increment(ref completedCount);
						}
						catch (Exception ex)
						{
							exceptions.Add(ex);
						}
					}
				)
			)
			.ToList()
			;

		threads.ForEach(thread => thread.Start());
		threads.ForEach(thread => thread.Join());

		// Assert
		Assert.That(exceptions, Is.Empty, $"Exceptions occurred: {string.Join(", ", exceptions.Select(exception => exception.Message))}");
		Assert.That(completedCount, Is.EqualTo(threadCount), "Not all threads completed successfully");
	}

	#endregion

	#endregion
}