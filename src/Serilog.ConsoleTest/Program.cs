using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using IContainer = Autofac.IContainer;

namespace Serilog.ConsoleTest
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<IocModule>();
			var container = builder.Build();

			var logger = container.Resolve<Microsoft.Extensions.Logging.ILogger>();
			var entryAssembly = Assembly.GetEntryAssembly();
			var fileVersion = new Version(entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0.0.0");
			var informationalVersion = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? fileVersion.ToString();
			logger.LogInformation("Application started. Version is {Version}", informationalVersion);

			//IReadOnlyCollection<(int JobId, string JobName, string LocationName)> collection = new(int JobId, string JobName, string LocationName)[]
			//{
			//	(123, "Job#01", "Location#01"),
			//	(456, "Job#02", "Location#01"),
			//	(789, "Job#03", "Location#01")
			//};
			//logger.LogInformation("The following {Count} jobs where NOK {Jobs}.", collection.Count, collection);
			//await Task.Delay(100, CancellationToken.None);

			var cancellationTokenSource = new CancellationTokenSource();
			var token = cancellationTokenSource.Token;

			Console.WriteLine("Press CTRL+C to quit");
			var tasks = Enumerable
				.Range(1, 1)
				//.Range(1, 10)
				.Select(identifier => Program.CreateAndStartEndlessLogging(container, identifier, token))
				;

			//await Task.WhenAll(tasks);
			logger.LogInformation("Application stopped.");
		}

		static Task CreateAndStartEndlessLogging(IContainer container, int identifier, CancellationToken cancellationToken = default)
		{
			var operationScope = container.BeginLifetimeScope(childBuilder => childBuilder.RegisterModule(new OperationIocModule($"Operation{identifier:D3}")));
			var endlessLogging = operationScope.Resolve<EndlessLogging>();
			return endlessLogging.StartEndlessLogging(cancellationToken);
		}
	}

	class EndlessLogging
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly Microsoft.Extensions.Logging.ILogger _logger;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		public EndlessLogging(Microsoft.Extensions.Logging.ILogger logger)
		{
			// Save parameters.
			_logger = logger;

			// Initialize fields.
		}

		#endregion

		#region Methods

		internal async Task StartEndlessLogging(CancellationToken cancellationToken = default)
		{
			var iteration = 0;
			var @continue = true;
			do
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				try
				{

					using (_logger.BeginScope
					(
						new Dictionary<string, object?>()
						{
							{"Iteration", ++iteration},
						}
					))
					{
						_logger.LogDebug("Starting iteration {Iteration}");
						
						for (int i = 0; i < 6; i++)
						{
							using (_logger.BeginScope
							(
								new Dictionary<string, object?>()
								{
									{"Guid", Guid.NewGuid()},
								}
							))
							{
								var logLevel = (LogLevel) i;
								_logger.Log(logLevel, $"{logLevel} message.");
								await Task.Delay(1000, cancellationToken);
							}
						}
					}
				}
				catch (OperationCanceledException )
				{
					@continue = false;
				}
			}
			while (@continue);
		}

		#endregion
	}
}