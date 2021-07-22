# Phoenix.Functionality.Logging.Extensions

This repository contains different projects centered to help and improve with logging.

___

# Table of content

[toc]
___

# Phoenix.Functionality.Logging.Extensions.Microsoft

|   .NET Framework   |     .NET Standard      |          .NET          |
| :----------------: | :--------------------: | :--------------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

## General Information

This package contains different helper classes that can be used when logging with [**Microsoft.Extensions.Logging**](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line).

## Logging with event ids

It is always good practice to add an unique id to each different log message. Using such an id leads to some benefits:
- Changing log messages (e.g. in case of misspelling) does not lead to not being able to find messages during error analysis anymore, since the id is a constant.
- Mostly no need to specify origin information like class name, function name and line number for a log entry.
- Log entries can be translated into different languages even after they have been written to a specific target without the need to apply complex regular expressions.

### Classes

This package provides the following classes that help with writing logs along with a id:

#### `EventIdLogger`

This class decorates an **Microsoft.Extensions.ILogger** and additionally provides a single method:

```csharp
void LogEvent(int eventId, LogLevel logLevel, string logMessage, params object[] args)
```

This class is pretty straight forward. The `LogEvent` method just passes the value from the `logMessage`parameter to the underlying **Microsoft.Extensions.ILogger**. Nothing fancy about it.

#### `EventIdResourceLogger`

This class inherits from `EventIdLogger` and adds functionality, that helps when it comes to resolving log messages from **System.Resources.ResourceManager**.

```csharp
string LogEventFromResource(int eventId, LogLevel logLevel, string resourceName, object[]? logArgs = null, object[]? messageArgs = null)
```

A typical use case for this class is logging to a backend while simultaneously showing messages to the user (e.g. in a console application or via a message box). The log messages in the backend should be readable by the application developer, as to easily understand application state in case of errors. The user on the other hand should in many cases see a language native to him. The `EventIdResourceLogger` class accepts a collection of **System.Resources.ResourceManager**s which it later uses to resolve **log**- and **output** messages from.

- **Log** messages are resolved from resource of the culture **lo**. _Sorry to you guys in Lao._
- **Output** messages are resolved from resources matching the current applications culture.

When using the `LogEventFromResource` the appropriate **log** message resolved from the `resourceName` parameter is directly passed to the decorated **Microsoft.Extensions.ILogger** and the **output** message will be returned by the function for further use.

### Concept

The concept of how those classes should be used is shown in the `Microsoft.ConsoleTest` project. Basically each class that should produce log output, should have a nested `Logger` class that inherits from either of the above mentioned ones. Since the base class `EventIdLogger` decorates an **Microsoft.Extensions.ILogger**, each class can create an instance of its nested `Logger` by just passing this **Microsoft.Extensions.ILogger** to the constructor.

```csharp
class MyClass
{
	private readonly Logger _logger;

	public MyClass(Microsoft.Extensions.Logging.ILogger logger)
	{
		_logger = Logger.FromILogger(logger);
	}

	#region Logging

	private class Logger : EventIdLogger
	{
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
## Extensions

The `Phoenix.Functionality.Logging.Extensions.Microsoft` package also provides some extension methods to the original **Microsoft.Extensions.ILogger**.

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
using (logger.BeginScope(("User", user), ("Action", action)))
{
	//...
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
using (logger.BeginScope(() => user, () => action))
{
	//...
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
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

## General Information

This package contains different helper classes that can be used when logging with [**Serilog**](https://serilog.net).

## Serilog

### Settings

With some extension methods of **LoggerSettingsConfiguration** creating a new **LoggerConfiguration** and thus a new **Logger** from a json file is pretty simple.

```csharp
// Get the configuration file.
var configurationFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "serilog.config"));

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

## Serilog.Seq

If using [**Seq**](https://datalust.co/seq) as a sink for **Serilog** it is good practice to use an separate **Api Key** for each application forwarding logs to the **Seq Server** so that authentication and filtering can be handled by the server. Normally those **APi Keys** are manually created via the web gui of the **Seq Server** and then hard-code it into the application.

For some applications this may however not be feasible, e.g. if one application has many different installations each using a different feature set. In such cases it would be better to differentiate those instances from one another via different **Api Keys**. This package helps in creating and registering such **Api Keys** dynamically. To be able to use this feature, the following things are necessary:

- Configuration **Api Key**

	A separate **Api Key** has to be created in the **Seq Server** that is allowed to change the servers configuration. This **Api Key** is the one that will get hard-coded into the application, but won't be used to emit log messages. It is only used to dynamically create and retrieve other **Api Keys** for different application instances.

- Unique application name

	Each instance of an application needs a unique name. This could be the normal name of the application suffixed with the computer it is running on (e.g. MyApplication@Home, MyApplication@Server, ...). This name is used to create the unique 20 alphanumeric characters long  **Api Key** that will be registered in the **Seq Server** if necessary.

Then only the `Seq` extension method has to during configuration.

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

Alternatively this can be done via a json configuration. However, dynamically creating the application identifier from runtime information is not possible this way.

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

- Send a log file formatted in **Serilog's** [compact JSON format](https://github.com/serilog/serilog-formatting-compact) directly to a **Seq Server**.
```csharp
Task SendLogFileAsync(string applicationTitle, FileInfo logFile)
```



___

# Authors

* **Felix Leistner**: _v1.x_