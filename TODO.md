# TODO

All planned changes to this project will be documented in this file.
___

## Functionality

## Microsoft

- [ ] Create extension method for capturing execution time in a scope.

- [ ] Create a custom **ILogger** that allows to capture all log output and maybe react on them based on events or simply forward them to handler functions. This would be needed for further unit tests. [Here](https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider) is how to implement the necessary interfaces.

- [ ] Implement a new pattern for exception logging according to [this](https://blog.stephencleary.com/2020/06/a-new-pattern-for-exception-logging.html)

- [x] ~~If resolving a message from a resource failed, add a hint to the error message that suggests to check if all required **RessourceManagers** have been passed to the parent `EventIdResourceLogger`.~~

- [x] ~~Add an example to the documentation on how to create a custom `EventIdResourceLogger` that shows to pass required **RessourceManagers** to the parent.~~

- [x] ~~Add a timeout when initially connecting to a **Seq Server**. If the specified server is not available, this may otherwise lead to long application startup times.~~

- [ ] Scope-Chaining: When creating scopes (via different extension methods) it would be beneficial to be able to chain the creation process together so that at the end only a single scope is created:

	```c#
	Microsoft.Extensions.Logging.ILogger logger = ...;
	var user = "John Doe";
	var action = "Delete";
	var strangeVariableName = DateTime.Now;
	
	// This would not work. The `strangeVariableName` would not match the {Date} placeholder.
	using (logger.CreateScope(user, action, strangeVariableName))
	{
	    logger.LogInformation("User {User} triggered {Action} at {Date}.");
	}
	
	// Therefore the declaration of the scope would need to be explicitly.
	using (logger.CreateScope(("User", user), ("Action", action), ("Date", strangeVariableName)))
	{
	    logger.LogInformation("User {User} triggered {Action} at {Date}.");
	}
	
	// For such cases chained scope definition could be used.
	```

	

## Serilog

- [x] ~~Create a script or a console application for importing log files to a **Seq Server**.~~

	This can be accomplished via the `SendLogFile` or `SendLogFileAsync` methods of the `SeqServer` class

___

## Unit Tests

## Microsoft

- [x] ~~The `EventIdLogger` is not yet tested due to not being able to intercept/redirect log output.~~

	`EventIdLogger` has been deprecated already.

## Serilog

- [ ] ...