﻿using Autofac;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Extensions.Serilog;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.ConsoleTest;

class IocModule : Autofac.Module
{

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        IocModule.RegisterLogging(builder);
    }

    private static void RegisterLogging(ContainerBuilder builder)
    {
        var applicationTitle = "Phoenix.LogEmitter";

        // Setup Serilog self logging.
        global::Serilog.Debugging.SelfLog.Enable(message => System.Diagnostics.Debug.WriteLine(message));
        global::Serilog.Debugging.SelfLog.Enable(System.Console.Error);

        // Create the serilog configuration.

        var configuration = new LoggerConfiguration()
            //.WriteTo.Debug()
            //.WriteTo.Console()
            .ReadFrom.JsonFile("serilog.config", "Serilog")
            //.ReadFrom.JsonFile("serilog_with_file.config", "Serilog")
            //.ReadFrom.JsonFile("serilog_with_seq.config", "Serilog")
            ;

        if (true)
        {
            configuration
                .WriteTo.Seq
                (
                    seqHost: "http://localhost",
                    seqPort: 5341,
                    applicationTitle: applicationTitle,
                    configurationApiKey: "pYHlGsUQw5RsLSFTJHKF"
                )
                //.WriteTo.Seq("http://localhost:5341", Serilog.Events.LogEventLevel.Verbose, apiKey: "Y3umZ02hux3ZlW4noezT") // Original Seq method.
                ;
        }
			
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
        builder.RegisterGeneric(typeof(Logger<>)).As(typeof(Microsoft.Extensions.Logging.ILogger<>));
        builder.Register(context => context.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger(String.Empty)).As<Microsoft.Extensions.Logging.ILogger>();
    }
}

class OperationIocModule : Autofac.Module
{
    private readonly string _identifier;

    public OperationIocModule(string identifier)
    {
        _identifier = identifier;
    }

    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context => context.Resolve<ILoggerFactory>().CreateLogger(_identifier)).As<Microsoft.Extensions.Logging.ILogger>();
        builder.RegisterType<EndlessLogging>().AsSelf().SingleInstance();
    }
}