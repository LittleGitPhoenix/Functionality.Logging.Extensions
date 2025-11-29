#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

class NoDisposable : IDisposable
{
	public static NoDisposable Instance => Lazy.Value;
	private static readonly Lazy<NoDisposable> Lazy = new Lazy<NoDisposable>(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);
	
	private NoDisposable() { }

	/// <inheritdoc />
	public void Dispose() { }
}

static class Example
{
	//# Unit Test: Check that multiple Enrich calls can be chained and that they all are properly disposed.
	//# Unit Test: Check that the actual logger of a ChainingLogScopeDisposable is always the initial logger even if multiple nested instance are used.
	//# Unit Test: Check that Log uses the actual logger in case of a ChainingLogScopeDisposable but still returns the chanined one.
	//# Unit Test: Check missmatch in args supplied to Log.

	/*
	[LoggerMessage(LogLevel.Information, EventId = 2134430538, Message = "Database connection is to {DatabaseName}@{DatabaseServer}.")]
	public static partial void DatabaseInformation(ILogger logger, string databaseName, string databaseServer);

	internal static readonly Action<ILogger, string, string, Exception?> ProcessingStarted =
	LoggerMessage.Define<string, string>(LogLevel.Debug, 38984401, "Start handling request with identifier {RequestIdentifier} for {Request}.");
	*/

	internal static void ExampleChain()
	{
		// Example how to use the Enrich method.
		ILogger logger = null!;
		var scope = new LogScope
		(
			("UserId", 12345),
			("SessionId", "abcde-67890-fghij-12345")
		);
		var logEvent = new LogEvent(1178801003, LogLevel.Information, "User logged in.");

		//var disposable = (IChainingLogScopeDisposable) logger.Enrich(scope).Enrich(scope).Log(logEvent);
		using (logger.Enrich(scope).Enrich(scope).Log(logEvent).Use())
		{
			// ...
		}
	}
}

/// <summary>
/// Provides extension methods for <see cref="ILogger"/>.
/// </summary>
public static partial class LoggerExtensions
{
	#region Logging

	/// <summary>
	/// Emits the given <paramref name="logEvent"/>  to the <paramref name="logger"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/> used to emit the event. If this is <see cref="NoLogger.Instance"/> or <see cref="global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance"/> nothing will be done. </param>
	/// <param name="logEvent"> The <see cref="ILogEvent"/> to log. If this is <see langword="null"/> or <see cref="LogEvent.NoLogEvent"/> nothing will be done. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if <paramref name="logger"/> is <see langword="null"/>. </exception>
	public static ILogger Log(this ILogger logger, ILogEvent? logEvent)
		=> Log(logEvent, logger);

	/// <summary>
	/// Emits the given <paramref name="logEvents"/>  to the <paramref name="logger"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/> used to emit the event. If this is <see langword="null"/>, <see cref="NoLogger.Instance"/> or <see cref="global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance"/> nothing will be done. </param>
	/// <param name="logEvents"> The collection of <see cref="ILogEvent"/>s to log. Each event that is <see langword="null"/> or <see cref="LogEvent.NoLogEvent"/> will be ignored. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger Log(this ILogger logger, IEnumerable<ILogEvent> logEvents)
	{		
		foreach (var logEvent in logEvents) Log(logEvent, logger);
		return logger;
	}

	/// <summary>
	/// Logs an event with a given <paramref name="logMessage"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="logMessage"> The message to log. </param>
	/// <param name="args"> Arguments passed to the log message. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger Log(this ILogger logger, int eventId, LogLevel logLevel, string logMessage, params object?[] args)
		=> Log(new LogEvent(eventId, logLevel, logMessage, args), logger);

	/// <summary>
	/// Logs an event with a given <paramref name="logMessage"/> and <paramref name="exception"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="exception"> The <see cref="Exception"/> to log. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="logMessage"> The message to log. </param>
	/// <param name="args"> Arguments passed to the log message. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger Log(this ILogger logger, int eventId, Exception exception, LogLevel logLevel, string logMessage, params object?[] args)
		=> Log(new LogEvent(eventId, exception, logLevel, logMessage, args), logger);

