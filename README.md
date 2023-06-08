# Phoenix.Functionality.Logging.Extensions

This repository contains different projects centered to help and improve with logging.

___

# Table of content

[toc]
___

# Logging.Base

|   .NET Framework   |     .NET Standard      |                     .NET                      |
| :----------------: | :--------------------: | :-------------------------------------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 6.0 |

## General Information

This package contains classes with basic functionality (hence the name).

## LogApplicationInformation

This class contains information about an application that is typically needed when logging. It provides the following properties and is used by some of the other packages.

| Property Name            | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| `Name`                   | The name of the application.                                 |
| `NumericIdentifier`      | A unique numeric identifier build from `Name` that can be used for example to register the application with a log target or to enrich log events. |
| `AlphanumericIdentifier` | A unique 20 chars long alpha-numeric identifier build from`Name` that can be used for example to register the application with a log target or to enrich log events. |
| `AssemblyVersion`        | The assembly version of the running executable, which is specified in the project file as [**AssemblyVersion**](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-version). |
| `FileVersion`            | The file version of the running executable, which is specified in the project file as [**FileVersion**](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-file-version). |
| `InformationalVersion`   | The informational version of the running executable, which is specified in the project file as [**InformationalVersion**](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-informational-version). |

Building an instance is done via **builder pattern**. The `Name` of the application can be composed in different ways, depending on the used methods during building.

The following example will try to obtain the application name from the **entry assembly**.

```c#
var info = LogApplicationInformation.Create().StartingWithApplicationName().Build();
```

Below is a more complex example.

```c#
var info = LogApplicationInformation
    .Create()
    .StartingWith("MyApplication")
    .SeparatedBy('-')
    .AndMachineName()
    .SeparatedByDash()
    .AndOperatingSystemInformation()
    .Build()
    ;
```

### Special instances

- The static `LogApplicationInformation.None` instance is internally used as a **NUll-object** and shouldn't be used in consumer code.
- The `LogApplicationInformation.Default` instance can be used if no customization to the application name is necessary, as it uses the **entry assembly** to obtain everything.

## Converting Log Levels

The `ILogLevelConverter<TSourceLogLevel, TTargetLogLevel>` interface defined in this package can be implemented when it is necessary to convert log levels from different logging systems (e.g. from **Serilog** to **Microsoft** implementations).

Currently the following specific implementations are available:

| Converter Name                      | Conversion                           | Package                                          |
| ----------------------------------- | ------------------------------------ | ------------------------------------------------ |
| SerilogToMicrosoftLogLevelConverter | Serilog :left_right_arrow: Microsoft | Phoenix.Functionality.Logging.Extensions.Serilog |

# Logging.Extensions.Microsoft

|   .NET Framework   |     .NET Standard      |                     .NET                      |
| :----------------: | :--------------------: | :-------------------------------------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 :heavy_check_mark: 6.0 |

## General Information

This package contains different helper classes that can be used when logging with [**Microsoft.Extensions.Logging**](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line).

## Logging with event ids

It is always good practice to add a unique id to each distinct log message. Using such an id has some benefits:
- Changing log messages (e.g. in case of misspelling) does not lead to not being able to find messages during error analysis anymore, since the id is a constant.
- Mostly no need to specify origin information like class name, function name and line number for a log entry.
- Log entries can be translated into different languages even after they have been written to a specific target without the need to apply complex regular expressions.

### Creating an event id

