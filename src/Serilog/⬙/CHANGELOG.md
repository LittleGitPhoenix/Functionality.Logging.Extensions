# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.5.0

:calendar: _2024-03-11_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 6.0 :new: 8.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### References

:large_blue_circle: Phoenix.Functionality.Logging.Base ~~1.0.0~~ → [**1.1.0**](../../Logging.Base/⬙/CHANGELOG.md#1.1.0)
:large_blue_circle: Serilog.Settings.Configuration ~~3.3.0~~ → **8.0.0**
:large_blue_circle: Microsoft.Extensions.Configuration.Json ~~6.0.0~~ → **8.0.0**

___

## 1.4.0

:calendar: _2023-06-08_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :new: 6.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### Added

- The new `ApplicationInformationEnricher` that allows logs to be enriched by a selectable information about an application like **name**, **version**, etc.
- Added `SerilogLogLevelConverter`, an `INoLogLevelConverter` for **Serilog.Events.LogEventLevel**.

### Deprecated

- `ApplicationVersionEnricher` and `ApplicationIdentifierEnricher` (both are replaced by the new `ApplicationInformationEnricher`).

### References

:white_circle: Phoenix.Functionality.Logging.Base **1.0.0**
___

## 1.3.0

:calendar: _2023-03-27_

### Added

- The new `ApplicationVersionEnricher` that allows logs to be enriched by a selectable application version.
___

## 1.2.0

:calendar: _2022-01-09_

### Added

- Project now natively supports **.NET 6**.

### References

:large_blue_circle: Microsoft.Extensions.Configuration.Json ~~5.0.0~~ → **6.0.0**
___

## 1.1.0

:calendar: _2021-11-27_

### Changed

- If the working directory differs from the applications base directory, then the serilog configuration file will be copied from the application directory to the working directory (if the file not already exists). This helps with isolating different configuration based on the working directory of an application.
___

## 1.0.0

:calendar: _2021-10-15_

Initial release.