	/// <summary>
	/// Resolves the log message from resource files and then emits it to the <paramref name="logger"/> and returns the message that was translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/> used to emit the event. If this is <see langword="null"/>, <see cref="NoLogger.Instance"/> or <see cref="global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance"/> nothing will be done and an empty string will be returned. </param>
	/// <param name="logEvent"> The <see cref="ILogResourceEvent"/>. If this is <see langword="null"/> or <see cref="LogResourceEvent.NoLogResourceEvent"/> nothing will be done and an empty string will be returned. </param>
	/// <returns> The translated log message or an empty string if <paramref name="logEvent"/> is <see langword="null"/> or <see cref="LogEvent.NoLogEvent"/>. </returns>
	public static string Log(this ILogger logger, ILogResourceEvent logEvent)
		=> LogEventFromResource(logEvent, logger);

	/// <summary>
	/// Logs an event with a message resolved from a resource file and returns this message translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="resourceManager"> The <see cref="ResourceManager"/>s that is queried to find the proper messages for <paramref name="resourceName"/>. </param>
	/// <param name="resourceName"> The name of the resource that is the log message. </param>
	/// <param name="args"> Optional arguments passed to the log message. Those arguments are directly passed to the underlying logger instance. </param>
	/// <param name="outputArgs"> Optional arguments merged into the returned output message via <see cref="String.Format(string,object?[])"/>. If this is omitted, then <paramref name="args"/> will be used. </param>
	/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. Default value is <see cref="LogResourceEvent.LogCulture"/>. </param>
	/// <returns> The translated log message. </returns>
	public static string Log(this ILogger logger, int eventId, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
		=> LogEventFromResource(new LogResourceEvent(eventId, logLevel, resourceManager, resourceName, args, outputArgs, logCulture), logger);

	/// <summary>
	/// Logs an event with a message resolved from a resource file together with an <paramref name="exception"/> and returns this message translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="exception"> The <see cref="Exception"/> to log. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="resourceManager"> The <see cref="ResourceManager"/>s that is queried to find the proper messages for <paramref name="resourceName"/>. </param>
	/// <param name="resourceName"> The name of the resource that is the log message. </param>
	/// <param name="args"> Optional arguments passed to the log message. Those arguments are directly passed to the underlying logger instance. </param>
	/// <param name="outputArgs"> Optional arguments merged into the returned output message via <see cref="String.Format(string,object?[])"/>. If this is omitted, then <paramref name="args"/> will be used. </param>
	/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. Default value is <see cref="LogResourceEvent.LogCulture"/>. </param>
	/// <returns> The translated log message. </returns>
	public static string Log(this ILogger logger, int eventId, Exception exception, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? args = null, object?[]? outputArgs = null, CultureInfo? logCulture = null)
		=> LogEventFromResource(new LogResourceEvent(eventId, exception, logLevel, resourceManager, resourceName, args, outputArgs, logCulture), logger);

	#region Helper

	/// <summary>
	/// Logs messages while catching format exceptions.
	/// </summary>
	/// <param name="logEvent"> The <see cref="ILogEvent"/> to log. </param>
	/// <param name="logger"> The <see cref="ILogger"/> used to emit the event. If this is <see cref="NoLogger.Instance"/> or <see cref="global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance"/> nothing will be done. </param>
	/// <exception cref="ArgumentNullException"> Is thrown if <paramref name="logger"/> is <see langword="null"/>. </exception>
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static ILogger Log(ILogEvent? logEvent, ILogger logger)
	{
		if (logger is null) throw new ArgumentNullException(nameof(logger));
		if (logger == NoLogger.Instance || logger == global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance) return logger;
		if (logEvent is null || logEvent == LogEvent.NoLogEvent) return logger;

		//* If the logger is wrapped within a chaining logger, always use the inner logger. This way the actual logger is always used for logging.
		//* This improves performance and using the actual logger allows type checking further down the log chain.
		//* Important: Alwayys(!) return the original logger as to not switch instances.
		var actualLogger = logger as ChainingLogScopeDisposable ?? logger;
		
		// Denconstruct the event and log it.
		var (eventId, exception, logLevel, logMessage, args, payload) = logEvent;

		// Fast level check before deconstruction & scope creation to avoid overhead.
		if (!actualLogger.IsEnabled(logLevel)) return logger;
		
		// Create scope from payload.
		using var payloadScope = actualLogger.Enrich(payload);

		try
		{
			actualLogger.Log(logLevel, eventId, exception, logMessage, args);
		}
		catch (AggregateException ex) when (ex.Flatten().InnerExceptions.Select(e => e.GetType()).Contains(typeof(IndexOutOfRangeException)))
		{
			var arguments = args.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", args);
			actualLogger.Log(logLevel, eventId, exception, $"Could not format the message '{logMessage.Replace("{", "{{").Replace("}", "}}")}' because of a mismatch with the supplied arguments {arguments}.", args);
		}
		catch (Exception)
		{
			var arguments = args.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", args);
			System.Diagnostics.Debug.WriteLine($"Could not write log for the message '{logMessage}' with arguments '{arguments}'.");
		}

		return logger;
	}

	/// <summary>
	/// Logs an event with a message resolved from a resource file and returns this message translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logEvent"> The <see cref="ILogResourceEvent"/> to log. </param>
	/// <param name="logger"> The <see cref="ILogger"/> to use. </param>
	/// <returns> The translated log message or an empty string if <paramref name="logEvent"/> is <see langword="null"/> or <see cref="LogEvent.NoLogEvent"/>. </returns>
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	private static string LogEventFromResource(ILogResourceEvent? logEvent, ILogger logger)
	{		
		if (logger is null || logger == NoLogger.Instance || logger == global::Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance) return String.Empty;
		if (logEvent is null || logEvent == LogResourceEvent.NoLogResourceEvent) return String.Empty;

		// Log
		Log(logEvent, logger);

		// Return the translated message.
		return logEvent.OutputMessage;
	}

	#endregion

	#endregion

	#region Groups

	//# The LoggerGroup must be made into a regualr ILogger too.

	/// <summary>
	/// Adds the <paramref name="logger"/> to the group identified by <paramref name="groupIdentifier"/>.
	/// </summary>
	/// <typeparam name="TIdentifier"> The type of the <paramref name="groupIdentifier"/>. </typeparam>
	/// <param name="logger"> The <see cref="ILogger"/> to add. </param>
	/// <param name="groupIdentifier"> The group identifier used when adding. </param>
	/// <param name="applyExistingScope"> Should existing scopes be applied tho the <paramref name="logger"/>. Default is <see langword="true"/>. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger AddToGroup<TIdentifier>(this ILogger logger, TIdentifier groupIdentifier, bool applyExistingScope = true)
		where TIdentifier : notnull
		=> LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier, applyExistingScope);

