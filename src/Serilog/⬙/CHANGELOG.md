# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.4.0

:calendar: _2023-05-??_

### Added

- The new `ApplicationInformationEnricher` that allows logs to be enriched by a selectable information about an application like **name**, **version**, etc.

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