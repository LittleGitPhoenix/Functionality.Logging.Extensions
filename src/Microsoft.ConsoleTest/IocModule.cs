using System;
using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Microsoft.ConsoleTest
{
	class IocModule : Autofac.Module
	{
		/// <inheritdoc />
		protected override void Load(ContainerBuilder builder)
		{
			IocModule.RegisterLogging(builder);
			builder.RegisterType<ExecutionExample>().AsSelf().SingleInstance();
		}

		private static void RegisterLogging(ContainerBuilder builder)
		{
			//var applicationTitle = "Phoenix.LogEmitter";

			// Setup Serilog self logging.
			global::Serilog.Debugging.SelfLog.Enable(message => System.Diagnostics.Debug.WriteLine(message));
			global::Serilog.Debugging.SelfLog.Enable(System.Console.Error);

			// Create the serilog configuration.
			var configuration = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Debug
				(
					outputTemplate: "[{Timestamp:HH:mm:ss.ffff} {Level:u3}] {Message:lj} {Scope} {EventId}{NewLine}{Exception}",
					restrictedToMinimumLevel: LogEventLevel.Verbose
				)
				//.WriteTo.Console
				//(
				//	outputTemplate: "[{Timestamp:HH:mm:ss.ffff} {Level:u3}] {Message:lj} {EventId}{NewLine}{Exception}",
				//	restrictedToMinimumLevel: LogEventLevel.Verbose
				//)
				;
			
			// Create the logger.
			var logger = configuration.CreateLogger();

			// Obsolete options.
			////IocModule.RegisterLoggersViaServiceCollection(builder);
			////IocModule.RegisterLoggersWithILoggerFactory(builder, logger);

			// Register the logger factories.
			IocModule.RegisterLoggerFactories(builder, logger);

			// Register the logger.
			IocModule.RegisterLoggers(builder);
		}

		/// <summary>
		/// This uses a <see cref="Extensions.DependencyInjection.ServiceCollection"/> to for registering the <see cref="Microsoft.Extensions.Logging.ILogger"/>s.
		/// </summary>
		/// <remarks> Needs nuget package <see cref="Autofac.Extensions.DependencyInjection"/>. </remarks>
		[Obsolete("This is not fully researched. For example it is unclear, how to use custom configuration.")]
		private static void RegisterLoggersViaServiceCollection(ContainerBuilder builder)
		{
			var serviceCollection = new Extensions.DependencyInjection.ServiceCollection();
			serviceCollection
				.AddLogging
				(
					loggingBuilder =>
					{
						loggingBuilder.ClearProviders();
						loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
						loggingBuilder.AddSerilog(dispose: true);
					}
				)
				.BuildServiceProvider()
				;

			// Integrate the service collection into the autofac container.
			builder.Populate(serviceCollection);
		}

		/// <summary>
		/// Use the <paramref name="logger"/> instance to register a <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> that then is used to get <see cref="Microsoft.Extensions.Logging.ILogger"/>s at runtime.
		/// </summary>
		/// <remarks> This options seems to lead to all created <see cref="Microsoft.Extensions.Logging.ILogger"/>s to share their log-scope. </remarks>
		[Obsolete("This should not be used, as all created ILoggers seem to share a common log-scope.")]
		private static void RegisterLoggersWithILoggerFactory(ContainerBuilder builder, Logger logger)
		{
			builder
				.Register
				(
					handler => LoggerFactory.Create
					(
						loggingBuilder =>
						{
							loggingBuilder.ClearProviders();
							loggingBuilder.SetMinimumLevel(LogLevel.Trace);
							//loggingBuilder.AddProvider(new Serilog.Extensions.Logging.SerilogLoggerProvider(logger, true)); //! Same as 'AddSerilog' extension method.
							loggingBuilder.AddSerilog(logger, dispose: true);
						}
					)
				)
				.As<Microsoft.Extensions.Logging.ILoggerFactory>()
				.SingleInstance()
				;

			// Register generic and un-generic ILoggers.
			builder.RegisterGeneric(typeof(Logger<>)).As(typeof(Microsoft.Extensions.Logging.ILogger<>));
			builder
				.Register
				(
					context =>
					{
						var loggerFactory = context.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>();
						//var logger = loggerFactory.CreateLogger(String.Empty);
						var logger = loggerFactory.CreateLogger(Guid.NewGuid().ToString());
						return logger;
					}
				)
				.As<Microsoft.Extensions.Logging.ILogger>()
				;
		}

		/// <summary>
		/// Directly use the <paramref name="logger"/> instance to register <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerFactory"/> and <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.NamedLoggerFactory"/>.
		/// </summary>
		/// <remarks> This is the only option that will create separate <see cref="Microsoft.Extensions.Logging.ILogger"/>s that will not share their log-scopes. </remarks>
		private static void RegisterLoggerFactories(ContainerBuilder builder, Logger logger)
		{
			// Register the factory returning unnamed loggers.
			builder
				.Register
				(
					context =>
					{
						ILogger Factory() => new Serilog.Extensions.Logging.SerilogLoggerProvider(logger, true).CreateLogger(String.Empty);
						return (Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerFactory)Factory;
					}
				)
				.As<Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerFactory>()
				.SingleInstance()
				;

			// Register the factory returning named loggers.
			builder
				.Register
				(
					context =>
					{
						ILogger Factory(string name) => new Serilog.Extensions.Logging.SerilogLoggerProvider(logger, true).CreateLogger(name);
						return (Phoenix.Functionality.Logging.Extensions.Microsoft.NamedLoggerFactory)Factory;
					}
				)
				.As<Phoenix.Functionality.Logging.Extensions.Microsoft.NamedLoggerFactory>()
				.SingleInstance()
				;
		}

		/// <summary>
		/// Directly use the factories to get <see cref="Microsoft.Extensions.Logging.ILogger"/>s at runtime from the container.
		/// </summary>
		/// <remarks> This is the only option that will create separate <see cref="Microsoft.Extensions.Logging.ILogger"/>s that will not share their log-scopes. </remarks>
		private static void RegisterLoggers(ContainerBuilder builder)
		{
			// Register unnamed loggers via the factory.
			builder
				.Register(context => context.Resolve<Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerFactory>().Invoke())
				.As<Microsoft.Extensions.Logging.ILogger>()
				;

			// Register a named logger via the factory.
			builder
				.Register(context => context.Resolve<Phoenix.Functionality.Logging.Extensions.Microsoft.NamedLoggerFactory>().Invoke("MyLogger"))
				.Named<Microsoft.Extensions.Logging.ILogger>("MyLogger")
				.SingleInstance()
				;
		}
	}
}
