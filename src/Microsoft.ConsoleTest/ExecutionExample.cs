using System;
using System.Diagnostics;
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

		private readonly Logger _logger;

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

		#region (De)Constructors

		public ExecutionExample(Microsoft.Extensions.Logging.ILogger logger)
		{
			// Save parameters.

			// Initialize fields.
			_logger = Logger.FromILogger(logger);
		}

		#endregion

		#region Methods

		internal async Task StartWork()
		{
			var iterations = 10;
			for (int iteration = 1; iteration <= iterations; iteration++)
			{
				await this.Execute(iteration);
			}
		}

		private async Task Execute(int iteration)
		{
			using (_logger.BeginScope(() => iteration))
			{
				Console.WriteLine(_logger.LogStart(iteration));

				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var random = new Random();
				await Task.Delay(random.Next(500, 1000));
				stopwatch.Stop();
				
				var result = Enum.Parse<ExecutionResult>(random.Next(0, 2).ToString());
				Console.WriteLine(_logger.LogFinished(iteration, result, stopwatch.Elapsed));
			}
		}

		#endregion

		#region Logging

		private class Logger : EventIdResourceLogger
		{
			#region (De)Constructors

			private Logger(Microsoft.Extensions.Logging.ILogger logger) : base(logger, new[] { l10n.ResourceManager }) { }

			public static Logger FromILogger(Microsoft.Extensions.Logging.ILogger logger) => new Logger(logger);

			#endregion

			#region Log methods

			internal string LogStart(int iteration)
			{
				//! The 'iteration' argument can be omitted from the logArgs, as this method is always called from a log-scope that encapsulates this value.
				return base.LogEventFromResource(1523340757, LogLevel.Debug, nameof(l10n.Start), messageArgs: new object[] { iteration });
			}

			internal void LogProgress(LogLevel logLevel)
			{
				using var process = Process.GetCurrentProcess();
				base.LogEventFromResource(813784984, LogLevel.Debug, nameof(l10n.Progress), new object[] { process.WorkingSet64 });
			}

			internal string LogFinished(int iteration, ExecutionResult result, TimeSpan duration)
			{
				var logLevel = result == ExecutionResult.Valid ? LogLevel.Debug : LogLevel.Warning;

				//! The 'iteration' argument cannot be omitted from the logArgs even if this method is always called from a log-scope that encapsulates this value, because other values will be provided too.
				return base.LogEventFromResource(813784984, logLevel, nameof(l10n.Finished), new object[] { iteration, duration, result }, new object[] { iteration, duration.TotalMilliseconds, result });
			}

			#endregion
		}

		#endregion
	}
}