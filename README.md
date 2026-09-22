# Phoenix.Functionality.Logging.Extensions

This repository contains different projects that aim to help with and improve structured logging.

___

# Table of content

[toc]
___

> [!NOTE]
>
> **TL;DR**
>
> If your use case is only logging during web requests that are processed in a straight sequence and you are also using Microsoft `IServiceCollection`, then read no further. But if your use case involves more complex scenarios, desktop applications or you simply want to break free from Microsoft's all-enthralling IoC approach, then this repository may be worth looking into.

# Overview

In modern .NET applications logging is mostly handled via an **Microsoft.Extensions.Logging.ILogger** that actually is an instance of some major log-library like **Serilog** or **NLog**. That means that in user code, only methods of Microsoft's `ILogger` can be used, despite the underlying logger probably having much more functionality. The minimal surface of the Microsoft `ILogger` leaves things missing at times. But that is not the only issue in modern .NET applications. Most guidelines and documentations nowadays only center around how logging is setup using Microsoft own IoC system, the `IServiceCollection `. It's rather rare to see examples about how to manually setup a logging system that ultimately just returns an instance of some `ILogger` that can then be used directly or registered in IoC systems that is not Microsoft's version of it. An example is the [README](https://github.com/serilog/serilog-extensions-logging/blob/v10.0.0/README.md) of the **Serilog.Extensions.Logging** package. Not a word on how to manually create an `Ilogger` that uses **Serilog** and its configured sinks under the hood. It all just about extension methods applied to an `IServiceCollection `. The rest is kept hidden.

This repository tries to tackle those shortcomings:

- It contains helper classes and methods to streamline logger setup in regards to using **Serilog** as underlying logging framework, **Autofac** as IoC system and **Seq** as log target.
- It features a thought through approach for different logging scenarios and requirements.
- It provides several extension methods to enhance usage of Microsofts minimal `ILogger` interface.



## Project structure

> [!NOTE]
>
> The name of a project indicates which logging frameworks it enhances. If the name starts with **Microsoft**, the project is probably at least referencing **Microsoft.Extensions.Logging.Abstractions**.

| Project               | Description                                                  |
| --------------------- | ------------------------------------------------------------ |
| **Base**              | Basic classes and helper functionality for logging. This is completely free of any references to Microsoft or any other logging provider. It contains independent base classes used for logging. |
| **Microsoft**         | Provides extension methods for `ILogger`s, special `ILogger` implementations and grouping mechanisms for `ILogger`s. |
| **Microsoft.Autofac** | Helpers for registering `ILogger` in **Autofac**.            |
| **Serilog**           | General helpers that can be used with **Serilog** like special `ILogEventEnricher`. Also contains helper for loading `LoggerConfiguration` directly from a separate file. |
| **Serilog.File**      | Helper functionality for using the **File** sink of **Serilog**. |
| **Serilog.Seq**       | Helper functionality for using the **Seq** sink of **Serilog**. |



## About Logging

> [!TIP]
>
> The following section describes what structured logging is and how this repository aims at helping with properly implementing it. It points at typical issues in the world of structured logging and how they can be solved. It introduces the mechanics, functionality and purpose of the many projects of this repository. Although it is a long read, it is worth taking the time to understand the background and how this repository works.

When this documents uses the word logging, this refers to structured logging. This is a method of recording log data that can be enriched by key-value pairs of supplementary and searchable information. One benefit of such information is that it can be carried along with the logger, allowing processes at every step of an execution chain to enrich log events with data, despite the issuer not knowing about it. For example, in a pipeline of a web request processor, the correlation/request-id should be part of every log record to easily follow it throughout the log. To achieve this with plain text logs, that id would have to be passed down the whole execution chain even though that this id is functionality-wise not required besides that it can then be logged. In structured logging that id can be added as ambient scope that is then picked up by the logger every time something is actually emitted. Such ambient log scope is represented by the `LogScope` class. The **Microsoft.Extensions.Logging.ILogger** enables creation of log scopes via its `BeginScope<TState>(TState state)` function. It returns an `IDisposable` so that any information added to the ambient scope can be removed again.

Sadly, just adding information at any point is not always enough to get the log records that one would expect. The ambient log scope is directly tied to the logger instance where the scope was added to in the first place. That raises one issue:

> [!CAUTION]
>
> Scope cannot be shared between different logger instances. If two different classes use different logger instances, but must share information, that is not directly supported.

So, multiple separate logger instances cannot easily share ambient log scope. The simple solution may be to use only a single logger instance. But that raises another issue: In a highly parallel execution flow (for example the above mentioned web server processing hundreds of incoming requests in parallel) it must be guaranteed that scope information is not contaminated by other parallel processes. How can this be solved?

1) One solution is having **each request** be processed in a pipeline that has its own logger instance. The underlying IoC system registers all classes that are required to process incoming requests in a way that a new logger instance is created for any request (commonly named **InstancePerRequest** or **InstancePerLifetimeScope** registration). That fully solves the issue in such scenarios at the cost of a little CPU time and more memory pressure.
2) Another solution is having only a single logger for all requests but binding the ambient log scope to the execution context by storing the scope as `AsyncLocal<TScope>`. The benefits of this are reduced memory allocations and probably also less CPU time used to always creating new instance. The downside is that the way the scope is actually stored is an implementation detail of the underlying logger. Microsoft's `ILogger` interface can do nothing in this regard itself. This approach can also seem like magic for anyone that is not familiar with  `AsyncLocal` (or at least its predecessor `ThreadLocal`).

And then there is a whole other world besides web servers: Regular (desktop) applications. They typically don't just process requests in sequence but usually have to handle user callbacks or must react to events. With applications it is much more common to have several classes all having different logger instances that are orchestrated to handle a specific workflow. Having different loggers means that solution 2 can't be used. Besides that, it is also uncommon to create new instances of classes every time a callback is invoked (in desktop applications). That rules solution 1 out. What are the remaining options?

