# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.3.0

:calendar: _2024-03-11_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_check_mark: 6.0 :new: 8.0 | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

### References

:large_blue_circle: Phoenix.Functionality.Logging.Base ~~1.0.0~~ → [**1.1.0**](../../Logging.Base/⬙/CHANGELOG.md#1.1.0)
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