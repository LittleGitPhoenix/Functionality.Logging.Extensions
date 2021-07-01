using System;
using Autofac;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

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
					outputTemplate: "[{Timestamp:HH:mm:ss.ffff} {Level:u3}] {Message:lj} {EventId}{NewLine}{Exception}",
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

			// Option 1: Use the .Net ServiceCollection for mostly automatic logger registration.
			//var serviceCollection = new ServiceCollection();
			//serviceCollection
			//	.AddLogging
			//	(
			//		loggingBuilder =>
			//		{
			//			loggingBuilder.ClearProviders();
			//			loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
			//			loggingBuilder.AddSerilog(dispose: true);
			//		}
			//	)
			//	.BuildServiceProvider()
			//	;

			//// Integrate the service collection into the autofac container.
			//builder.Populate(serviceCollection);

			// // Option 2: Manually add the necessary bindings to the autofac container.
			builder
				.Register
				(
					handler => LoggerFactory.Create
					(
						loggingBuilder =>
						{
							loggingBuilder.ClearProviders();
							loggingBuilder.SetMinimumLevel(LogLevel.Trace);
							//loggingBuilder.AddProvider(new SerilogLoggerProvider(logger, true)); //! Same as 'AddSerilog' extension method.
							loggingBuilder.AddSerilog(logger, dispose: true);
						}
					)
				)
				.As<ILoggerFactory>()
				.SingleInstance()
				//.AutoActivate()
				;

			//builder
			//	.Register
			//	(
			//		handler => LoggerFactory.Create
			//		(
			//			loggingBuilder =>
			//			{
			//				loggingBuilder.ClearProviders();
			//				loggingBuilder.SetMinimumLevel(LogLevel.Warning);
			//				loggingBuilder.AddSerilog(dispose: true);
			//			}
			//		)
			//	)
			//	.Named<ILoggerFactory>("EFLoggerFactory")
			//	.SingleInstance()
			//	.AutoActivate()
			//	;

			// Register generic and un-generic ILoggers.
			builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
			builder.Register(context => context.Resolve<ILoggerFactory>().CreateLogger(String.Empty)).As<Microsoft.Extensions.Logging.ILogger>();
		}
	}

	//class OperationIocModule : Autofac.Module
	//{
	//	private readonly string _identifier;

	//	public OperationIocModule(string identifier)
	//	{
	//		_identifier = identifier;
	//	}

	//	/// <inheritdoc />
	//	protected override void Load(ContainerBuilder builder)
	//	{
	//		builder.Register(context => context.Resolve<ILoggerFactory>().CreateLogger(_identifier)).As<Microsoft.Extensions.Logging.ILogger>();
	//		builder.RegisterType<EndlessLogging>().AsSelf().SingleInstance();
	//	}
	//}
}