It is a combination of using scopes that can be aware about their execution context and something called `LoggerGroup`s.  While groups allow to share scope between different logger instances, explicitly specifying awareness of the execution context allows to either separate or share scope in multiple parallel callbacks handled by a single logger.

First, let's look at how specifying **scope awareness** works: In applications a user may open a dialog. Assume, that this dialog is represented by a class named `Dialog`. Depending on what the user clicks in the dialog, several callback functions may get executed. For simplicity the below example only has a single callback function named `Handle`. It is important to understand, that only a single `Dialog` instance is created when the user opens the dialog and that all callbacks must be handled by that sole instance. The callbacks are not guaranteed to be executed in the same execution context that was used to create the dialog itself, after all we live in a multi-threaded and asynchronous world (just humor me on this). Therefore, creating a log scope at the dialog level (e.g. a log scope created in the constructor that enriches each log event with the id of the user that opened the dialog) may **not** flow to log events that are emitted in the callbacks as they may or may not be emitted in the same execution context. Another similar example are classes that subscribe and react to events. Due to the nature of event handling, the execution context in which the class was created is most likely not the same that any of the event callback handlers is executed in. The below example of the `Dialog` class shows how ambient scopes are created with explicit scope awareness using two different factory methods, thus allowing to use a single logger instance in a workflow that still supports parallel execution. Something that is mutual exclusive given the above solutions.

- **`LogScope.CreateIndependent`**

	Creates an independent scope available to the logger instance regardless of which task it actually accesses. This one is used in the constructor to enrich all log events of the `Dialog` class with the user id regardless of the execution context. This scope is added to **every** log event that the logger emits.

- **`LogScope.CreateAware`**

	Creates a scope that is bound to the execution context from which it was created, thus blocking access to the scope from other tasks. It is used in the `Handle` callback to enrich log events with a fictional callback id. Even if the `Handle` callback is invoked hundreds of times in parallel, each emitted log event would have its specific callback id added as scope.

```c#
class Dialog : IDisposable
{
	private readonly ILogger _logger;

	private readonly IDisposable _logScope;

	public Dialog(ILogger logger, int userId)
	{
		_logger = logger;

		// In order for every log event to carry the user id as a log scope, it must be created as independent (not execution context aware) as otherwise only the task that created the dialog would have the user id in its log scope, but not other tasks that are running in parallel and also emit log events related to the same dialog.
		_logScope = logger.Enrich(LogScope.CreateIndependent(userId));
		logger.LogInformation("Dialog created for user {UserId}.", userId);
	}

	internal void Handle(int callbackId, string result)
	{
		// The log scope containing the callback id must be execution context aware, so that logs from multiple parallel callbacks can differentiate themselves by their callback id.
		using (_logger.Enrich(LogScope.CreateAware(callbackId)))
		{
			_logger.LogInformation("Callback result was: {CallBackResult}.", result);
		}
	}

	/// <inheritdoc />
	public void Dispose() => _logScope.Dispose();
}
```