	/// <summary>
	/// Adds the <paramref name="logger"/> to all groups identified by <paramref name="groupIdentifiers"/>.
	/// </summary>
	/// <typeparam name="TIdentifier"> The type of the <paramref name="groupIdentifiers"/>. </typeparam>
	/// <param name="logger"> The <see cref="ILogger"/> to add. </param>
	/// <param name="applyExistingScope"> Should existing scopes be applied tho the <paramref name="logger"/>. Default is <see langword="true"/>. </param>
	/// <param name="groupIdentifiers"> A collection of group identifiers. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger AddToGroups<TIdentifier>(this ILogger logger, bool applyExistingScope = true, params TIdentifier[] groupIdentifiers)
		where TIdentifier : notnull
	{
		foreach (var groupIdentifier in groupIdentifiers) logger.AddToGroup(groupIdentifier, applyExistingScope);
		return logger;
	}

	/// <summary>
	/// Removes the <paramref name="logger"/> from the group identified by <paramref name="groupIdentifier"/>.
	/// </summary>
	/// <typeparam name="TIdentifier"> The type of the <paramref name="groupIdentifier"/>. </typeparam>
	/// <param name="logger"> The <see cref="ILogger"/> to remove. </param>
	/// <param name="groupIdentifier"> The group identifier used when removing. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger RemoveFromGroup<TIdentifier>(this ILogger logger, TIdentifier groupIdentifier)
		where TIdentifier : notnull
		=> LoggerGroupManager.RemoveLoggerFromGroup(logger, groupIdentifier);

