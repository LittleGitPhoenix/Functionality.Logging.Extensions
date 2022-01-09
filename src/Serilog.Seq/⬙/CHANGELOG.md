# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.3.0 (2022-01-09)

### Added

- Project now natively supports **.NET 6**.
- Added option to specify the internal log level for errors via the new `SeqSinkErrorLogLevel`enumeration that gets passed to the `LoggerSinkConfiguration.Seq` extension method.

### Changed

- Updated the way the internal **IBatchedLogEventSink** class is obtained from the **Serilog.Sinks.Seq** package. Previously this interface was implemented by **Serilog.Sinks.Seq.SeqSink** but now the new class **Serilog.Sinks.Seq.Batched.BatchedSeqSink** took its place.

### References

:large_blue_circle: Serilog.Sinks.Seq ~~5.0.1~~ → **5.1.0**
___

## 1.2.0 (2021-11-27)

### Added

- Added `SeqServerConnectionData` entity, that can be used to create `SeqServer` instances.
- Added `SeqServerApplicationInformation` entity, that can be used to register an application with a **Seq Server** via the `SeqServer` class.

### Fixed

- When sending a log file to a **Seq Server**, that file will now be opened for read with shared access. This may allow to send log files, that are currently in use.
___

## 1.1.0 (2021-10-18)

### Updated

- The `SeqBufferSink` now always creates a new thread when trying to register the application with the **Seq Server**. This is because the previous handling blocked the calling thread for the first register attempt.

### Fixed

- A **TaskCanceledException** that could only be seen via the **TaskScheduler.UnobservedTaskException** event handler, was thrown in `SeqServer.RegisterApplication` called by the `SeqBufferSink` when trying to register an application with a **Seq Server**. The reason was the register method using **Wait()** on its asynchronous counterpart, passing in a real **CancellationToken**.
___

## 1.0.0 (2021-10-15)

Initial release.
