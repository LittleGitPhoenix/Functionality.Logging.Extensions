#if NETCOREAPP3_0_OR_GREATER

using Microsoft.Extensions.Logging;
using Microsoft.Test.Localization;
using Phoenix.Functionality.Logging.Base;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

/// <summary>
/// Those are example classes demonstrating logging patterns and usage with Microsoft.Extensions.Logging and Phoenix.Functionality.Logging.
/// They are written here and are used in the <b>README.md</b> file to illustrate logging usage patterns.
/// </summary>
public class ReadmeClasses
{
	#region About Logging

	class Dialog : IDisposable
	{
		private readonly ILogger _logger;

		private readonly IDisposable _logScope;

		public Dialog(ILogger logger, int userId)
		{
			_logger = logger;

			// In order for every log event to carry the user id as a log scope, it must be created as independent (not execution context aware) as otherwise only the task that created the dialog would have the user id in its log scope, but not other tasks that are running in parallel and also emit log events related to the same dialog.
			_logScope = logger.Enrich(LogScope.CreateIndependent(userId));
			logger.LogInformation("Dialog created for user {UserId}.", userId);
		}

		internal void Handle(int callbackId, string result)
		{
			// The log scope containing the callback id must be execution context aware, so that logs from multiple parallel callbacks can differentiate themselves by their callback id.
			using (_logger.Enrich(LogScope.CreateAware(callbackId)))
			{
				_logger.LogInformation("Callback result was: {CallBackResult}.", result);
			}
		}

		/// <inheritdoc />
		public void Dispose() => _logScope.Dispose();
	}

	class Orchestrator
	{
		internal const string GroupIdentifier = "DialogSettingsGroup";

		void Setup()
		{
			Func<ILogger> loggerFactory = () => null!; // Assume that this creates new logger instances.

			// The below factories use 'AddToGroup' to add the logger instances they create into a group identified by a custom identifier.
			var settingsManagerFactory = () => new SettingsManager(loggerFactory.Invoke().AddToGroup(GroupIdentifier));
			var dialogFactory = (int userId) => new SettingsDialog(loggerFactory.Invoke().AddToGroup(GroupIdentifier), userId, settingsManagerFactory.Invoke());
		}
	}

	class SettingsDialog(ILogger logger, int userId, SettingsManager settingsManager)
	{
		void UserPressedSaveButton()
		{
			// Before the scope is created, the logger is used to access the group by its identifier, so that the scope enriches all loggers in the group.
			// The scope must be created as execution context aware, so that other parallel calls to 'SettingsManager.Save' do not interfere with each other's log scopes.
			using var userIdScope = logger.AsGroup(Orchestrator.GroupIdentifier).Enrich(LogScope.CreateAware(userId));
			settingsManager.Save();
		}
	}

	class SettingsManager(ILogger logger)
	{
		// Even though the 'SettingsManager' is unaware of the 'Dialog' and its user id scope, the log output will still contain the user id as attached property.
		internal void Save() => logger.LogInformation("Settings were saved.");
	}

	#endregion

	#region LogEvent templates

	class MyClass(Microsoft.Extensions.Logging.ILogger logger)
	{
		public void DoSomething(int userId)
		{
			var somethingEvent = Log.SomethingHappened.Build((userId, Unit.Value));
			logger.Log(somethingEvent);

			var resourceEvent = Log.IdenticalResourcePlaceholders.Build((userId, "John"));
			var outputMessage = logger.Log(resourceEvent);
			Console.WriteLine(outputMessage);

			var importantData = "Some important pice of information.";
			var logEvent = Log.NoPlaceholder.Build(payload: Payload.Create(importantData));

			var correlationId = Guid.NewGuid();
			var startTime = DateTime.UtcNow;
			var correlationIdScope = LogScope.CreateAware(correlationId);
			var startTimeScope = LogScope.CreateAware(startTime);
			IDisposable appliedLogScope = logger.Enrich(correlationIdScope).Enrich(startTimeScope);

			//var combinedScope = LogScope.CreateAware(correlationId, startTime);
			//IDisposable appliedLogScope = logger.Enrich(combinedScope);

			var applicationName = LogApplicationInformation.Default.Name;
			var applicationVersion = LogApplicationInformation.Default.AssemblyVersion;
			logger.EnrichPermanently(LogScope.CreateIndependent(applicationName, applicationVersion));

			using (appliedLogScope)
			{
				// Each of the emitted logs will carry the correlation id as attached property.
				logger.LogInformation("...");
				logger.LogInformation("...");
			}
			// This log event will not have the correlation id as attached property anymore.
			logger.LogInformation("...");

			using 
				(
					logger
						.Enrich(correlationIdScope)
						.Enrich(startTimeScope)
						.Log(Log.NoPlaceholder.Build())
						.Use()
				)
			{
				logger.LogInformation("...");
			}
		}

		#region Logging

		// This is the static Log class.
		static class Log
		{
			// This is a static log event template that accepts a user id as placeholder for its message.
			internal static LogEventTemplate<(int userId, Unit)> SomethingHappened { get; } = new()
			{
				EventId = 1668049180,
				LogLevel = LogLevel.Information,
				LogMessage = "Something happened to user {UserId}.",
			};

			internal static LogEventTemplate NoPlaceholder { get; } = new()
			{
				EventId = 1698180849,
				LogLevel = LogLevel.Information,
				LogMessage = "Just a message.",
			};

			internal static LogEventTemplate<(Guid identifier, Unit)> SinglePlaceholder { get; } = new()
			{
				EventId = 418730146,
				LogLevel = LogLevel.Information,
				LogMessage = "Identifier is: {Identifier}.",
			};

			internal static LogEventTemplate<(int userId, string userName)> MultiplePlaceholders { get; } = new()
			{
				EventId = 1949425686,
				LogLevel = LogLevel.Information,
				LogMessage = "The name of user {UserId} is {UserName}.",
			};

			internal static LogResourceEventTemplate<(int userId, string userName)> IdenticalResourcePlaceholders { get; } = new()
			{
				EventId = 1785678218,
				LogLevel = LogLevel.Information,
				ResourceManager = l10n.ResourceManager,
				ResourceName = nameof(l10n.MessageWithMatchingPlaceholders),
			};

			internal static LogResourceEventTemplate<(int userId, string userName), (string useName, Unit)> DifferentResourcePlaceholders { get; } = new()
			{
				EventId = 1805702069,
				LogLevel = LogLevel.Information,
				ResourceManager = l10n.ResourceManager,
				ResourceName = nameof(l10n.MessageWithDifferentPlaceholders),
			};
		}

		#endregion
	}

	#endregion
}

#endif