	/// <summary>
	/// Removes the <paramref name="logger"/> from all its groups.
	/// </summary>
	/// <param name="logger"> The <see cref="ILogger"/> to remove. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger RemoveFromAllGroups(this ILogger logger)
		=> LoggerGroupManager.RemoveFromAllGroups(logger);

	/// <summary>
	/// Return all groups that the <paramref name="logger"/> is a part of.
	/// </summary>
	/// <param name="logger"> The <see cref="ILogger"/> whose groups to get.. </param>
	/// <returns> A collection of groups, where the <paramref name="logger"/> is a part of. </returns>
	public static IReadOnlyCollection<(object GroupIdentifier, ILoggerGroup LoggerGroup)> GetGroups(this ILogger logger)
		=> LoggerGroupManager.GetGroupsOfLogger(logger);

	/// <summary>
	/// Returns the <see cref="ILoggerGroup"/> for <paramref name="groupIdentifier"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/> whose groups to get. </param>
	/// <param name="groupIdentifier"> The group identifier of used to obtain the grouped loggers. </param>
	/// <returns> The <see cref="ILoggerGroup"/> containing the grouped loggers or an empty group. </returns>
	public static ILoggerGroup AsGroup<TIdentifier>(this ILogger logger, TIdentifier groupIdentifier)
		where TIdentifier : notnull
		=> LoggerGroupManager.GetGroup(groupIdentifier);

	#endregion

	#region Scoping

	/// <summary>
	/// Creates a new logging scope.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="logScope"> The <see cref="ILogScope"/> that enriches the logger. If this is <see langword="null"/> than a no-op <see cref="IChainingLogScopeDisposable"/> is returned. </param>
	/// <returns> An <see cref="IChainingLogScopeDisposable"/> that can be used either in a using statement or to further use the logger. </returns>
	public static IChainingLogScopeDisposable Enrich(this ILogger logger, ILogScope? logScope)
	{
		return logScope is null
			? new ChainingLogScopeDisposable(logger)
			: new ChainingLogScopeDisposable(logger, logger.BeginScope((IDictionary<string, object?>) logScope)!)
			?? new ChainingLogScopeDisposable(logger)
			;
	}

	/// <summary>
	/// Creates a new logging <paramref name="scope"/> that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger EnrichPermantently(this ILogger logger, LogScope? scope)
		=> logger.Enrich(scope);

