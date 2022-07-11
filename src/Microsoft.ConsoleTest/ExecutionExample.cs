using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.ConsoleTest.Localization;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.ConsoleTest
{
	class ExecutionExample
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly ILogger _logger;

		#endregion

		#region Properties
		#endregion

		#region Enumerations

		enum ExecutionResult
		{
			Unknown,
			Valid,
			Invalid,
		}

		#endregion

		#region (De)Constructos

		public ExecutionExample(Microsoft.Extensions.Logging.ILogger logger)
		{
			// Save parameters.

			// Initialize fields.
			_logger = logger;
			logger.CreateScope(Log.NameScope(nameof(ExecutionExample)));
		}

		#endregion

		#region Methods

		internal async Task StartWork()
		{
			const int iterations = 3;
			for (var iteration = 1; iteration <= iterations; iteration++)
			{
				await this.Execute(iteration);
			}
		}

		private async Task Execute(int iteration)
		{
			using (_logger.CreateScope(Log.IterationScope(iteration)))
			{
				Console.WriteLine(_logger.Log(Log.StartEvent(iteration)));

				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var random = new Random();
				await Task.Delay(random.Next(500, 1000));
				stopwatch.Stop();
				
				var result = Enum.Parse<ExecutionResult>(random.Next(0, 2).ToString());
				Console.WriteLine(_logger.Log(Log.FinishedEvent(iteration, result, stopwatch.Elapsed)));
			}
		}

		#endregion

		#region Logging

		static class Log
		{
			internal static LogScope NameScope(string scope)
			{
				return new LogScope(scope);
			}
			internal static LogScope IterationScope(int iteration)
			{
				return new LogScope(iteration);
			}

			internal static LogResourceEvent StartEvent(int iteration)
			{
				//! The 'iteration' argument can be omitted from the logArgs, as this method is always called from a log-scope that encapsulates this value.
				return new LogResourceEvent(1523340757, LogLevel.Debug, l10n.ResourceManager, nameof(l10n.Start), messageArgs: new object[] { iteration });
			}
			
			internal static LogResourceEvent FinishedEvent(int iteration, ExecutionResult result, TimeSpan duration)
			{
				var logLevel = result == ExecutionResult.Valid ? LogLevel.Debug : LogLevel.Warning;

				//! The 'iteration' argument cannot be omitted from the logArgs even if this method is always called from a log-scope that encapsulates this value, because other values will be provided too.
				return new LogResourceEvent(813784984, logLevel, l10n.ResourceManager, nameof(l10n.Finished), new object[] { iteration, duration, result }, new object[] { iteration, duration.TotalMilliseconds, result });
			}
		}

		#endregion
	}
}