# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.3.0 (2022-01-09)

### Added

- Project now natively supports **.NET 6**.
- A new `ILogger.CreateScope` overload utilizing [**CallerArgumentExpression**](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute?view=net-6.0) has been added. This requires at least **.NET Core 3.0** as target framework.

### References

:large_blue_circle: Microsoft.Extensions.Logging ~~5.0.0~~ → **6.0.0**
___

## 1.2.0 (2021-11-25)

### Fixed

- Getting all loggers belonging to a group via the `AsGroup` extension method, threw an **ArgumentNullException** if the logger invoking the method did not belong to any group. Reason was invalid handling of a **System.Value.Tuple** returned inside a collection.
- Using **value types** as group identifier led to not being able to address the group via the identifier, because all identifiers where stored as **objects** which automatically boxed the **value types** into **reference types**. When trying to get the groups via such an identifier, the equality comparison then failed because the even identical **value types** will get different references when being boxed.

### Deprecated

- The `EventIdLogger.Logger` property has been marked obsolete, because interacting with this internal `ILogger` can cause problems. For example adding the internal logger to a group, but later trying to get other loggers belonging to that group via the `EventIdLogger` would fail. Since every `EventIdLogger` is itself an `ILogger`, it should be used instead.
___

## 1.1.0 (2021-11-01)

### Added

- Each `EventIdLogger` or `EventIdResourceLogger` can now be named via new constructor parameters. This name is added as scope property with the default property name **Context**.
___

## 1.0.0 (2021-10-15)

Initial release.