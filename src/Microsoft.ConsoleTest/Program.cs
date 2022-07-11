using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Extensions.Microsoft;
using Serilog.Core;

namespace Microsoft.ConsoleTest
{
	class Program
	{
		static async Task Main(string[] _)
		{
			Program.ChangeCulture();

			var builder = new ContainerBuilder();
			builder.RegisterModule<IocModule>();
			var container = builder.Build();

			//! Log events emitted from this class are only written to the debug window (and not to the console), because:
			//! • The logger is not configured to log to the console.
			//! • The logger functions are not returning anything that is then used to produce console output via Console.WriteLine.
			var logger = container.Resolve<ILogger>();

			try
			{
				logger.Log(Log.ApplicationStartEvent());
				var executor = container.Resolve<ExecutionExample>();
				await executor.StartWork();
				logger.Log(Log.IterationsFinishedEvent());
			}
			finally
			{
				Console.WriteLine();
				Console.WriteLine("Press any key to quit.");
				Console.ReadKey();
				logger.Log(Log.ApplicationEndEvent());
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

		static class Log
		{
			internal static LogEvent ApplicationStartEvent()
			{
				var entryAssembly = Assembly.GetEntryAssembly();
				var fileVersion = new Version(entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0.0.0");
				return new LogEvent(133887472, LogLevel.Information, "Application started. Version is {Version}.", fileVersion);
			}

			public static LogEvent IterationsFinishedEvent()
			{
				return new LogEvent(1163052199, LogLevel.Information, "All iterations finished.");
			}

			internal static LogEvent ApplicationEndEvent()
			{
				using var process = Process.GetCurrentProcess();
				var difference = DateTime.Now - process.StartTime;
				return new LogEvent(555776364, LogLevel.Information, "Application closed. Was running for {Runtime}.", difference);
			}
		}

		#endregion
	}
}