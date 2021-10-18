# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.1.0 (2021-10-18)

### Updated

- The `SeqBufferSink` now always creates a new thread when trying to register the application with the **Seq Server**. This is because the previous handling blocked the calling thread for the first register attempt.

### Fixed

- A **TaskCanceledException** that could only be seen via the **TaskScheduler.UnobservedTaskException** event handler, was thrown in `SeqServer.RegisterApplication` called by the `SeqBufferSink` when trying to register an application with a **Seq Server**. The reason was the register method using **Wait()** on its asynchronous counterpart, passing in a real **CancellationToken**.

___

## 1.0.0 (2021-10-15)

Initial release.

