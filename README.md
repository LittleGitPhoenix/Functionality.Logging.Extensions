# Phoenix.Functionality.Logging.Extensions

This repository contains different projects centered to help and improve with logging.

___

# Table of content

[toc]
___

# Phoenix.Functionality.Logging.Extensions.Microsoft

|   .NET Framework   |     .NET Standard      |                     .NET                      |
| :----------------: | :--------------------: | :-------------------------------------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 :heavy_check_mark: 6.0 |

## General Information

This package contains different helper classes that can be used when logging with [**Microsoft.Extensions.Logging**](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line).

## Logging with event ids

It is always good practice to add an unique id to each distinct log message. Using such an id has some benefits:
- Changing log messages (e.g. in case of misspelling) does not lead to not being able to find messages during error analysis anymore, since the id is a constant.
- Mostly no need to specify origin information like class name, function name and line number for a log entry.
- Log entries can be translated into different languages even after they have been written to a specific target without the need to apply complex regular expressions.

### Classes

This package provides the following classes that help with writing logs along with an id. All those classes are abstract and also decorators of a **Microsoft.Extensions.ILogger**.

- Since those classes are abstract, using them requires creating specific implementations. More on this is explained in the [concept](#Concept).
- Creating instances requires at least a **Microsoft.Extensions.ILogger** to be passed as constructor parameter. 

#### `EventIdLogger`

The abstract `EventIdLogger` has to be inherited by a custom logger class to be used. It is the base class of all other logger classes.

```csharp
private class MyLogger : EventIdLogger
{
	// Pass the Microsoft.Extensions.Logging.ILogger to the base class for internal usage.
	public MyLogger(Microsoft.Extensions.Logging.ILogger logger) : base(logger) { }
}
```

It provides just a single method, that basically just passes its parameter on to the underlying **Microsoft.Extensions.ILogger**.

```csharp
void LogEvent(int eventId, LogLevel logLevel, string logMessage, params object[] args)
```

#### `EventIdResourceLogger`

The `EventIdResourceLogger` is a descendent of [`EventIdLogger`](#EventIdLogger). It too is an abstract class and has to be inherited by a custom logger class to be used. A typical use case for this class is logging to a backend while simultaneously showing messages to the user (e.g. in a console application or via message boxes). The **log messages** in the backend should be readable by the application developer, as to easily understand application state in case of errors. The user on the other hand should only see **output messages** in a language native to him. To get the different **log**- and **output messages**, the `EventIdResourceLogger` requires a collection of **System.Resources.ResourceManager**s that it can use to resolve the messages.

- **Log messages** are by default resolved from resource of the culture **lo**. _Sorry to you guys in Lao._
- **Output messages** are resolved from resources matching the current applications culture.

```csharp
private class MyLogger : EventIdResourceLogger
{
	// Don't forget to pass ALL required System.Resources.ResourceManagers to the base class.
	public MyLogger(Microsoft.Extensions.Logging.ILogger logger) : base(logger, new[] { l10n.ResourceManager }) { }
}
```

When using the `LogEventFromResource` function, the appropriate **log message** resolved from the `resourceName` parameter is directly passed to the decorated **Microsoft.Extensions.ILogger** and the **output message** will be returned by the function for further use.

```csharp
string LogEventFromResource(int eventId, LogLevel logLevel, string resourceName, object[]? logArgs = null, object[]? messageArgs = null)
```

Here a more complete example:

```csharp
class MyLogger : EventIdResourceLogger
{
	#region (De)Constructors

	public MyLogger(Microsoft.Extensions.Logging.ILogger logger)
		: base(logger, new[] { Resources.l10n.ResourceManager }) { }

	#endregion

	#region Log methods

	internal string LogProgress(decimal progress)
	{
		// This will log the progress to the backend system with a log message that will be
		// resolved from the 'Resources\l10n.lo.resx' resource files 'Progress' entry.
		var outputMessage = base.LogEventFromResource
		(
			eventId: 1523340757,
			logLevel: LogLevel.Debug,
			resourceName: nameof(Resources.l10n.Progress),
			logArgs: new object[] { progress },
			messageArgs: new object[] { progress }
		);

		// This is the output message that was resolved from the
		// resource file matching the current threads CultureInfo.
		return outputMessage;
	}

	#endregion
}
```



### Concept

The concept of how logging should be implemented, is shown in the `Microsoft.ConsoleTest` project. Basically each class that produces log output, should have its own nested `Logger` class that inherits from either of the available classes mentioned [here](#Classes). Since this logger class is nested and additionally just a decorator for a **Microsoft.Extensions.ILogger**, the class producing the log output only requires such a **Microsoft.Extensions.ILogger** instance, making its public interface independent of the specific logger class.

```csharp
class MyClass
{
	private readonly Logger _logger;

    // The public interface (here the constructor) only requires a Microsoft.Extensions.Logging.ILogger.
    // This makes the class independent from the specific nested logger.
	public MyClass(Microsoft.Extensions.Logging.ILogger logger)
	{
		// Create the nested logger by passing the Microsoft.Extensions.Logging.ILogger to it.
		_logger = Logger.FromILogger(logger);
	}

	#region Logging

	// Nested logger
	private class Logger : EventIdLogger
	{
		// Pass the Microsoft.Extensions.Logging.ILogger to the base class for internal usage.
		private Logger(Microsoft.Extensions.Logging.ILogger logger) : base(logger) { }

		public static Logger FromILogger(Microsoft.Extensions.Logging.ILogger logger) => new Logger(logger);
	}

	#endregion
}
```

Emitting log messages should now **no longer** be done **directly** from the actual, but rather from the nested `Logger`class. For each distinct log message a separate method in the nested `Logger` should to be created. This method defines the unique and constant event id that identifies the message. Creating the id has to be done manually or with the help of the [**AutoHotkey**](#Creating-an-event-id) script.

```csharp
class MyClass
{
	private readonly Logger _logger;

	public MyClass(Microsoft.Extensions.Logging.ILogger logger)
	{
		_logger = Logger.FromILogger(logger);
		_logger.LogConstructed();
	}

	#region Logging

	private class Logger : EventIdLogger
	{
		private Logger(Microsoft.Extensions.Logging.ILogger logger) : base(logger) { }

		public static Logger FromILogger(Microsoft.Extensions.Logging.ILogger logger) => new Logger(logger);

		#region Log methods

		internal void LogConstructed()
		{
			base.LogEvent
			(
				eventId: 686462161,
				logLevel: LogLevel.Information,
				logMessage: "A new instance of {Instance} was created.",
				args: nameof(MyClass)
			);
		}
		
		#endregion
	}

	#endregion
}
```

### Creating an event id

This script for [**AutoHotkey**](https://www.autohotkey.com) creates a random number in the range from **0** to **2147483647** when **AltGr** + **i** is pressed. This number can be used as an event id.

```
; Create log event id
<^>!i:: ; ALTGR+I
Random, logIdentifier, 0, 2147483647
Send, %logIdentifier%
return
```
## Logger groups

Logger groups is the concept of grouping multiple **Microsoft.Extensions.ILogger**s together, identifiable via a custom group identifier. The goal is to use those groups to apply certain methods to all the loggers belonging to it. Currently the groups only purpose is to easily apply log scopes to **different** logger instances. For example if having a central module in an application, that triggers a complex workflow utilizing many classes based on some external criteria, it would be of great help, that every logger instance used throughout the workflow logs the criteria that originally triggered execution. To be more precise, the workflow could be processing a web request and the external criteria a request id. If the logger instance is **not** shared between all classes, then creating a log scope at the root of the workflow will only output the criteria for log messages emitted from the entry module. Logger groups allow to create a scope at the root level, that will automatically be applied to all other logger that share the same group. As a result the trigger criteria would be logged even if all loggers are different instances, as long as they have been registered as a group member.

### Usage

The static `LoggerGroupManager` class handles the different logger groups, but should typically not be used directly but implicitly via the below listed extension method of **Microsoft.Extensions.ILogger**.

**Add logger to group**

```csharp
ILogger AddToGroup(this ILogger logger, object groupIdentifier)   
```

Example:

```csharp
delegate ILogger LoggerFactory();
LoggerFactory factory = () => { ... };
ILogger logger = factory.Invoke().AddToGroup("SomeGroupIdentifier");
```

**Add logger to groups**

```csharp
ILogger AddToGroup(this ILogger logger, params object[] groupIdentifiers)
```

Example:

```csharp
delegate ILogger LoggerFactory();
LoggerFactory factory = () => { ... };
ILogger logger = factory.Invoke().AddToGroups("SomeGroupIdentifier", "AnotherGroupIdentifier");
```

**Address all loggers of a group**

To address all loggers sharing a group, it is only necessary to use the `AsGroup` extension method on **any** logger instance. It will return a collection of all loggers belonging to the group. The logger instance that is used mustn't even be part of the group at all. To create scopes for all those loggers, special `CreateScope` extension methods operating on logger collections are available.

```csharp
IReadOnlyCollection<ILogger> AsGroup(this ILogger _, object groupIdentifier)
```

Example:

```csharp
delegate ILogger LoggerFactory();
LoggerFactory factory = () => { ... };
ILogger logger = factory.Invoke().AddToGroups("SomeGroupIdentifier", "AnotherGroupIdentifier");
ILogger someLogger = factory.Invoke().AddToGroup("SomeGroupIdentifier");
ILogger anotherLogger = factory.Invoke().AddToGroup("AnotherGroupIdentifier");

var user = "John Doe";
var action = "Delete";
using (logger.AsGroup("SomeGroupIdentifier").CreateScope(("User", user)))
using (logger.AsGroup("AnotherGroupIdentifier").CreateScope(("Action", action)))
{
    logger.LogInformation("User {User} triggered {Action}.");
    someLogger.LogInformation("I am user {User}.");
    anotherLogger.LogInformation("Triggered {Action}.");
}
```
### Complete Example

Below shows how three different classes, all belonging to a group identified via the enumeration `LoggerGroup.Event`, "share" a common logging scope. The main class `EventEmitter` creates a scope for this group that contains an **event id**. The logger instances of the helper classes `EventHandler` and `EventHandlerHelper` will implicitly use that **event id** with every log output they produce.

```csharp
// Custom enumeration defining all available groups.
enum LoggerGroup
{
	Event,
	SomethingElse
}

class EventEmitter
{
	private readonly ILogger _logger;

	private readonly EventHandler _eventHandler;

	public EventEmitter(ILogger logger, EventHandler eventHandler)
	{
		// Add the logger to a group. The identifier can be any object.
		// It is even possible to add one logger to many different groups
		_logger = logger.AddToGroups(LoggerGroup.Event, LoggerGroup.SomethingElse);
		_eventHandler = eventHandler;
	}

	void EmitEvents()
	{
		for (var eventId = 0; eventId < 10; eventId++)
		{
			// Create the log scope.
			using (_logger.AsGroup(LoggerGroup.Event).CreateScope(() => eventId))
			{
				_eventHandler.HandleEvent();
			}
		}
	}
}

class EventHandler
{
	private readonly ILogger _logger; 

	private readonly EventHandlerHelper _eventHandlerHelper;

	public EventHandler(ILogger logger, EventHandlerHelper eventHandlerHelper)
	{
		// Add the logger to the same a group.
		_logger = logger.AddToGroup(LoggerGroup.Event);
		_eventHandlerHelper = eventHandlerHelper;
	}

	internal void HandleEvent()
	{
		// Even if the logger did not explicitly scope the event id,
		// its generated output will contain it because of the group.
		_logger.LogInformation("Starting to handle.");
		_eventHandlerHelper.Process();
	}
}

class EventHandlerHelper
{
	private readonly ILogger _logger;

	public EventHandlerHelper(ILogger logger)
	{
		// Add the logger to the same a group.
		_logger = logger.AddToGroup(LoggerGroup.Event);
	}

	internal void Process()
	{
		// Even if the logger did not explicitly scope the event id,
		// its generated output will contain it because of the group.
		_logger.LogInformation("Starting to help.");
	}
}
```



## Extensions

The `Phoenix.Functionality.Logging.Extensions.Microsoft` package also provides some extension methods to the original **Microsoft.Extensions.ILogger**.

### Groups

Groups are explained [here](#Logger-groups).

### Scoping

**Scopes from Tuples**

The following function helps creating log scopes by specifying the scope values as **ValueTuple**.

```csharp
IDisposable CreateScope(this ILogger logger, params (string Identifier, object? Value)[] scopedValues)
```
Example:

```csharp
var user = "John Doe";
var action = "Delete";
Microsoft.Extensions.Logging.ILogger logger = null;
using (logger.CreateScope(("User", user), ("Action", action)))
{
    logger.LogInformation("User {User} triggered {Action}.");
}
```

Output:

```json
{
  "@t": "2000-01-01T00:00:00.0000001Z",
  "@mt": "User 'John Doe' triggered 'Delete'.",
  "User": "John Doe",
  "Action": "Delete"
}
```

**Scopes from Expressions**

The following function helps creating log scopes by passing value as **Expression**s. The names of the values will be inferred from the **Expression** and converted into **PascalCase**.

```csharp
IDisposable CreateScope(this ILogger logger, params Expression<Func<object>>[] scopedValues)
```
Example:

```csharp
var user = "John Doe";
var action = "Delete";
Microsoft.Extensions.Logging.ILogger logger = null;
using (logger.CreateScope(() => user, () => action))
{
    logger.LogInformation("User {User} triggered {Action}.");
}
```

Output:

```json
{
  "@t": "2000-01-01T00:00:00.0000001Z",
  "@mt": "User 'John Doe' triggered 'Delete'.",
  "User": "John Doe",
  "Action": "Delete"
}
```

**Scopes from CallerArgumentExpression**

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Information</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		This is only available when using at least <b>.NET Core 3.0</b>.
    </div>
</div>

The following function helps creating log scopes by simply passing a value (like a variable) as parameter. The names of the values will be inferred via the [**System.Runtime.CompilerServices.CallerArgumentExpression**](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute?view=net-6.0) introduced in **C# 10**.


```csharp
IDisposable CreateScope(this ILogger logger, object? value[1...10], [CallerArgumentExpression("value1")] string? name[1...10] = default)
```
<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ffd200; background-color: #ffd20020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Advice</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		The current implementation allows for up to ten values to be added at a time to the scope. If more parameters are needed, the method must be called multiple times.
    </div>
</div>

Example:

```csharp
var user = "John Doe";
var action = "Delete";
Microsoft.Extensions.Logging.ILogger logger = null;
using (logger.CreateScope(user, action))
{
    logger.LogInformation("User {User} triggered {Action}.");
}
```

Output:

```json
{
  "@t": "2000-01-01T00:00:00.0000001Z",
  "@mt": "User 'John Doe' triggered 'Delete'.",
  "User": "John Doe",
  "Action": "Delete"
}
```
___

# Phoenix.Functionality.Logging.Extensions.Serilog

|   .NET Framework   |     .NET Standard      |          .NET          |
| :----------------: | :--------------------: | :--------------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 :heavy_check_mark: 6.0 |

## General Information

This package contains different helper classes that can be used when logging with [**Serilog**](https://serilog.net).

## Serilog

### Settings

With some extension methods of **LoggerSettingsConfiguration** creating a new **LoggerConfiguration** and thus a new **Logger** from a json file is pretty simple.

```csharp
// Get the configuration file.
var configurationFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog.config"));

// Build the configuration.
var configuration = new LoggerConfiguration()
	.ReadFrom
	.JsonFile
	(
		serilogConfigurationFile: configurationFile,
		serilogSectionName: "Serilog"
	);

// Create the logger.
var logger = configuration.CreateLogger();
```

### Enrichers

#### `ApplicationIdentifierEnricher`

An **ILogEventEnricher** that adds a unique application identifier to log events. The property name of the enriched application identifier will be `ApplicationIdentifier`. Creating the enricher can be done via one of the following constructors, which uses different approaches to creating the unique identifier.

```csharp
// Manually create the identifier.
var identifier = Guid.NewGuid().ToString();
var logger = Log.Logger = new LoggerConfiguration()
	.Enrich.WithApplicationIdentifier(identifier)
	.WriteTo.Debug()
	.CreateLogger()
	;
```

```csharp
// Let the identifier be created from a collection of values.
var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
var applicationName = entryAssembly.GetName().Name;
var applicationVersion = entryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
LoggerConfiguration? t = new LoggerConfiguration()
	.Enrich.WithApplicationIdentifier(applicationName, applicationVersion)
	.WriteTo.Debug()
	;
```



## Serilog.File

### `ArchiveHook`

This is a special [**FileLifecycleHooks**](https://github.com/serilog/serilog-sinks-file/blob/dev/src/Serilog.Sinks.File/Sinks/File/FileLifecycleHooks.cs) for the [serilog file sink](https://github.com/serilog/serilog-sinks-file), that compresses log files into zip archives and also only keeps a configurable amount of archived files. It lets you configure the following parameters:

|   Parameter   |     Description      | Default |
| :-- | :-- | :-- |
| amountOfFilesToKeep | The amount of archived files that should be kept. | 30 |
| compressionLevel | The **CompressionLevel** to use. | CompressionLevel.Fastest |
| archiveDirectory | The directory where the zipped log files are saved. | The directory of the log file. |

sample of a complete **Serilog** configuration file:

```json
{
	"Serilog": {
		"Using": [
			"Serilog.Formatting.Compact",
			"Serilog.Sinks.Async",
			"Serilog.Sinks.File",
			"Phoenix.Functionality.Logging.Extensions.Serilog.File"
		],
		"MinimumLevel": {
			"Default": "Verbose"
		"LevelSwitches": {
			"$fileSwitch": "Verbose"
		},
		"Enrich": [
			"FromLogContext",
			"WithThreadId"
		],
		"WriteTo:Async": {
			"Name": "Async",
			"Args": {
				"bufferSize": 1000,
				"blockWhenFull": false,
				"configure": [
					{
						"Name": "File",
						"Args": {
							"path": ".log\\log_.json",
							"formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
							"shared": false,
							"rollingInterval": "Day",
							"retainedFileCountLimit": 1,
							"fileSizeLimitBytes": null,
							"levelSwitch": "$fileSwitch",
							"hooks": "Phoenix.Functionality.Logging.Extensions.Serilog.File.ArchiveHook, Phoenix.Functionality.Logging.Extensions.Serilog.File"
						}
					}
				]
			}
		}
	}
}
```

When using the json based **Serilog** configuration with custom parameters, it is necessary to first create a class that then will be used in the configuration file. 

```csharp
using Phoenix.Functionality.Logging.Extensions.Serilog.File;

namespace MyApp.Logging
{
	public class SerilogHooks
	{
		public static ArchiveHook MyArchiveHook =>
			new ArchiveHook
			(
				amountOfFilesToKeep: 10,
				compressionLevel: CompressionLevel.Optimal,
				archiveDirectory: new DirectoryInfo(@"C:\LogArchives")
			);
	}
}
```

```json
"hooks": "MyApp.Logging.SerilogHooks::MyArchiveHook, MyApp"
```

## Serilog.Microsoft

This package provides an adapater for **Microsoft.Extensions.Logging.ILogger** named `FrameworkLogger` that forwards log events to a **Serilog.ILogger**. Most of the implementation is taken from the existing package [**Serilog.Extensions.Logging**](https://github.com/serilog/serilog-extensions-logging/) with one key difference: 

Whereas **Serilog.Extensions.Logging** uses **System.ThreadingAsyncLocal\<T\>** to add scope to its loggers in a seemingly magical way, this package only uses a simple collection of objects `FrameworkLoggerScopes` that stores the scope. This collection is an internal member of each logger instance and cannot be shared. When using the `FrameworkLogger`  within an application, controlling which logger shares the same scope is now all about which loggers are the same instance and no longer about the execution context of the loggers. Together with the [**logger groups**](#Logger-groups) feature, handling scope becomes more transparent. Additionally it no longer matter in which order scope is added to or removed from a `FrameworkLogger`. Each scope value can be removed from the internal collection at any time.

### IoC (Autofac)

Below is an example on how to register an **Microsoft.Extensions.ILogger** backed by **Serilog** using `FrameworkLogger` with **Autofac**.

```csharp
class LoggerModule : Autofac.Module
{
	/// <inheritdoc />
	protected override void Load(ContainerBuilder builder)
	{
		LoggerModule.RegisterLogging(builder);
	}

	private static void RegisterLogging(ContainerBuilder builder)
	{
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
			;

		// Create the logger.
		var logger = configuration.CreateLogger();

		// Register the logger factories.
		LoggerModule.RegisterLoggerFactories(builder, logger);
		
		// Register the logger.
		LoggerModule.RegisterLoggers(builder);
	}

	/// <summary>
	/// Directly use the <paramref name="logger"/> instance to register <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerFactory"/> and <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.NamedLoggerFactory"/>.
	/// </summary>
	private static void RegisterLoggerFactories(ContainerBuilder builder, Serilog.ILogger logger)
	{
		// Register the factory returning unnamed loggers.
		builder
			.Register
			(
				context =>
				{
					Microsoft.Extensions.Logging.ILogger Factory() => new FrameworkLogger(logger);
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
					Microsoft.Extensions.Logging.ILogger Factory(string name) => new FrameworkLogger(logger, name);
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
```

## Serilog.Seq

If using [**Seq**](https://datalust.co/seq) as a sink for **Serilog** it is good practice to use an separate **Api Key** for each application forwarding logs to the **Seq Server** so that authentication and filtering can be handled by the server. Normally those **Api Keys** are manually created via the web frontend of the **Seq Server** and then hard-coded into the application.

For some applications this may however not be feasible, e.g. if one application has many different installations each using a different configuration or feature set. In such cases it would be better to differentiate those instances from one another via different **Api Keys**. This package helps in creating and registering such **Api Keys** dynamically. To be able to use this feature, the following things are necessary:

- Configuration **Api Key**

	A separate **Api Key** has to be created in the **Seq Server** that is allowed to change the servers configuration. This **Api Key** is the one that will get hard-coded into the application, but won't be used to emit log messages. It is only used to dynamically create and retrieve other **Api Keys** for different application instances.

> This *admin* **Api Key** seems to need all available permission of the **Seq Server**:
>  - Ingest
>  - Read
>  - Write
>  - Setup

- Unique application name

	Each instance of an application needs a unique name. For example this could be the normal name of the application suffixed with the computer it is running on (e.g. MyApplication@Home, MyApplication@Server, ...). This name is used to create the unique 20 alphanumeric characters long  **Api Key** that will be registered in the **Seq Server** if necessary.

Then only the `Seq` extension method has to be called during configuration.

```csharp
var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
var applicationName = entryAssembly.GetName().Name;
var computerName = System.Environment.MachineName;
var logger = new LoggerConfiguration()
	.WriteTo.Seq
	(
		seqHost: "http://localhost",
		seqPort: 5341,
		applicationTitle: $"{applicationName}@{computerName}",
		configurationApiKey: "pYHlGsUQw5RsLSFTJHKF"
	)
	.CreateLogger()
	;
```

Alternatively this can be done via json configuration. However, dynamically creating the application identifier from runtime information is not possible this way.

```json
{
	"Serilog": {
		"Using": [
			"Serilog.Formatting.Compact",,
			"Serilog.Enrichers.Thread",
			"Phoenix.Functionality.Logging.Extensions.Serilog.Seq"
		],
		"MinimumLevel": {
			"Default": "Verbose"
		},
		"WriteTo": [
			{
				"Name": "Seq",
				"Args": {
					"seqHost": "http://localhost",
					"seqPort": 5341,
					"applicationTitle": "MyApplication@Home",
					"configurationApiKey": "pYHlGsUQw5RsLSFTJHKF",
					"queueSizeLimit": 1000
				}
			}
		]
	}
}
```

The `Seq` extension method has many parameters, most of them being optional, all documented. The most important ones are:

|   Parameter   |     Description      | Hint |
| :-- | :-- | :-- |
| seqHost | The host address of the **Seq Server**. |  |
| seqPort | The port where the **Seq Server** listens for messages. | Can be omitted if the `seqHost`includes the port. |
| applicationTitle | A unique application title. | Must be unique per instance of an application. |
| configurationApiKey | Existing **Api key** that is used to register the application. | Has to already exist in the **Seq Server** |
| retryOnError | Automatically retry registering until it succeeds. |  |

In most cases registering a new (or already existing) application instance will succeed on the first try. But in some cases the server may temporarily not be available. The parameter `retryOnError` controls what should happen then.

- Retry on error  (true, default)

	The configuration will return a special sink that buffers messages while registering the application is repeatedly is done in the background. Once the connection to the **Seq Server** was established and the **Api Key** has been registered, the queued messages are flushed to the server.

- Don't retry on error (false)

	The configuration will return a sink that just discards log messages.

If registering fails, those errors will be written to **Serilog.Debugging.SelfLog**. To see those error messages, enable and forward the output.

```cs
global::Serilog.Debugging.SelfLog.Enable(message => System.Diagnostics.Debug.WriteLine(message));
global::Serilog.Debugging.SelfLog.Enable(System.Console.Error);
```

### `SeqServer`

This class for interacting with a given **Seq Server** provides the following helper functionality:

- Register an application with the **Seq Server**.

    ```csharp
    var seqServer = new Phoenix.Functionality.Logging.Extensions.Serilog.Seq.SeqServer("localhost", 5341, "***");
    await seqServer.RegisterApplicationAsync("MyApplication");
    ```

- Send a log file formatted in **Serilog's** [compact JSON format](https://github.com/serilog/serilog-formatting-compact) directly to a **Seq Server**.

    ```csharp
    var logFile = new FileInfo("...");
    var seqServer = new Phoenix.Functionality.Logging.Extensions.Serilog.Seq.SeqServer("localhost", 5341, "***");
    await seqServer.SendLogFileAsync("MyApplication", logFile);
    ```



___

# Authors

* **Felix Leistner**: _v1.x_