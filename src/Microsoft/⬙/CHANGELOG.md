# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.2.0

:calendar: _2023-03-25_

### Added

- New `ILogger` extension methods `PinScope` that create log scopes that are not removable and return the same `ILogger` instance for chaining. This can be used when initially setting up logger instances.

### Fixed

- Due to **automatic type inference** not working if the **generic type** parameter is inside a **ValueTuple**, calls to `CreateScopeAndLog` having a generic `LogScope<>` as parameter was invoking the wrong method. This cannot be prevented in a reasonable way by just changing method signatures. Therefor the falsely called method now checks if its log scope parameter is generic and then forwards the call to the correct method.

___

## 2.1.0

:calendar: _2023-03-16_

### Added

- New null-object logger `NoLogger.Instance`.
- New trace and console logger `TraceLogger`. It can be instantiated or used directly via the static `TraceLogger.Instance` property.

___

## 2.0.0

:calendar: _2022-12-01_

### Changed

- The `ILogger.CreateScope` extension method now supports **Expression** parameters that use enumerations (e.g. `logger.CreateScope(() => MyEnum.Something)`). In such cases the scope would use the name of the enumeration **MyEnum** and its value **Something** as pair.
- The **ILogger** extension methods used for logging are now all just named `ILogger.Log(...)` (instead of `ILogger.LogEvent(...)`).

### Deprecated

- Both `EventIdLogger` and `EventIdResourceLogger` are now obsolete. Their functionality has been moved to extension methods of **Microsoft.Extensions.Logging.ILogger**.
	- ~~`EventIdLogger.LogEvent`~~ → **`ILogger.Log`**
	- ~~`EventIdResourceLogger.LogEventFromResource`~~ → **`ILogger.Log`**

### Removed

- The accessible `Logger` property of the `EventIdLogger` class, that was previously marked as obsolete, has now been removed.
- The (now obsolete) `EventIdResourceLogger` does not have an internal cache anymore. It was removed in favor of the new extension methods and because
  - it would fail to adapt to dynamic changes to the application culture.
  - it was based on the uniqueness of event ids, which would fail if consuming code somehow had identical event ids for different messages.
  - it would not garbage collect event from dynamic loggers or loggers that are just used once throughout application lifetime.
- The `LogData` structure has been removed, as it was only used by the cache of `EventIdResourceLogger`.

### Fixed

-   The `ILogger.CreateScope` extension method using **CallerArgumentExpression** did not clean the caller argument and therefore produced values that differed from the overload that uses **Expression**s. For example `logger.CreateScope(_member.Property)` would produce a scope with the name **MemberProperty** as opposed to just **Property**. The old behavior can be restored by setting the new optional parameter `cleanCallerArgument` to false. **This fix is implemented as a breaking change**.

___

## 1.3.0

:calendar: _2022-01-09_

### Added

- Project now natively supports **.NET 6**.
- A new `ILogger.CreateScope` overload utilizing [**CallerArgumentExpression**](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute?view=net-6.0) has been added. This requires at least **.NET Core 3.0** as target framework.

### References

:large_blue_circle: Microsoft.Extensions.Logging ~~5.0.0~~ → **6.0.0**
___

## 1.2.0

:calendar: _2021-11-25_

### Fixed

- Getting all loggers belonging to a group via the `AsGroup` extension method, threw an **ArgumentNullException** if the logger invoking the method did not belong to any group. Reason was invalid handling of a **System.Value.Tuple** returned inside a collection.
- Using **value types** as group identifier led to not being able to address the group via the identifier, because all identifiers where stored as **objects** which automatically boxed the **value types** into **reference types**. When trying to get the groups via such an identifier, the equality comparison then failed because the even identical **value types** will get different references when being boxed.

### Deprecated

- The `EventIdLogger.Logger` property has been marked obsolete, because interacting with this internal `ILogger` can cause problems. For example adding the internal logger to a group, but later trying to get other loggers belonging to that group via the `EventIdLogger` would fail. Since every `EventIdLogger` is itself an `ILogger`, it should be used instead.
___

## 1.1.0

:calendar: _2021-11-01_

### Added

- Each `EventIdLogger` or `EventIdResourceLogger` can now be named via new constructor parameters. This name is added as scope property with the default property name **Context**.
___

## 1.0.0

:calendar: _2021-10-15_

Initial release.