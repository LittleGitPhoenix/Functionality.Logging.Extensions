# TODO

All planned changes to this project will be documented in this file.
___

## Functionality

## Microsoft

- [ ] Create extension method for capturing execution time in a scope.
- [ ] Create a custom **ILogger** that allows to capture all log output and maybe react on them based on events or simply forward them to handler functions. This would be needed for further unit tests. [Here](https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider) is how to implement the necessary interfaces.

## Serilog

- [ ] Create a script or a console application for importing log files to a **Seq Server**.

___

## Unit Tests

## Microsoft

- [ ] The `EventIdLogger` is not jet tested due to not being able to intercept/redirect log output.

## Serilog

- [ ] ...