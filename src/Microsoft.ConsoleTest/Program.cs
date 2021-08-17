using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.ConsoleTest
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Program.ChangeCulture();

			var builder = new ContainerBuilder();
			builder.RegisterModule<IocModule>();
			var container = builder.Build();

			//! Log events are only written to the debug window (and not to the console).
			var msLogger = container.Resolve<ILogger>();
			var logger = Logger.FromILogger(msLogger);
			var msLogger2 = container.Resolve<ILogger>();
			var logger2 = Logger.FromILogger(msLogger2);

			if (Object.ReferenceEquals(msLogger, msLogger2))
			{

			}
			
			try
			{
				logger.LogApplicationStart();
				var executor = container.Resolve<ExecutionExample>();
				await executor.StartWork();
				logger.IterationsFinished();
			}
			finally
			{
				Console.WriteLine();
				Console.WriteLine("Press any key to quit.");
				Console.ReadKey();
				logger.LogApplicationEnd();
			}
		}

		/// <summary>
		/// Changes the culture used for the application.
		/// </summary>
		private static void ChangeCulture()
		{
			// Override the culture settings.
			var culture = new CultureInfo("de-DE");
			CultureInfo.CurrentUICulture = culture;
			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
		}

		#region Logging

		private class Logger : EventIdLogger
		{
			#region (De)Constructors

			private Logger(Microsoft.Extensions.Logging.ILogger logger) : base(logger) { }

			public static Logger FromILogger(Microsoft.Extensions.Logging.ILogger logger) => new Logger(logger);

			#endregion

			#region Log methods

			internal void LogApplicationStart()
			{
				var entryAssembly = Assembly.GetEntryAssembly();
				var fileVersion = new Version(entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0.0.0");
				base.LogEvent(133887472, LogLevel.Information, "Application started. Version is {Version}.", fileVersion);
			}

			public void IterationsFinished()
			{
				base.LogEvent(1163052199, LogLevel.Information, "All iterations finished.");
			}

			internal void LogApplicationEnd()
			{
				using var process = Process.GetCurrentProcess();
				var difference = DateTime.Now - process.StartTime;
				base.LogEvent(555776364, LogLevel.Information, "Application closed. Was running for {Runtime}.", difference);
			}

			#endregion
		}

		#endregion
	}
}