Below script for [**AutoHotkey**](https://www.autohotkey.com) creates a random number in the range from **0** to **2147483647** when **AltGr** + **i** is pressed. This number can be used as an event id.

```shell
; Create log event id
<^>!i:: ; ALTGR+I
Random, logIdentifier, 0, 2147483647
Send, %logIdentifier%
return
```

## Concept

The concept of how logging should be implemented, is shown in the `Microsoft.ConsoleTest` project. Basically each class that produces log output, should have a _static_ nested `Log` class that helps separates logging from business logic. This class is responsible for creating `LogScope`s and `LogEvent`s that can then be used with any regular **Microsoft.Extensions.ILogger** via the extension methods described [here](#Extensions).

The `LogScope` class encapsules all necessary data that are required to create a log scope, whereas the `LogEvent` class encapsules all necessary data that are required for emitting a log event.

```csharp
class MyClass
{
	private readonly ILogger _logger;

	public MyClass(Microsoft.Extensions.Logging.ILogger logger)
	{
		_logger = logger;
	}

	public void DoSomething(object data, int userId)
	{
		_logger.Log(Log.MessageScopeEvent(data, userId));
	}

	#region Logging

	// This is the static Log class.
	static class Log
	{
		// Create log events and/or scopes that can be used with an ILogger.
		internal static (LogScope, LogEvent) MessageScopeEvent(object data, int userId)
		{
			return
			(
				new LogScope(userId),
				new LogEvent(0, LogLevel.Information, "New data available {Data}.", data)
			);
		}
	}

	#endregion
}
```

## Logging with resources

A typical scenario when logging (especially exceptions) is writing the error to the log while simultaneously showing a message to the user (e.g. in a console application or via message boxes). The **log messages** in the backend should be readable by the application developer, to easily understand application state in case of errors. The user on the other hand should only see **output messages** in a language native to him. To get different **log**- and **output messages**, the special `LogResourceEvent` is available, that can be used to resolve those messages.

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Log messages</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Are by default resolved from resource of the culture <b>lo</b>. <i>Sorry to you guys in Lao.</i>
    </div>
</div>


<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Output messages</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Are resolved from resources matching the current applications culture.
    </div>
</div>

When using the appropriate `Log` extension method, the **log message** resolved from the `resourceName` parameter is directly passed to the decorated **Microsoft.Extensions.ILogger** and the **output message** will be returned by the function for further use.

```csharp
// Given a .resx file named "l10n.resx" containing a resource named 'Introduction':
var firstName = "Foo";
var lastName = "Bar";
var age = 99;
var logEvent = new LogResourceEvent
(
	1523340757,
	LogLevel.Debug,
	resourceManager: l10n.ResourceManager,
	resourceName: nameof(l10n.Introduction),
	logArgs: new object[] { firstName, lastName }
	messageArgs: new object[] { firstName, lastName, age }
);

// The function Will log: 'My name is {For} {Bar}.'
var outputMessage = logger.Log(logEvent);
// The returned ouput will be: 'Mein Name ist For Bar. Ich bin 99 Jahre alt.'
Console.WriteLine(outputMessage);
```
<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Information</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		As shown above, the format parameters can differ between what is used for logging and what is used to build the output message.
    </div>
</div>
## Logger groups

Logger groups is the concept of grouping multiple **Microsoft.Extensions.ILogger**s together, identifiable via a custom group identifier. The goal is to use those groups to apply certain methods to all the loggers belonging to it. Currently the groups only purpose is to easily apply log scopes to **different** logger instances. For example, if having a central module in an application, that triggers a complex workflow utilizing many classes based on some external criteria, it would be of great help, that every logger instance used throughout the workflow logs the criteria that originally triggered execution. To be more precise, the workflow could be processing a web request and the external criteria a request id. If the logger instance is **not** shared between all classes and processing is handled by different tasks that ignore the **SynchronizationContext**, then creating a log scope at the root of the workflow will only output the criteria for log messages emitted from the entry module. Logger groups allow to create a scope at the root level, that will automatically be applied to all other loggers that share the same group. As a result the trigger criteria would be logged even if all loggers are different instances, as long as they were registered as a group member.

### Usage

The static `LoggerGroupManager` class handles the different logger groups, but should typically not be used directly but implicitly via the below listed extension methods of **Microsoft.Extensions.ILogger**.

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

```c#
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

Below shows how three different classes, all belonging to a group identified via the enumeration `LoggerGroup.Event`, share a common logging scope. The main class `EventEmitter` creates a scope for this group that contains an **event id**. The logger instances of the helper classes `EventHandler` and `EventHandlerHelper` will implicitly use that **event id** with every log output they produce.

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

The `Phoenix.Functionality.Logging.Extensions.Microsoft` package also provides some extension methods for the original **Microsoft.Extensions.ILogger** that help with creating scopes, groups and writing logs.

### Logging

Below are some examples of the extension methods that can be used to emit log events.

- Directly logging `LogEvents`

```c#
// Single event.
var logEvent = new LogEvent(1163052199, LogLevel.Information, "All done.");
logger.Log(logEvent);
```
```c#
// Multiple events.
var logEvents = Enumerable
	.Range(0, 10)
	.Select(number => new LogEvent(546124364, LogLevel.Trace, "Number is {number}.", number))
	;
logger.Log(logEvents);
```

- Logging with single parameters

```c#
logger.Log(1732634211, LogLevel.Trace, "Finished");
```
```c#
var message = "All done";
logger.Log(1732634211, LogLevel.Trace, "Message {message} received.", message);
```
```c#
var ex = new Exception("Total disaster.");
logger.Log(1732634211, ex, LogLevel.Error, "An unexpected error occurred.");
```

### Scoping

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #37ff00; background-color: #37ff0020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Information</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		There are two types of extension methods that create scopes. The ones starting with <b>Create...</b> will return an <i>IDisposable</i> that can be used to remove the scope. The ones starting with <b>Pin...</b> will also create a scope, but will return the <i>ILogger</i> instance for chaining, thus making it impossible to remove the scope. Those extenion methods can be used when initially setting up logger instances.<br><br>The following examples will only contain the <b>Create...</b> methods, as the <b>Pin...</b> methods are only counterparts.
    </div>
</div>


Below are some examples of the extension methods that can be used to create log scopes.

```c#
var userId = 0;
var logScope = new LogScope(userId);
using (logger.CreateScope(logScope))
{
    // All logs emitted from within this scope will contain the user id.
}
```
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

### Groups

Groups are explained [here](#Logger-groups).

## NoLogger

The `Phoenix.Functionality.Logging.Extensions.Microsoft.NoLogger` is a simple null-object that can be accessed via the static `NoLogger.Instance` property and can be used to better implement nullable reference types.

## TraceLogger

The `Phoenix.Functionality.Logging.Extensions.Microsoft.TraceLogger` is a simple **ILogger** implementation that writes its log events to **System.Diagnostics.Trace** and - if available - to the console output. It can be instantiated or directly used via the static `TraceLogger.Instance` property.

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ffd200; background-color: #ffd20020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Advice</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		It does not support <b>log scopes</b> at all
    </div>
</div>

___

# Logging.Extensions.Microsoft.Autofac

|   .NET Framework   |     .NET Standard      |                     .NET                      |
| :----------------: | :--------------------: | :-------------------------------------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 :heavy_check_mark: 6.0 |

## General Information

This package contains helper functionality that can be used when registering components in [**Autofac**](https://autofac.org) along with a specific **Microsoft.Extensions.Logging.ILogger** instance.

## Extensions

Register a component that uses a named **ILogger** instance.

```c#
var builder = new ContainerBuilder();
var loggerName = "MyLogger";
var logger = _fixture.Create<ILogger>();

// Register the named logger.
builder
    .RegisterInstance(logger)
    .As<ILogger>()
    .Named<ILogger>(loggerName)
    ;

// Register some component that uses that named logger instance.
builder
    .RegisterType<MyClass>()
    .WithLogger(loggerName)
    .AsSelf()
    ;
```

Register a component with an **ILogger** that is manipulated before the component is resolved.

```c#
var builder = new ContainerBuilder();
var logger = _fixture.Create<ILogger>();

// Register the logger instance.
builder
    .RegisterInstance(logger)
    .As<ILogger>()
    ;

// Register some component that uses a logger instance which is added to a logger group.
builder
    .RegisterType<MyClass>()
    .WithLogger(l => l.AddToGroup("MyGroup"))
    .AsSelf()
    ;
```

Register a component with a named **ILogger** that is manipulated before the component is resolved.

```c#
var loggerName = "MyLogger";
var builder = new ContainerBuilder();
var logger = _fixture.Create<ILogger>();

// Register the named logger.
builder
    .RegisterInstance(logger)
    .As<ILogger>()
    .Named<ILogger>(loggerName)
    ;

// Register some component that uses the named logger instance which is added to a logger group.
builder
    .RegisterType<MyClass>()
    .WithLogger(loggerName, l => l.AddToGroup("MyGroup"))
    .AsSelf()
    ;
```

Register a component with an **ILogger** that allows that the resolved logger instance can be replaced. This can be used if the resolved logger needs to be wrapped in a [decorator](https://en.wikipedia.org/wiki/Decorator_pattern).

```c#
var builder = new ContainerBuilder();
var logger = _fixture.Create<ILogger>();

// Register the logger instance.
builder
    .RegisterInstance(logger)
    .As<ILogger>()
    ;

// Register some component that uses a different logger than the one that was resolved.
builder
    .RegisterType<MyClass>()
    .WithLogger(l => new DecoratedLogger(l))
    .AsSelf()
    ;
```



___

# Logging.Extensions.Serilog

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

The package provides some **ILogEventEnricher** that help adding data to log events. More information about log enrichment in general can be found [here](https://github.com/serilog/serilog/wiki/Enrichment).

#### `ApplicationInformationEnricher`

An **ILogEventEnricher** that adds configurable information about an application via [`LogApplicationInformation`](#LogApplicationInformation) to log events. What information will be used can be specified via the `ApplicationInformationEnricher.LogApplicationInformationParts` flags-enumeration during setup of the enricher. The enricher itself is accessible via an the `WithApplicationInformation` extension method of **Serilog.Configuration.LoggerEnrichmentConfiguration**.


```csharp
// Create the application information (or use LogApplicationInformation.Default if applicable).
var logApplicationInformation = LogApplicationInformation
    .Create()
    .StartingWithApplicationName
    .SeparatedByDash()
    .AndMachineName()
    .Build()
    ;

// Enrich a logger.
var configuration = new LoggerConfiguration()
	.Enrich.WithApplicationInformation
    (
    	logApplicationInformation,
    	ApplicationInformationEnricher.LogApplicationInformationParts.Name
	    | ApplicationInformationEnricher.LogApplicationInformationParts.NumericIdentifier
   	    | ApplicationInformationEnricher.LogApplicationInformationParts.InformationalVersion
	)
	.WriteTo.Debug()
	;
```

All overloads of `WithApplicationInformation` have an optional callback parameter that allows the used version to be modified. This can be used to pretty-print the version (e.g **1.0.0-beta1** could be rewritten into **1.0.0 Beta 1** or a custom string could be returned in cases the inferred version is **null**).

#### `ApplicationIdentifierEnricher`

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ff0000; background-color: #ff000020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Deprecated</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Use <i>ApplicationInformationEnricher</i> instead.
    </div>
</div>

An **ILogEventEnricher** that adds a unique application identifier to log events. The property name of the enriched application identifier will be **ApplicationIdentifier**. Creating the enricher can be done via one of the following constructors, which uses different approaches to creating the unique identifier.

```csharp
// Manually create the identifier.
var identifier = Guid.NewGuid().ToString();
var configuration = new LoggerConfiguration()
	.Enrich.WithApplicationIdentifier(identifier)
	.WriteTo.Debug()
	;
```

```csharp
// Let the identifier be created from a collection of values.
var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
var applicationName = entryAssembly.GetName().Name;
var applicationVersion = entryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
var configuration = new LoggerConfiguration()
	.Enrich.WithApplicationIdentifier(applicationName, applicationVersion)
	.WriteTo.Debug()
	;
```

#### `ApplicationVersionEnricher`

<div style='padding:0.1em; border-style: solid; border-width: 0px; border-left-width: 10px; border-color: #ff0000; background-color: #ff000020' >
	<span style='margin-left:1em; text-align:left'>
    	<b>Deprecated</b>
    </span>
    <br>
	<div style='margin-left:1em; margin-right:1em;'>
		Use <i>ApplicationInformationEnricher</i> instead.
    </div>
</div>
An **ILogEventEnricher** that adds the application version to log events. The property name of the enriched application identifier will be **ApplicationVersion**. The enricher can be added as follows:

```csharp
var configuration = new LoggerConfiguration()
	.Enrich.WithApplicationVersion(ApplicationVersionEnricher.VersionType.InformationalVersion)
	.WriteTo.Debug()
	;
```

The type of the version that is used can be selected between the following options of the `ApplicationVersionEnricher.VersionType` enumeration:

- [`AssemblyVersion`](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-version)
- [`FileVersion`](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-file-version)
- [`InformationalVersion`](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-informational-version)

Additionally an optional callback can be specified in the `WithApplicationVersion` extension method that allows the obtained version to be modified. This can be used to pretty-print the version (e.g **1.0.0-beta1** could be rewritten into **1.0.0 Beta 1** via string or regex replacements).

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

Whereas **Serilog.Extensions.Logging** uses **System.ThreadingAsyncLocal\<T\>** to add scope to its loggers in a seemingly magical way, this package only uses a simple collection of objects `FrameworkLoggerScopes` that stores the scope. This collection is an internal member of each logger instance and cannot be shared. When using the `FrameworkLogger`  within an application, controlling which logger shares the same scope is now all about which loggers are the same instance and no longer about the execution context of the loggers. Together with the [**logger groups**](#Logger-groups) feature, handling scope becomes more transparent. Additionally it no longer matters in which order scope is added to or removed from a `FrameworkLogger`. Each scope value can be removed from the internal collection at any time.

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

	Each instance of an application needs a unique name. For example this could be the normal name of the application suffixed with the computer it is running on (e.g. MyApplication@Home, MyApplication@Server, ...). This name is internally used to create a unique 20 alphanumeric characters long  **Api Key** that will be registered in the **Seq Server** if necessary.

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

In most cases registering a new (or already existing) application instance will succeed on the first try. But in some cases the server may be temporarily unavailable. The parameter `retryOnError` controls what should happen then.

- Retry on error  (true, default)

	The configuration will return a special sink that buffers messages while registering the application is repeatedly done in the background. Once the connection to the **Seq Server** was established and the **Api Key** has been registered, the queued messages are flushed to the server.

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

* **Felix Leistner**: _v1.x_ - _v2.x_