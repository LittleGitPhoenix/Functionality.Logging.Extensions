# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.0.1

:calendar: _2023-08-??_

### Fixed

- Made overloads of `SeqServer.RegisterApplication(Async)` public, that where previously already accessible. The original functions where removed during restructuring for `LogApplicationInformation` and their replacements where private.

___

## 2.0.0

:calendar: _2023-06-08_

|   .NET Framework   |     .NET Standard      |                     .NET                      |
| :----------------: | :--------------------: | :-------------------------------------------: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_check_mark: 6.0 |

### Added

- The `SeqServer` now fully uses the new base assembly **Phoenix.Functionality.Logging.Base** and its `LogApplicationInformation` class.

### Removed

- `SeqServerApplicationInformation` (along with its `SeqServerApplicationInformationBuilder`) has been removed, because the new base package **Phoenix.Functionality.Logging.Base** provides a superior implementation named `LogApplicationInformation`.
- All functions in `SeqServer` that utilized `SeqServerApplicationInformation` have been replaced by matching counterparts now using `LogApplicationInformation`. This is the reason, why this is a **breaking change**. It would have been possible to keep the old function signatures, but that would have made the `SeqServer` class too confusing. This decision makes a clean cut. 

### References

:white_circle: Phoenix.Functionality.Logging.Base **1.0.0**
:large_blue_circle: Seq.Api ~~2022.1.0~~ → **2023.1.0**
___

## 1.4.0

:calendar: _2022-12-30_

### Added

- `SeqServerApplicationInformation` can (and should) now be created via builder pattern starting from `SeqServerApplicationInformation.Create()...`.

### Fixed

- Previous version where unable to operate with **Seq Server** version **2022.x** due to API changes. This has been addressed by updating the **Seq.Api** nuget package.

### Deprecated

- `SeqServerApplicationInformation.CreateWithMachineName` has been marked obsolete, as it is replaced by the new `SeqServerApplicationInformationBuilder`.

### References

:large_blue_circle: Seq.Api ~~2021.3.0~~ → **2022.1.0**

___

## 1.3.0

:calendar: _2022-01-09_

### Added

- Project now natively supports **.NET 6**.
- Added option to specify the internal log level for errors via the new `SeqSinkErrorLogLevel`enumeration that gets passed to the `LoggerSinkConfiguration.Seq` extension method.

### Changed

- Updated the way the internal **IBatchedLogEventSink** class is obtained from the **Serilog.Sinks.Seq** package. Previously this interface was implemented by **Serilog.Sinks.Seq.SeqSink** but now the new class **Serilog.Sinks.Seq.Batched.BatchedSeqSink** took its place.

### References

:large_blue_circle: Serilog.Sinks.Seq ~~5.0.1~~ → **5.1.0**
___

## 1.2.0

:calendar: _2021-11-27_

### Added

- Added `SeqServerConnectionData` entity, that can be used to create `SeqServer` instances.
- Added `SeqServerApplicationInformation` entity, that can be used to register an application with a **Seq Server** via the `SeqServer` class.

### Fixed

- When sending a log file to a **Seq Server**, that file will now be opened for read with shared access. This may allow to send log files, that are currently in use.
___

## 1.1.0

:calendar: _2021-10-18_

### Updated

- The `SeqBufferSink` now always creates a new thread when trying to register the application with the **Seq Server**. This is because the previous handling blocked the calling thread for the first register attempt.

### Fixed

- A **TaskCanceledException** that could only be seen via the **TaskScheduler.UnobservedTaskException** event handler, was thrown in `SeqServer.RegisterApplication` called by the `SeqBufferSink` when trying to register an application with a **Seq Server**. The reason was the register method using **Wait()** on its asynchronous counterpart, passing in a real **CancellationToken**.
___

## 1.0.0

:calendar: _2021-10-15_

Initial release.