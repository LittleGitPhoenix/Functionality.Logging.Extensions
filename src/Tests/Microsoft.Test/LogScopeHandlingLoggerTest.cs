using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

#if NETCOREAPP3_0_OR_GREATER // → The InMemory sink used in the test is only available for .NET Standard 2.1.
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.InMemory;
using SerilogLogEvent = Serilog.Events.LogEvent;
#endif

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Microsoft.Test;

public class LogScopeHandlingLoggerTest
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

	/// <summary>
	/// The amount of parallel calls to run within the test to maximize the chance of scope cross-contamination if isolation is broken. The actual number is not important as long as it is >1 and high enough to create a realistic level of concurrency, but not too high to cause thread pool starvation or excessively long test runtimes.
	/// </summary>
	private const int ParallelCalls = 10;

	class Dialog : IDisposable
	{
		private readonly ILogger _logger;

		private readonly IDisposable _logScope;

		public Dialog(ILogger logger, int userId)
		{
			_logger = logger;

			// Every(!) log event must carry the user id as a log scope.
			// Therefore, the log scope must be created as independent (not execution context aware) as otherwise only the task that created the dialog would have the user id in its log scope, but not other tasks that are running in parallel and also emit log events related to the same dialog.
#if NETCOREAPP3_0_OR_GREATER
			_logScope = logger.Enrich(LogScope.CreateIndependent(userId));
			logger.Log(InitialLogEventTemplate.Build((userId, Unit.Value)));
#else
			_logScope = logger.Enrich(LogScope.CreateIndependent((nameof(userId), userId)));
			logger.Log(InitialLogEventTemplate.Build((userId, Unit.Value)));
#endif
		}

		internal void Handle(int callbackId, string result)
		{
			// The log scope containing the callback id must be execution context aware, so that logs from multiple parallel callbacks can differentiate themselves by their callback id.
#if NETCOREAPP3_0_OR_GREATER
			using (_logger.Enrich(LogScope.CreateAware(callbackId)))
#else
			using (_logger.Enrich(LogScope.CreateAware((nameof(callbackId), callbackId))))
#endif
			{
				// The additionally added date payload must also be execution context ware. Each callback should log its own date, not the date of another callback.
				var date = DateTime.UtcNow;
				var logEvent =
#if NETCOREAPP3_0_OR_GREATER
					CallbackLogEventTemplate.Build((result, Unit.Value), payload: Payload.Create(date));
#else
					CallbackLogEventTemplate.Build((result, Unit.Value), payload: Payload.Create((nameof(date), date)));
#endif
				_logger.Log(logEvent);
			}
		}

		static LogEventTemplate<(int, Unit)> InitialLogEventTemplate { get; } = new()
		{
			EventId = 1158155927,
			LogLevel = LogLevel.Information,
			LogMessage = "Dialog created for user {UserId}.",
		};

		static LogEventTemplate<(string, Unit)> CallbackLogEventTemplate { get; } = new()
		{
			EventId = 1933397192,
			LogLevel = LogLevel.Information,
			LogMessage = "Callback result was: {CallBackResult}.",
		};

		#region IDisposable

		/// <inheritdoc />
		public void Dispose() => _logScope.Dispose();

		#endregion
	}

	#endregion

	#region Tests

#if NETCOREAPP3_0_OR_GREATER
	[Test]
	public void SerilogUsesExecutionContextAwareScope()
	{
		// Arrange: Create a Serilog logger that uses the assertable InMemory sink.
		var serilogLogger = new LoggerConfiguration()
			.Enrich.FromLogContext()
			.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.ffff} {Level:u3}]{Message:lj} {EventId}{Properties:j}{NewLine}{Exception}") // → For debugging purposes, to see the log events in the test output. Can be removed if too noisy.
			.WriteTo.InMemory()
			.CreateLogger()
			;

		// Arrange: Create a Microsoft.Extensions.Logging.ILogger that uses the Serilog logger as underlying logger.
		var frameworkLogger = new SerilogLoggerProvider(serilogLogger).CreateLogger(nameof(this.SerilogUsesExecutionContextAwareScope));

		// Arrange: Create a LogScopeHandlingLogger that uses the Microsoft.Extensions.Logging.ILogger as underlying logger.
		var logger = new LogScopeHandlingLogger(frameworkLogger);

		// Act: Snapshot the current event count so this test is isolated from any pre-existing events in the static sink.
		var initialEventCount = InMemorySink.Instance.LogEvents.Count();
		this.CreateLogEvents(logger);
		var logEvents = InMemorySink.Instance.LogEvents.Skip(initialEventCount).ToList();

		// Assert: 1 initial event + callback events.
		Assert.That(logEvents, Has.Count.EqualTo(1 + ParallelCalls));

		// Assert: Every event carries the userId from the Independent scope, regardless of which thread/task emitted it.
		// Note: Serilog.Extensions.Logging capitalizes the first letter of scope property keys (e.g. "userId" → "UserId").
		Assert.That(logEvents, Has.All.Matches<SerilogLogEvent>(e => e.Properties.TryGetValue("UserId", out var userId) && userId is ScalarValue { Value: 1 }));

		// Assert: Exactly 3 callback events.
		var callbackEvents = logEvents.Where(e => e.MessageTemplate.Text.Contains("{CallBackResult}")).ToList();
		Assert.That(callbackEvents, Has.Count.EqualTo(ParallelCalls));

		// Assert: Each callback event carries its own callbackId from the ExecutionContextAware scope.
		// Note: Serilog.Extensions.Logging capitalizes the first letter of scope property keys (e.g. "callbackId" → "CallbackId").
		Assert.That(callbackEvents, Has.All.Matches<SerilogLogEvent>(e => e.Properties.ContainsKey("CallbackId")));

		// Assert: The callbackId on each event matches the result logged within the same callback, proving that ExecutionContextAware scopes are isolated between parallel callbacks.
		using (Assert.EnterMultipleScope())
		{
			foreach (var logEvent in callbackEvents)
			{
				var callbackId = ((ScalarValue) logEvent.Properties["CallbackId"]).Value;
				var result = ((ScalarValue) logEvent.Properties["CallBackResult"]).Value?.ToString();
				Assert.That(result, Is.EqualTo($"Result-{callbackId}"), $"ExecutionContextAware scope must be isolated per callback: callbackId {callbackId} must only appear on its own log event, not bleed into another callback's event.");
			}
		}
	}
#endif

	private void CreateLogEvents(ILogger logger)
	{
		var dialog = new Dialog(logger, userId: 1);

		// Run several callbacks in parallel to verify that the log scopes are correctly created as execution context aware or independent.
		// The delay ensures all tasks are in-flight simultaneously, maximizing the chance of scope cross-contamination if isolation is broken.
		var tasks = Enumerable.Range(1, ParallelCalls)
			.Select
			(
				callbackId => Task.Run
				(
					async () =>
					{
						await Task.Delay(10);
						// ReSharper disable once AccessToDisposedClosure → The dialog is disposed after all tasks have completed, so it is guaranteed to still be alive when the callbacks are invoked.
						dialog.Handle(callbackId, $"Result-{callbackId}");
					}
				)
			)
			.ToArray()
			;

		Task.WaitAll(tasks);
		dialog.Dispose();
	}

#endregion
}