	/// <summary>
	/// This is a special helper method that can be used at the end of a logging chain to cast the <see cref="ILogger"/> back into an <see cref="IChainingLogScopeDisposable"/>.
	/// </summary>
	/// <remarks>
	/// In a logging chain that first creates scopes via the <see cref="Enrich(ILogger,ILogScope)"/> method and then logs events via the <see cref="Log(ILogger, ILogEvent)"/> method,
	/// this method can be used at the end of the chain to obtain an <see cref="IChainingLogScopeDisposable"/> that can be disposed to end all created scopes.
	/// This is necessary as the last call to <see cref="Log(ILogger, ILogEvent)"/> returns an <see cref="ILogger"/>, which does not support scope disposal.
	/// <para />
	/// If the <paramref name="logger"/> is not an <see cref="IChainingLogScopeDisposable"/>, then a new <see cref="IChainingLogScopeDisposable"/> will be created. This case is not the intended use case, but it is supported to anyway.
	/// </remarks>
	/// <param name="logger"> The logger instance that should be cast back into an <see cref="IChainingLogScopeDisposable"/>. Cannot be <see langword="null"/>. </param>
	/// <returns> An <see cref="IChainingLogScopeDisposable"/> that represents the created log scope. The returned object should be disposed to end the scope. </returns>
	/// <example>
	/// <code>
	/// ILogger logger = ...;
	/// var scope1 = ...;
	/// var scope2 = ...;
	/// var logEvent = ...;
	/// using var disposable = logger.Enrich(scope1).Enrich(scope2).Log(logEvent).Use();
	/// ...
	/// </code>
	/// </example>
	public static IChainingLogScopeDisposable Use(this ILogger logger)
	{
		return logger is IChainingLogScopeDisposable chainingLogger
			? chainingLogger
			: new ChainingLogScopeDisposable(logger)
			;
	}

#if NETCOREAPP3_0_OR_GREATER
	/// <summary>
	/// Creates a new logging scope with named values extracted from the given parameters.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained even though its value is specified. </exception>
	/// <remarks> This method exists only because creating an implicit or explicit conversion operator in <see cref="LogScope"/> that has those parameters is not possible. </remarks>
	public static IChainingLogScopeDisposable Enrich
	(
		this ILogger logger,
		object? value1,
		object? value2 = default,
		object? value3 = default,
		object? value4 = default,
		object? value5 = default,
		object? value6 = default,
		object? value7 = default,
		object? value8 = default,
		object? value9 = default,
		object? value10 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default,
		bool cleanCallerArgument = true
	)
	{
		var scopes = new LogScope
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		);
		return logger.Enrich(scopes);
	}

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given parameters that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained even though its value is specified. </exception>
	/// <remarks> This method exists only because creating an implicit or explicit conversion operator in <see cref="LogScope"/> that has those parameters is not possible. </remarks>
	public static ILogger EnrichPermantently
	(
		this ILogger logger,
		object? value1,
		object? value2 = default,
		object? value3 = default,
		object? value4 = default,
		object? value5 = default,
		object? value6 = default,
		object? value7 = default,
		object? value8 = default,
		object? value9 = default,
		object? value10 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default,
		bool cleanCallerArgument = true
	)
	{
		var scopes = new LogScope
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		);
		return logger.Enrich(scopes);
	}
