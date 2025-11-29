# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.4.0

:calendar: _2025-12-??_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: ~~6~~ :heavy_check_mark: 8 :new: 10 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### Changed

- The `FrameworkLogger` has been updated to use implementations from the `Serilog.Extensions.Logging.SerilogLogger` tag **v9.0.2**.

### Removed

- Support for **.NET 6** has been removed since that framework is officially deprecated.
- The `FrameworkLogger` no longer uses an internal collection of low event id `LogEventProperty`s to optimize logging of those. This optimization was part of the original `LogEventProperty` used as template. Now all already processed event ids are stored in a shared cache and will be reused when the same event id is logged again.

### References

:white_circle: Microsoft.Extensions.Logging.Abstractions **10.0.0**  
:large_blue_circle: Phoenix.Functionality.Logging.Base ~~1.1.0~~ → [**1.2.0**](../../Logging.Base/⬙/CHANGELOG.md#1.2.0)  
:large_blue_circle: Serilog ~~3.0.0~~ → **4.3.0**  
:red_circle: ~~Microsoft.Extensions.Logging~~  
___

## 1.3.0

:calendar: _2024-03-11_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 6.0 :new: 8.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### References

:large_blue_circle: Phoenix.Functionality.Logging.Base ~~1.0.0~~ → [**1.1.0**](../../Logging.Base/⬙/CHANGELOG.md#1.1.0)  
:large_blue_circle: Serilog ~~2.10.0~~ → **3.0.0**  
:large_blue_circle: Microsoft.Extensions.Logging ~~6.0.0~~ → **8.0.0**  
___

## 1.2.0

:calendar: _2023-06-08_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :new: 6.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### Added

- The log level converter `SerilogToMicrosoftLogLevelConverter` now implements the `Phoenix.Functionality.Logging.Base.ILogLevelConverter<TSourceLogLevel, TTargetLogLevel>` interface and is public.

### References

:white_circle: Phoenix.Functionality.Logging.Base **1.0.0**
___

## 1.1.0

:calendar: _2022-01-09_

### Added

- Project now natively supports **.NET 6**.

### References

:large_blue_circle: Microsoft.Extensions.Logging ~~5.0.0~~ → **6.0.0**  
___

## 1.0.0

:calendar: _2021-11-01_

Initial release.