🔗 More about log scopes can be found [here](#Ambient-Scopes).

Now let's look at `LoggerGroup`s: The initial problem is that classes that have different logger instance (common in desktop applications) can't share ambient log scope. Separate logger instance don't know each other and therefore can't share something. The solution is to group selected loggers together and then use that group to address all those loggers at once. So, when is this necessary? Let's expand the above example a bit more. Assume that the dialog that the user opened is a settings dialog where application configuration can be changed. Further assume, that the application already has a single instance that manages the settings of the application. This settings manager can load and save settings. Given such a setup, the dialog now also requires the settings manager instance so that it has access to the application configuration and can persists changes made to it. If settings are saved due to the user updating the application configuration, it would be good to log this. And it might be even better to also log who applied the changes (auditing). Saving the settings is the responsibility of the settings manager, so it must emit the log event. But, by design the settings manager must be unaware of the concept of a user or a user id. It only knows how to load and save settings, nothing more. Therefore, it cannot log a user id along with the change in settings by itself. Here `LoggerGroup`s come into play. Both instances, the `SettingsDialog` and the `SettingsManager` have their own logger instances. During orchestration those loggers can be put into a group. And via this group, the dialog can create a log scope that is applied to all loggers within this group. That allows to create a scope for the user id in the `SettingsDialog` class that is picked up by the logger of the `SettingsManager` every time it emits a log event. Group orchestration and access is done by the following two extension methods to `ILogger`:

- **`AddToGroup`**

	Adds an `ILogger` to a named group. The identifier is a generic type. Best use a string or an enumeration value as identifier.

- **`AsGroup`**

	Access all `ILoggers` of a group via an identifier. This must be used to enrich all loggers of a group with the same ambient log scope.

```c#
class Orchestrator
{
	internal const string GroupIdentifier = "DialogSettingsGroup";

	void Setup()
	{
		Func<ILogger> loggerFactory = () => null!; // Assume that this creates new logger instances.

		// The below factories use 'AddToGroup' to add the logger instances they create into a group identified by a custom identifier.
		var settingsManagerFactory = () => new SettingsManager(loggerFactory.Invoke().AddToGroup(GroupIdentifier));
		var dialogFactory = (int userId) => new SettingsDialog(loggerFactory.Invoke().AddToGroup(GroupIdentifier), userId, settingsManagerFactory.Invoke());
	}
}

class SettingsDialog(ILogger logger, int userId, SettingsManager settingsManager)
{
	void UserPressedSaveButton()
	{
		// Before the scope is created, the logger is used to access the group by its identifier, so that the scope enriches all loggers in the group.
		// The scope must be created as execution context aware, so that other parallel calls to 'SettingsManager.Save' do not interfere with each other's log scopes.
		using var userIdScope = logger.AsGroup(Orchestrator.GroupIdentifier).Enrich(LogScope.CreateAware(userId));
		settingsManager.Save();
	}
}

class SettingsManager(ILogger logger)
{
	// Even though the 'SettingsManager' is unaware of the 'Dialog' and its user id scope, the log output will still contain the user id as attached property.
	internal void Save() => logger.LogInformation("Settings were saved.");
}
```

🔗 More about logger groups can be found [here](#Logger-Groups).

That concludes the introduction into structured logging and how typical use cases can be handled using this repository and its projects. There is more information to be found in the sections of the projects. Here are some of the more interesting and important parts:

- [LogEventTemplates](#Event-Templates)
- Serilog Setup



## Event IDs

It is always good practice to add a unique id to each distinct log message. Using such an id has several benefits:
- Changing log messages (e.g. in case of misspelling) does not lead to not being able to find messages during error analysis anymore, since the id is a constant.
- No need to specify origin information of a log entry like class name, function name and line number .
- Log entries can be translated into different languages even after they have been written to a specific target without the need to apply complex regular expressions.

Microsoft provides the `EventId` struct that is a combination of an numeric `Id` and an arbitrary `Name`. Since the `Id` alone perfectly identifies the log entry, the name is optional. Furthermore, an `EventId` can be implicitly casted from an integer. Therefore, simply using a randomized number is typically enough to have a unique event id for every log.

### Creating an event id

Below scripts for [**AutoHotkey**](https://www.autohotkey.com) create a random number when `AltGr` + `I` is pressed. This can be used as event id.

> [!NOTE]
>
> Both scripts use a limit range from **0** to **2147483647** as Microsoft's `EventId` uses an signed Int32 which ranges from **-2147483648** to **2147483647** but negative event ids are unwelcome.

- **AutoHotkey 1**

	```
	; Create log event id
	<^>!i:: ; ALTGR+I
	Random, logIdentifier, 0, 2147483647
	Send, %logIdentifier%
	return
	```

- **AutoHotkey 2**

	````
	; Create log event id
	<^>!i:: {
	    logIdentifier := Random(0, 2147483647)
	    Send logIdentifier
	}
	````
___

# Logging.Base

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 8.0 :heavy_check_mark: 10.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

This package contains classes with basic functionality (hence the name).



## Application Information

A class containing information about an application that is typically needed when logging. For example it is used by the special [Seq Sink](#Logging.Extensions.Serilog.Seq) to register an application with a Seq server.

`LogApplicationInformation` provides the following properties:

| Property                 | Description                                                  |
| ------------------------ | ------------------------------------------------------------ |
| `Name`                   | The name of the application.                                 |
| `NumericIdentifier`      | A unique numeric identifier build from `Name` that can be used for example to register the application with a log target or to enrich log events. |
| `AlphanumericIdentifier` | A unique 20 chars long alpha-numeric identifier build from`Name` that can be used for example to register the application with a log target or to enrich log events. |
| `AssemblyVersion`        | The assembly version of the running executable, which is specified in the project file as [**AssemblyVersion**](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-version). |
| `FileVersion`            | The file version of the running executable, which is specified in the project file as [**FileVersion**](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-file-version). |
| `InformationalVersion`   | The informational version of the running executable, which is specified in the project file as [**InformationalVersion**](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-informational-version). |

Building an instance is done via **builder pattern**. The `Name` of the application can be composed in different ways, depending on the which methods are used during building.

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



## Events

Log events are what is used to actually emit a log. It is represented by the `ILogEvent` interface and the actual `LogEvent` class.

> [!IMPORTANT]
>
> The `LogEvent` defined in the base project is a generic class that should be inherited. The reason is, that the base project does not know about the type of the log level or the type of the event id that an actual logging framework uses. The **Phoenix.Functionality.Logging.Extensions.Microsoft** package already has specific implementations using `Microsoft.Extensions.Logging.LogLevel` and `Microsoft.Extensions.Logging.EventId`. That specific implementation is a pure pass-through class that provides no additional logic and is not explicitly documented.

> [!TIP]
>
> Log events are typically not constructed directly but via [`LogEventTemplate`](#Event-Templates).

An `ILogEvent` provides the following properties:

| Property              | Description                                                 |
| --------------------- | ----------------------------------------------------------- |
| `EventId`             | The event id as generic type.                               |
| `LogLevel`            | The log level as generic type.                              |
| `LogMessage`          | The message to log.                                         |
| `Args`                | Format arguments for the `LogMessage`.                      |
| `Exception`           | Optional **Exception** that will be logged.                 |
| [`Payload`](#Payload) | Optional payload that is applied to the log event as scope. |





## Event Templates

`LogEventTemplate`s are meant to optimize memory allocation when logging. They are defined once as static properties and then (re)used to create actual `ILogEvent`s. They are an code-generator-free alternative to Microsoft's own [source generated message templates](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/high-performance-logging#define-logger-messages-with-source-generation) or the now already deprecated approach of [pre-defined message templates](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/high-performance-logging#legacy-approach-loggermessagedefine-for-net-framework-and-net-core-31).

> [!IMPORTANT]
>
> Same as for the `LogEvent` the `LogEventTemplate` and its [`LogResourceEventTemplate`](#Event-Templates-From-Resources) counterpart defined in the base project are generic and abstract classes that need to be implemented. The reason is, that the base project does not know about the type of the log level or the type of the event id that an actual logging framework uses. The **Phoenix.Functionality.Logging.Extensions.Microsoft** package already has specific implementations using `Microsoft.Extensions.Logging.LogLevel` and `Microsoft.Extensions.Logging.EventId`. Those specific implementations are pure pass-through classes that provide no additional logic and are not explicitly documented.

The idea behind `LogEventTemplate`s is to create static properties that contain the basic building blocks (like the `EventId` or the `LogMessage`) of all log events that will be emitted. The key word here is _static_. That means that those are only ever created once per application lifetime thus reducing memory footprint. Such a `LogEventTemplate` can then be `Build` into an actual log event with minimal overhead.

> [!TIP]
>
> To get at least a little bit of separation between actual code and logging, the `LogEventTemplate`s should be moved into a nested class called `Log`. This class then contains the log templates that are used to build and emit actual log events by using the [extension methods](#Extensions) provided by this package.

Since `LogEventTemplate` are defined during build time, they cannot contain data that is only available during runtime. Such need to be passed to the `Build` function of the template that creates an actual log event. Below is a simple example on how logging can be implemented into a class.

```csharp
class MyClass(Microsoft.Extensions.Logging.ILogger logger)
{
	public void DoSomething(int userId)
	{
		// Here the actual log event is created from the template supplying runtime data.
		var somethingEvent = Log.SomethingHappened.Build((userId, Unit.Value));
		logger.Log(somethingEvent);
	}

	#region Logging

	// This is the static Log class.
	static class Log
	{
		// The static log event template that accepts a user id as placeholder for its message.
		// Notice that the runtime data (the generic parameter) is actually a tuple.
		internal static LogEventTemplate<(int userId, Unit)> SomethingHappened { get; } = new()
		{
			EventId = 1668049180,
			LogLevel = LogLevel.Information,
			LogMessage = "Something happened to user {UserId}.",
		};
	}

	#endregion
}
```

The above example already shows something special in terms of the `LogEventTemplate`s: The `Unit` and its static `Value`  property. A `Unit` is  a type that has only a single value, typically used to indicate the absence of a meaningful result. It is required because the templates use a tuple as generic argument that defines the types and names of the dynamically added placeholders. Tuples **must** contain at least two values. That is a limitation of .NET. The above example only requires a single placeholder of type `int` (with an optional name `userId`). To still use tuples as generic argument though, the `Unit` type was introduced which is basically an empty placeholder in case only a single placeholder is required. If a `LogEventTemplate` has more then one placeholder, then `Unit` is not required. Those are the different variants that can be used to define a template:

- No placeholder

	```c#
	internal static LogEventTemplate NoPlaceholder { get; } = new()
	{
		EventId = 1698180849,
		LogLevel = LogLevel.Information,
		LogMessage = "Just a message.",
	};
	```

- Single placeholder - requires `Unit` and must be called with `Build((..., Unit.Value))`

	```c#
	internal static LogEventTemplate<(Guid identifier, Unit)> SinglePlaceholder { get; } = new()
	{
		EventId = 418730146,
		LogLevel = LogLevel.Information,
		LogMessage = "Identifier is: {Identifier}.",
	};
	```

- Multiple placeholders

	```c#
	internal static LogEventTemplate<(int userId, string userName)> MultiplePlaceholders { get; } = new()
	{
		EventId = 1949425686,
		LogLevel = LogLevel.Information,
		LogMessage = "The name of user {UserId} is {UserName}.",
	};
	```



### Event Templates From Resources

A typical logging-scenario is writing an exception to the log while also showing a message to the user (e.g. in a console application or via message boxes). The **log messages** in the backend should be readable by the application developer, to easily understand application state in case of errors. The user on the other hand should only see **output messages** in a language native to him and probably with less complex wording or information. To get different **log**- and **output messages**, the special `LogResourceEventTemplate` is available, that can be used to resolve those different messages. This special template behaves identical to the regular `LogEventTemplate`, with the difference that it uses a `RessourceManager` and the name of a resource within it instead of a message template.

> [!NOTE]
>
> - **Log messages** are by default resolved from resources of the **lo** culture. This can be changed via the static `LogResourceEventSettings.LogCulture` property however.
> - **Output messages** are resolved from resources matching the current applications culture.



`LogResourceEventTemplate` supports resolving messages that have a different amount of placeholders per culture. For example the default log culture **lo** typically has more placeholders then the actual cultures used to resolve the output message. This is because log messages are more in depth then the output message shown to the user. This is supported by specifying a second generic parameter when defining the template.

- Log and output message have the same placeholders (or the output message has none) - one generic tuple parameter is required only

	```c#
	// If the messages in the resource have the same amount of placeholders (or the output message has no placeholders at all), only a single tuple must be specified.
	internal static LogResourceEventTemplate<(int userId, string userName)> IdenticalResourcePlaceholders { get; } = new()
	{
		EventId = 1785678218,
		LogLevel = LogLevel.Information,
		ResourceManager = l10n.ResourceManager,
		ResourceName = nameof(l10n.MessageWithMatchingPlaceholders),
	};
	```

- Log and output message have different placeholders - two generic tuple parameters need to be specified, the first for the log message, the second for the output message

	```c#
	// If the messages in the resource have a different amount of placeholders, specify a second tuple that specifies the placeholders for the output message seperatly.
	internal static LogResourceEventTemplate<(int userId, string userName), (string useName, Unit)> DifferentResourcePlaceholders { get; } = new()
	{
		EventId = 1805702069,
		LogLevel = LogLevel.Information,
		ResourceManager = l10n.ResourceManager,
		ResourceName = nameof(l10n.MessageWithDifferentPlaceholders),
	};
	```



When using the appropriate `Log` extension method, the **log message** resolved from `ResourceName` is directly passed to the **Microsoft.Extensions.ILogger** and the **output message** will be returned by the function for further use.

```csharp
// Build the actual log event from the LogResourceEventTemplate.
var resourceEvent = Log.LogResourceEventTemplate.Build((10, "John"));
// When emitting the log via the special 'Log' extension method, the output message is returned.
var outputMessage = logger.Log(resourceEvent);
// Use the output message accordingly.
Console.WriteLine(outputMessage);
```



## Ambient Scopes

The [introduction](#About-Logging) already contains a good explanation about what ambient log scope is, what issues regarding them arise in complex use cases and how to solve them. It also established the concept of **execution awareness** in regards to log scopes. This section therefore centers around how to actually create `LogScope`s.

As established in the introduction, there are two fundamentally different types of `LogScope`s differentiated via the `LogScopeType`.

- `LogScope`s using **``LogScopeType.Independent``**

	A scope that is **not** influenced or controlled by external factors. No matter which task or thread accesses this kind of scope, it sees all elements of that scope.

- `LogScope`s using **``LogScopeType.ExecutionContextAware``**

	A scope that is bound to the current execution context. That means that only elements are visible to a caller that were added to the scope from the same execution context that the caller is on.



### Handling

Just having two different types of log scopes is only one part necessary to make ambient scopes work. The other is having a system that knows about those types and then acting (or storing) them in a way that meets their intent. This is the task of an `ILogScopeManager` that stores the scope for a logger and applies it to the log events that are emitted through that logger. The specific `LogScopeManager` of this library is aware of `ILogScope`s and stores them depending on their `LogScopeType`:

- **`LogScopeType.Independent`**

	Stored in a single shared `ConcurrentDictionary`. Every log event emitted through any execution context sees these scopes.

- **`LogScopeType.ExecutionContextAware`**

	Stored using copy-on-write `AsyncLocal<T>` semantics. Each `AddScope` call creates a new dictionary snapshot and assigns it only to the current execution-context slot. Parent and sibling contexts retain their own snapshots. Disposing the returned `IDisposable` restores the previous snapshot, making scope management behave like a stack.

> [!NOTE]
>
> To enable loggers to use the `LogScopeManager` and thus enabling proper handling for the two ambient scope types at all, special implementations of loggers need to be created. One such is the [**`LogScopeHandlingLogger`**](#LogScopeHandlingLogger) - a `Microsoft.Extensions.Logging.ILogger`.

> [!IMPORTANT]
>
> The `LogScopeManager` treats ambient scope that is not an `ILogScope` (e.g. a plain string) as execution context-aware by default.



#### Isolation Levels

Understanding _which_ `LogScopeType` to use requires distinguishing two separate isolation problems:

**Horizontal: Concurrent operations on the same class instance**

A **singleton class** handles many concurrent operations (e.g. a web API handler receiving 1000 parallel HTTP requests). Each operation must carry its own scope data (e.g. a trace id) without leaking into other operations. `LogScopeType.ExecutionContextAware` solves this as each request is run in its own execution context, so each context slot holds only that request's scope data.

**Vertical: Class hierarchy (Parent → Child)**

A parent class calls a child class. Both share the same `ILogger` instance. The child adds a scope. This scope must **not** appear in log events emitted by the parent after the child finishes. This problem **cannot** be solved by `LogScopeType` alone. The recommended solution is to give each class its own `ILogger` instance (and therefore its own `LogScopeManager`) and group them via `ILoggerGroup` when shared scope is needed.

> [!CAUTION]
>
> **Known limitation: Synchronous preamble bleed-back**
>
> `ExecutionContextAware` isolation only takes effect once the .NET runtime creates a new execution context (e.g. via `Task.Run` or a truly yielding `await`). If a child method adds its scope *before* its first actual suspension point, both caller and callee share the same execution context at that moment. The `AsyncLocal` assignment therefore overwrites the caller's slot directly. Additionally, if the scope is disposed on a thread-pool continuation thread (common with `ConfigureAwait(false)`), the restore applies to that continuation's context and not to the original caller's slot. The caller is permanently left carrying the child's scope.
>
> This example shows the two problems:
>
> ```c#
> class Parent(ILogger logger)
> {
> 	async Task Execute()
> 	{
> 		var parentScope = LogScope.CreateAware("ParentScope");
> 		using (logger.Enrich(parentScope))
> 		{
> 			var child = new Child(logger);
> 			var childTask = child.Execute();
> 			while (!childTask.IsCompleted)
> 			{
> 				await Task.Delay(1000);
> 				// This log event does contain both scopes.
> 				logger.LogInformation("Work in progress...");
> 			}
> 			// This log event could still contain both scopes.
> 			// That depends on whether the child disposed its scope on the original thread
> 			// or not (which is controlled only by the runtime).
> 			logger.LogInformation("Work done.");
> 		}
> 	}
> }
> 
> class Child(ILogger logger)
> {
> 	public async Task Execute()
> 	{
> 		// Scope is created in the same execution context.
> 		var childScope = LogScope.CreateAware("ChildScope");
> 		using (logger.Enrich(childScope))
> 		{
> 			// Only here is a new execution context created.
> 			await Task.Delay(10000).ConfigureAwait(false);
> 		}
> 		// After the using block it is likely that the child scope isn't removed from the
> 		// original execution context because 'ConfigureAwait(false)' runs the dispose logic
> 		// on the child's continuation thread.
> 	}
> }
> ```
>
> **This is a structural .NET limitation.** The logger has no control over when a new execution context is created. The recommended mitigation is the architecture described above: separate `ILogger` per class, grouped via `ILoggerGroup`.



### Choosing a LogScopeType

Use the following two questions to pick the right type every time:

- Scope that belongs to **this execution** (a request, an operation, a unit of work) → `LogScopeType.ExecutionContextAware` / `LogScope.CreateAware(...)`
- Scope that belongs to **this logger forever** (service name, version, environment) → `LogScopeType.Independent` / `LogScope.CreateIndependent(...)`

The four canonical scenarios are:

| Scenario | Logger setup | `LogScopeType` | Typical data |
|---|---|---|---|
| Concurrent operations on one class | Single `ILogger` (e.g. singleton) | `ExecutionContextAware` | Request trace id, correlation id |
| Static metadata for one class | Single `ILogger` | `Independent` | Service name, version, environment |
| Static metadata shared across classes | `ILoggerGroup` | `Independent` | Subsystem name, build number |
| Per-operation scope shared across classes | `ILoggerGroup` | `ExecutionContextAware` | Request trace id applied to all loggers in the group |



### Creation

Log scopes are created by the `Create...` factory methods provided by the `LogScope` class. As described above there are two types of scope. The ones that are aware of their execution context and those that are not. This is represented by the `LogScopeType` enumeration that must be specified when creating a scope.

- Independent (or unaware) scopes

	```c#
	LogScope.Create(LogScopeType.Independent, ("Property", "Value"));
	// or
	LogScope.CreateIndependent(("Property", "Value"));
	```

- Execution context aware scopes

	```c#
	LogScope.Create(LogScopeType.ExecutionContextAware, ("Property", "Value"));
	// or
	LogScope.CreateAware(("Property", "Value"));
	```




### Content

The actual content of the scope can be specified in different ways and depending on the .NET runtime.

- From **Tuples**

	Create log scopes by specifying the scope values as **ValueTuple**.

	```c#
	LogScope.CreateAware(("FirstName", "John"), ("LastName", "Doe"));
	```

- From **Expressions**

	Create log scopes by passing values as **Expression**s. The names of the values will be inferred from the **Expression** and converted into **PascalCase**.

	```c#
	var user = "John Doe";
	var action = "Delete";
	LogScope.CreateAware(() => user, () => action);
	```

- From **CallerArgumentExpression**

	Create log scopes by simply passing a variable as parameter. The names of the values will be inferred via the [**System.Runtime.CompilerServices.CallerArgumentExpression**](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute?view=net-6.0) introduced in **C#10**.

	> [!NOTE]
	> This is only available when targeting at least **.NET Core 3.0**.

	> [!IMPORTANT]
	> The current implementation allows for up to **ten values** to be added at a time. If more parameters are needed, the method must be called multiple scopes must be created.

	```c#
	var user = "John Doe";
	var action = "Delete";
	LogScope.CreateAware(user, action);
	```



### Disposal

`ILogScope` represent ambient data that is bound to a logger. Since such data can be temporary in nature, applying a scope to a logger will return an `IDisposable`. Disposing that will remove the scope from the logger again.

> [!TIP]
>
> More information about scope is applied to an `Microsoft.Extensions.Logging.ILogger` can be found [here](#Scoping).

```c#
var correlationId = Guid.NewGuid();
var startTime = DateTime.UtcNow;
// Combine all relevant values into a single scope or create one scope per value.
var logScope = LogScope.CreateAware(correlationId, startTime);
IDisposable appliedLogScope = logger.Enrich(logScope);
using (appliedLogScope)
{
	// Each of the emitted logs will carry the correlation id and the start time.
	logger.LogInformation("...");
	logger.LogInformation("...");
}
// This log event will not have the correlation id or start time as attached property anymore.
logger.LogInformation("...");
```



### Payload

The `PayLoad` is a special `ILogScope` implementation that is directly bound to a **specific** `ILogEvent`. It represent arbitrary data that is logged as additional properties along with that log event. It is typically used to **directly** enrich a log message with additional information. Since it is bound to a log event, it must always be execution context aware (`LogScope.CreateAware`). The `Payload` can be attached to a log event by passing it as parameter when building a log event from a `LogEventTemplate`. During logging the payload will be added like any other execution context aware scope. Once logging finished, the payload-scope will be automatically removed by disposing it - no need to do this manually.

```c#
ILogger logger = null!
var someTemplate = new LogEventTemplate() { ... };
var importantData = "Some important pice of information.";
var logEvent = someTemplate.Build(payload: Payload.Create(importantData));
// This will log the event along with the specified payload.
logger.Log(logEvent);
```



___

# Logging.Extensions.Microsoft

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 8.0 :heavy_check_mark: 10.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

## General Information

This package contains different helper classes that can be used when logging with [**Microsoft.Extensions.Logging**](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line).



## Extensions

The `Phoenix.Functionality.Logging.Extensions.Microsoft` package provides several extension methods for the original **Microsoft.Extensions.ILogger** that help with creating scopes, groups and writing logs.

### Logging

Below are some examples of the extension methods that can be used to emit log events.

- Logging using `ILogEvents` - Those are encouraged to be used.

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

	```c#
	// Error event.
	var logEvent = new LogEvent(1163052199, new Exception("ERROR"), LogLevel.Information, "All done.");
	logger.Log(logEvent);
	```

- Logging using parameters

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

An `ILogScope` can be used to `Enrich` a log event like this.

```c#
ILogger logger = null!;
ILogScope logScope1 = null!;
ILogScope logScope2 = null!;
using (logger.Enrich(logScope1).Enrich(logScope2))
{
	logger.LogInformation("...");
}
```

In some cases it may be beneficial to directly log something after a log scope was applied to a logger.

```c#
var correlationId = Guid.NewGuid();
var startTime = DateTime.UtcNow;
var correlationIdScope = LogScope.CreateAware(correlationId);
var startTimeScope = LogScope.CreateAware(startTime);
using 
	(
		logger
			.Enrich(correlationIdScope)
			.Enrich(startTimeScope)
			.Log(Log.NoPlaceholder.Build())
			// Calling 'Use' is necessary to cast the logger back to an IDisposable after above logging was executed. If nothing is logged (or logging is carried out in the using block itself), then 'Use' is not necessary.
			.Use()
	)
{
	// Execute work.
}
```

Then there is a special option that allows to **permanently enrich** a logger with scope. An example would be the application name or the application version. Such data will not change during the lifetime of any application. The method is named `EnrichPermanently`. It does not return an `IDisposable` but the same logger that was enriched. This is mostly used during orchestration where scope will be pinned to a logger forever. Note that the **permanent** guarantee only holds in full when using `LogScope.CreateIndependent`. When using `LogScope.CreateAware`, the scope is stored per execution context and will only be visible to the current and child execution contexts. It will not appear on log events from unrelated execution contexts.

```c#
var applicationName = LogApplicationInformation.Default.Name;
var applicationVersion = LogApplicationInformation.Default.AssemblyVersion;
logger.EnrichPermanently(LogScope.CreateIndependent(applicationName, applicationVersion));
```



### Groups

Groups are explained [below](#Logger-Groups).



## Logger Groups

Logger groups are the concept of grouping multiple **Microsoft.Extensions.ILogger**s together into an **`ILoggerGroup`**, identifiable by a custom group identifier. The goal is to use those groups to apply certain methods to all the loggers within it. Currently the groups only purpose is applying log scopes to **different** logger instances. More about when to use a `LoggerGroup` can be found in the [introduction](#About-Logging).

> [!NOTE]
>
> Since an `ILoggerGroup` handles specific loggers (in this case `Microsoft.Extensions.Logging.ILogger`s), it is implemented in the **Microsoft** specific project.

### Usage

The static `LoggerGroupManager` class handles `ILoggerGroup`s, but should typically not be used directly. It is implicitly used by the below listed extension methods of **Microsoft.Extensions.ILogger**.

- Add logger to group

	```c#
	ILogger logger = null!;
	logger.AddToGroup("MyGroup");
	```

- Add logger to multiple groups

	```c#
	ILogger logger = null!;
	logger.AddToGroups(applyExistingScope: true, "MyGroup", "AnotherGroup");
	```

- Address all loggers of a group

	To address all loggers of a group, use the `AsGroup` extension method on **any** logger instance. It will return the `ILoggerGroup` matching the given identifier. The logger instance that is used mustn't even be part of the group at all - it is just used as entry point for the `AsGroup` extension method. To create scopes for the loggers of a `ILoggerGroup`, special `Enrich` extension methods are available.

	```c#
	Func<ILogger> factory = null!;
	ILogger logger1 = factory.Invoke().AddToGroups(true, "Group1", "Group2");
	ILogger logger2 = factory.Invoke().AddToGroup("Group1");
	ILogger logger3 = factory.Invoke().AddToGroup("Group2");
	
	var user = "John Doe";
	var action = "Delete";
	using (logger1.AsGroup("Group1").Enrich(LogScopeType.ExecutionContextAware, user))
	using (logger1.AsGroup("Group2").Enrich(LogScopeType.ExecutionContextAware, action))
	{
		logger1.LogInformation("User {User} triggered {Action}.");
		logger2.LogInformation("I am user {User}.");
		logger3.LogInformation("Triggered {Action}.");
	}
	```

	

### Complete Example

Below shows how three different classes, all belonging to a group identified via the enumeration value `LoggerGroup.Event`, share a common logging scope. The main class `EventEmitter` creates a scope for this group that contains an **event id**. The logger instances of the helper classes `EventHandler` and `EventHandlerHelper` will implicitly use that **event id** with every log output they produce.

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
		_logger = logger.AddToGroups(true, LoggerGroup.Event, LoggerGroup.SomethingElse);
		_eventHandler = eventHandler;
	}

	void EmitEvents()
	{
		for (var eventId = 0; eventId < 10; eventId++)
		{
			// Create the log scope.
			using (_logger.AsGroup(LoggerGroup.Event).Enrich(LogScopeType.Independent, eventId))
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
		// Add the logger to the same a group (LoggerGroup.Event).
		_logger = logger.AddToGroup(LoggerGroup.Event);
		_eventHandlerHelper = eventHandlerHelper;
	}

	internal void HandleEvent()
	{
		// Even if the logger was not explicitly enriched with the event id,
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
		// Add the logger to the same a group (LoggerGroup.Event).
		_logger = logger.AddToGroup(LoggerGroup.Event);
	}

	internal void Process()
	{
		// Even if the logger was not explicitly enriched with the event id,
		// its generated output will contain it because of the group.
		_logger.LogInformation("Starting to help.");
	}
}
```



## Loggers

### LogScopeHandlingLogger

The `Phoenix.Functionality.Logging.Extensions.Microsoft.LogScopeHandlingLogger` is wrapper for any `ILogger` that uses an [**`ILogScopeManager`**](#Handling) (`LogScopeManager` by default) to store ambient scope and applies it to the log events emitted by the wrapped logger. It can be used like this:

> [!NOTE]
>
> The below example uses **Serilog** as the actual logger that is responsible for emitting the logs. The `LogScopeHandlingLogger` only requires an `Microsoft.Extensions.Logging.ILogger`, so any other major log library that is able to provide one, can be used instead.

```c#
// Create a Serilog logger.
var serilogLogger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

// Create a Microsoft.Extensions.Logging.ILogger that uses the Serilog logger as underlying logger.
var frameworkLogger = new SerilogLoggerProvider(serilogLogger).CreateLogger("MyLogger");

// Create a LogScopeHandlingLogger that uses the Framework logger as underlying logger.
var logger = new LogScopeHandlingLogger(frameworkLogger);
```



### NoLogger

> [!Caution]
> Consider using `Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance` instead.

The `Phoenix.Functionality.Logging.Extensions.Microsoft.NoLogger` is a simple null-object that can be accessed via the static `NoLogger.Instance` property and can be used to better implement nullable reference types.



### TraceLogger

> [!IMPORTANT]
> This logger does not support **log scopes** at all.

The `Phoenix.Functionality.Logging.Extensions.Microsoft.TraceLogger` is a simple **ILogger** implementation that writes its log events to **System.Diagnostics.Trace** and - if available - to the console output. It can be instantiated or directly used via the static `TraceLogger.Instance` property.
___

# Logging.Extensions.Microsoft.Autofac

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 8.0 :heavy_check_mark: 10.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

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

> [!TIP]
>
> This can be used to add the logger a service uses to a group.

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

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 8.0 :heavy_check_mark: 10.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

## General Information

This package contains different helper classes that can be used when logging via [**Serilog**](https://serilog.net).



## Settings

By using new extension methods of `LoggerSettingsConfiguration` creating a new `Serilog.LoggerConfiguration` and thus a new Serilog-**Logger** from a JSON file is pretty simple.

```csharp
// Get the configuration file.
var configurationFile = new FileInfo(Path.Combine("PATH_TO_CONFIGURATION", "serilog.config"));

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



## Enrichers

The package provides some **ILogEventEnricher** that help adding data to log events. More information about log enrichment in general can be found [here](https://github.com/serilog/serilog/wiki/Enrichment).

### `ApplicationInformationEnricher`

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

### `ApplicationIdentifierEnricher`

> [!WARNING]
> This is deprecated. Use `ApplicationInformationEnricher` instead.

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

### `ApplicationVersionEnricher`

> [!WARNING]
> This is deprecated. Use `ApplicationInformationEnricher` instead.

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
___

# Logging.Extensions.Serilog.File

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 8.0 :heavy_check_mark: 10.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

## `ArchiveHook`

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
___

# Logging.Extensions.Serilog.Microsoft

> [!CAUTION]
>
> ⚠️ **DEPRECATED**
>
> Instead use `SerilogLoggerProvider` from the official [**Serilog.Extensions.Logging**](https://github.com/serilog/serilog-extensions-logging) package together with the `LogScopeHandlingLogger` from the [**Phoenix.Functionality.Logging.Extensions.Microsoft**](#Logging.Extensions.Microsoft) package. How to set this up is described [here](#LogScopeHandlingLogger).

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 8.0 :heavy_check_mark: 10.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

~~This package provides an adapater for **Microsoft.Extensions.Logging.ILogger** named `FrameworkLogger`. It forwards log events to an underlying **Serilog.ILogger**. Most of the implementation is taken from the existing package [**Serilog.Extensions.Logging**](https://github.com/serilog/serilog-extensions-logging/) with one key difference: **Log scope handling**.~~

## ~~Log Scope Handling~~

~~**Serilog.Extensions.Logging** uses `System.Threading.AsyncLocal<T>` to store log scopes. Log scopes are therefore bound to the **execution context** they were created in. For use cases such as request/response services this is a good enough choice. For applications however this may be problematic. More about that can be found in the [introduction](#About-Logging).~~

~~The `FrameworkLogger` uses an internal class called `FrameworkLoggerScopes` for handling log scopes. It has two different collections where it stores scopes that have been created via `ILogger.BeginScope`:~~

- ~~A collection for general (independent) log scopes~~
- ~~A collection for execution context aware log scopes~~

~~The `FrameworkLoggerScopes` class distinguishes between those collections by inspecting the `LogScopeType` of the scope. The scope itself can be of any type since the `ILogger.BeginScope` uses a generic parameter for it. By default, every new log scope will be stored in the general collection. Only if the type implements the `IExecutionContextAwareLogScope` interface will the `FrameworkLoggerScopes` use the execution context aware collection to store a scope.~~

~~When using the **Phoenix.Functionality.Logging.Extensions.Microsoft** package there are two classes that inherit from `Dictionary<string, object?>` which can be used to specify if a log scope is general or execution context aware.~~

- ~~`LogScope`~~
- ~~`ExecutionContextAwareLogScope` (which implements `IExecutionContextAwareLogScope`)~~

> [!TIP]
> ~~Deciding if a log scope is generally available or strictly bound to an execution context is only a matter of using one of the those two classes.~~

~~Now back to the issue with **Serilog**s approach to always use execution context aware scopes: If important information that needs to be added to every log event is obtained by a long running background task, it is impossible to get that information through the bounds of the original execution context (which is the background task alone) to some other logger in a different execution context. Since the `FrameworkLogger` by default stores log scope in a simple collection, that scope can be passed to different loggers without issues.~~



## ~~Log Scope Sharing~~

~~Since the collections storing log scopes are instance members of the `FrameworkLoggerScopes` class and therefore each instance of a `FrameworkLogger` has its own separate log scope, controlling which logger shares the same scope is either about which loggers are the **same** instance or which loggers belong to the same [**logger group**](#Logger-groups).~~



## IoC (Autofac)

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
___

# Logging.Extensions.Serilog.Seq

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 8.0 :heavy_check_mark: 10.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

If using [**Seq**](https://datalust.co/seq) as a sink for **Serilog** it is good practice to use an separate **Api Key** for each application forwarding logs to the **Seq Server** so that authentication and filtering can be handled by the server. Normally those **Api Keys** are manually created via the web frontend of the **Seq Server** and then hard-coded into the application.

For some applications this may however not be feasible, e.g. if one application has many different installations each using a different configuration or feature set. In such cases it would be better to differentiate those instances from one another via different **Api Keys**. This package helps in creating and registering such **Api Keys** dynamically. To be able to use this feature, the following things are necessary:

- Configuration **Api Key**

	A separate **Api Key** has to be created in the **Seq Server** that is allowed to change the servers configuration. This **Api Key** is the one that will get hard-coded into the application, but won't be used to emit log messages. It is only used to dynamically create and retrieve other **Api Keys** for different application instances.

> [!IMPORTANT]
>
> The *admin* **Api Key** seems to need all available permission of the **Seq Server**:
>
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

## `SeqServer`

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

* **Felix Leistner**: _v1.x_ - _v3.x_