#endif

	#endregion

	#region Obsoletes

	/// <summary>
	/// Creates a new logging <paramref name="scope"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Use {nameof(Enrich)} instead.")]
	public static IDisposable CreateScope(this ILogger logger, LogScope? scope)
		=> logger.Enrich(scope);

	/// <summary>
	/// Creates a new logging <paramref name="scope"/> for a log group.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The logging scope. </returns>
	/// <remarks> It may be better to get the groups of a logger with the <see cref="AsGroup{TIdentifier}"/> extension method and then applying the scope with one of the group methods like <see cref="ILoggerGroup.CreateScope(LogScope)"/>. </remarks>
	[Obsolete($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.", true)]
	public static IDisposable CreateScope<TIdentifier>(this ILogger logger, LogScope<TIdentifier>? scope)
		where TIdentifier : notnull
		=> throw new NotSupportedException($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.");

	/// <summary>
	/// Creates a new logging scope with named values.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> Collection of named values. </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Use {nameof(Enrich)} instead.")]
	public static IDisposable CreateScope(this ILogger logger, params (string Identifier, object? Value)[] scopedValues)
		=> logger.Enrich(new LogScope(scopedValues));

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="System.Linq.Expressions.Expression"/>s.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> The <see cref="System.Linq.Expressions.Expression"/>s used to build the named values. </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Use {nameof(Enrich)} instead.")]
	public static IDisposable CreateScope(this ILogger logger, params System.Linq.Expressions.Expression<Func<object>>[] scopedValues)
		=> logger.Enrich(new LogScope(scopedValues));

#if NETCOREAPP3_0_OR_GREATER

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given parameters.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained even though its value is specified. </exception>
	[Obsolete($"Use {nameof(Enrich)} instead.")]
	public static IDisposable CreateScope
	(
		this ILogger logger,
		object? value1,
		object? value2 = default,
		object? value3 = default,
		object? value4 = default,
		object? value5 = default,
		object? value6 = default,
		object? value7 = default,
		object? value8 = default,
		object? value9 = default,
		object? value10 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default,
		bool cleanCallerArgument = true
	)
		=> logger.Enrich
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		);
#endif

	/// <summary>
	/// Creates a new logging <paramref name="scope"/> that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	[Obsolete($"Use {nameof(EnrichPermantently)} instead.")]
	public static ILogger PinScope(this ILogger logger, LogScope? scope)
		=> logger.EnrichPermantently(scope);

	/// <summary>
	/// Creates a new logging scope with named values that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> Collection of named values. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	[Obsolete($"Use {nameof(EnrichPermantently)} instead.")]
	public static ILogger PinScope(this ILogger logger, params (string Identifier, object? Value)[] scopedValues)
	{
		var scopes = new LogScope(scopedValues);
		return logger.EnrichPermantently(scopes);
	}

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="System.Linq.Expressions.Expression"/>s that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> The <see cref="System.Linq.Expressions.Expression"/>s used to build the named values. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	[Obsolete($"Use {nameof(EnrichPermantently)} instead.")]
	public static ILogger PinScope(this ILogger logger, params System.Linq.Expressions.Expression<Func<object>>[] scopedValues)
	{
		var scopes = new LogScope(scopedValues);
		return logger.EnrichPermantently(scopes);
	}

#if NETCOREAPP3_0_OR_GREATER
	/// <summary>
	/// Creates a new logging scope with named values extracted from the given parameters that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained even though its value is specified. </exception>
	[Obsolete($"Use {nameof(EnrichPermantently)} instead.")]
	public static ILogger PinScope
	(
		this ILogger logger,
		object? value1,
		object? value2 = default,
		object? value3 = default,
		object? value4 = default,
		object? value5 = default,
		object? value6 = default,
		object? value7 = default,
		object? value8 = default,
		object? value9 = default,
		object? value10 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default,
		bool cleanCallerArgument = true
	)
		=>
		logger.EnrichPermantently
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		);
#endif

	/// <summary>
	/// Creates a new logging scope and writes a log event afterward.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="log">
	/// <para> <see cref="ValueTuple"/> containing: </para>
	/// <para> Scope (<see cref="LogScope"/>): The scope to create. </para>
	/// <para> Event (<see cref="LogEvent"/>): The event to log. </para>
	/// </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Directly creating scopes and logging afterwards is no longer supported. Instead use {nameof(Enrich)} followed by {nameof(Log)} and finally {nameof(Use)}.", true)]
	public static IDisposable CreateScopeAndLog(this ILogger logger, (LogScope Scope, LogEvent Event)? log)
		=> throw new NotSupportedException($"Directly creating scopes and logging afterwards is no longer supported. Instead use {nameof(Enrich)} followed by {nameof(Log)} and finally {nameof(Use)}.");

	/// <summary>
	/// Creates a new logging scope and writes log events afterward.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvents"> A collection of <see cref="LogEvent"/>s. </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Directly creating scopes and logging afterwards is no longer supported. Instead use {nameof(Enrich)} followed by {nameof(Log)} and finally {nameof(Use)}.", true)]
	public static IDisposable CreateScopeAndLog(this ILogger logger, LogScope? scope, IEnumerable<LogEvent> logEvents)
		=> throw new NotSupportedException($"Directly creating scopes and logging afterwards is no longer supported. Instead use {nameof(Enrich)} followed by {nameof(Log)} and finally {nameof(Use)}.");

	/// <summary>
	/// Creates a new logging scope and writes a log event afterward.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvent"> The event to log. </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Directly creating scopes and logging afterwards is no longer supported. Instead use {nameof(Enrich)} followed by {nameof(Log)} and finally {nameof(Use)}.", true)]
	public static IDisposable CreateScopeAndLog(this ILogger logger, LogScope? scope, LogEvent? logEvent)
		=> throw new NotSupportedException($"Directly creating scopes and logging afterwards is no longer supported. Instead use {nameof(Enrich)} followed by {nameof(Log)} and finally {nameof(Use)}.");

	/// <summary>
	/// Creates a new logging scope and writes a log event afterward.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="log">
	/// <para> <see cref="ValueTuple"/> containing: </para>
	/// <para> Scope (<see cref="LogScope"/>): The scope to create. </para>
	/// <para> Event (<see cref="LogEvent"/>): The event to log. </para>
	/// </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.", true)]
	public static IDisposable CreateScopeAndLog<TIdentifier>(this ILogger logger, (LogScope<TIdentifier> Scope, LogEvent Event)? log)
		where TIdentifier : notnull
		=> throw new NotSupportedException($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.");

	/// <summary>
	/// Creates a new logging scope and writes log events afterward.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvents"> A collection of <see cref="LogEvent"/>s. </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.", true)]
	public static IDisposable CreateScopeAndLog<TIdentifier>(this ILogger logger, LogScope<TIdentifier>? scope, IEnumerable<LogEvent> logEvents)
		where TIdentifier : notnull
		=> throw new NotSupportedException($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.");

	/// <summary>
	/// Creates a new logging scope and writes a log event afterward.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvent"> The event to log. </param>
	/// <returns> The logging scope. </returns>
	[Obsolete($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.", true)]
	public static IDisposable CreateScopeAndLog<TIdentifier>(this ILogger logger, LogScope<TIdentifier>? scope, LogEvent? logEvent)
		where TIdentifier : notnull
		=> throw new NotSupportedException($"Directly creating scopes for a logger group is no longer supported. Instead use {nameof(AsGroup)} followed by {nameof(Enrich)}.");

	#endregion
}

/// <summary>
/// This is a special used to enable chaining of log scopes while still allowing proper disposal of all created scopes as well as continued logging.
/// </summary>
/// <remarks> This interface is not intended to be implemented externaly. </remarks>
public interface IChainingLogScopeDisposable : IDisposable, ILogger;

internal class ChainingLogScopeDisposable : IChainingLogScopeDisposable
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly IDisposable _scope;

	private readonly IDisposable? _nestedScope;

	#endregion

	#region Properties

	internal ILogger Logger { get; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor using <see cref="NoDisposable.Instance"/> as scope.
	/// </summary>
	/// <param name="logger"> The logger to associate with this scope. If the <paramref name="logger"/> is already a <see cref="ChainingLogScopeDisposable"/>, its inner logger will be used so that always the original logger persists. Cannot be <see langword="null"/>. </param>
	public ChainingLogScopeDisposable(ILogger logger)
		: this(logger, NoDisposable.Instance) { }

	/// <summary>
	/// Constructor that ensures, that the innermost logger is always available via the <see cref="Logger"/> property and that nested scopes are also properly disposed.
	/// </summary>
	/// <remarks>
	/// This constructor unwraps nested ChainingLogScopeDisposable instances to ensure that the actual underlying logger is always used, even when scopes are nested.
	/// This allows consistent access to the original logger type and behavior across multiple nested scopes.
	/// Furthermore, it ensures that all nested scopes are properly disposed of when the outermost scope is disposed.
	/// </remarks>
	/// <param name="logger"> The logger to associate with this scope. If the <paramref name="logger"/> is already a <see cref="ChainingLogScopeDisposable"/>, its inner logger will be used so that always the original logger persists. Cannot be <see langword="null"/>. </param>
	/// <param name="scope"> The disposable logging scope to manage. Cannot be <see langword="null"/>. </param>
	public ChainingLogScopeDisposable(ILogger logger, IDisposable scope)
	{
		// If the logger is wrapped within a chaining logger, always use the inner logger.
		// This way the actual logger (and its type) is always available even if a ChainingLogScopeDisposable is nested multiple times.
		var chainingLogger = logger as ChainingLogScopeDisposable;

		this.Logger = chainingLogger?.Logger ?? logger;
		_scope = scope;
		_nestedScope = chainingLogger;
	}

	#endregion

	#region Methods
	
	public void Dispose()
	{
		_scope.Dispose();
		_nestedScope?.Dispose();
	}

	#region ILogger Implementation

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this.Logger.BeginScope(state);

	public bool IsEnabled(LogLevel logLevel) => this.Logger.IsEnabled(logLevel);
	
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => this.Logger.Log(logLevel, eventId, state, exception, formatter);

	#endregion
	
	#endregion
}