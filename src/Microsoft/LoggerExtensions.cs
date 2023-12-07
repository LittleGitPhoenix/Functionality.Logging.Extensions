#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

class NoDisposable : IDisposable
{
	public static NoDisposable Instance => Lazy.Value;
	private static readonly Lazy<NoDisposable> Lazy = new Lazy<NoDisposable>(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);
	
	private NoDisposable() { }

	/// <inheritdoc />
	public void Dispose() { }
}

/// <summary>
/// Provides extension methods for <see cref="ILogger"/>.
/// </summary>
public static partial class LoggerExtensions
{
	internal static readonly ConcurrentDictionary<Type, MethodInfo> CreateScopeForGroupsAndLogMethodCache;

	internal static readonly MethodInfo? CreateScopeForGroupsAndLogMethod;

	static LoggerExtensions()
	{
		CreateScopeForGroupsAndLogMethodCache = new();
		CreateScopeForGroupsAndLogMethod = typeof(LoggerExtensions)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Where(m => m.Name == nameof(CreateScopeAndLog))
			.Where(m => m.ReturnType == typeof(IDisposable))
			.Select(m => new { Method = m, Parameters = m.GetParameters(), GenericParameters = m.GetGenericArguments() })
			.Where
			(
				tuple =>
				{
					if (tuple.GenericParameters.Length != 1 || tuple.Parameters.Length != 3) return false;

					// First parameter must be an ILogger.
					var firstParameterType = tuple.Parameters[0].ParameterType;
					if (firstParameterType != typeof(ILogger)) return false;

					// Second parameter must be a generic log scope.
					var secondParameterType = tuple.Parameters[1].ParameterType.GetGenericTypeDefinition();
					if (secondParameterType != typeof(LogScope<>)) return false;

					var thirdParameterType = tuple.Parameters[2].ParameterType;
					if (thirdParameterType != typeof(LogEvent)) return false;

					return true;
				}
			)
			.Select(tuple => tuple.Method)
			.FirstOrDefault()
			;
	}

	#region Logging

	/// <summary>
	/// Logs the given <paramref name="logEvents"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="logEvents"> A collection of <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>s. </param>
	public static void Log(this ILogger logger, IEnumerable<LogEvent> logEvents)
	{
		foreach (var logEvent in logEvents) Log(logger, logEvent);
	}

	/// <summary>
	/// Logs the given <paramref name="logEvent"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="logEvent"> The <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>. </param>
	public static void Log(this ILogger logger, LogEvent? logEvent)
	{
		if (logEvent is null) return;
		Log(logger, (EventId) logEvent.EventId, logEvent.Exception, logEvent.LogLevel, logEvent.LogMessage, logEvent.Args);
	}

	/// <summary>
	/// Logs an event with a given <paramref name="logMessage"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="logMessage"> The message to log. </param>
	/// <param name="args"> Arguments passed to the log message. </param>
	public static void Log(this ILogger logger, int eventId, LogLevel logLevel, string logMessage, params object?[] args)
		=> Log(logger, (EventId) eventId, null, logLevel, logMessage, args);

	/// <summary>
	/// Logs an event with a given <paramref name="logMessage"/> and <paramref name="exception"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="exception"> The <see cref="Exception"/> to log. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="logMessage"> The message to log. </param>
	/// <param name="args"> Arguments passed to the log message. </param>
	public static void Log(this ILogger logger, int eventId, Exception exception, LogLevel logLevel, string logMessage, params object?[] args)
		=> Log(logger, (EventId) eventId, exception, logLevel, logMessage, args);
	
	/// <summary>
	/// Logs an event with an automatically disposed scope.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="logs">
	/// <para> A collection of <see cref="ValueTuple"/> each containing: </para>
	/// <para> Scope (<see cref="LogScope"/>): The scope of the event. </para>
	/// <para> Event (<see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>): The event to log. </para>
	/// </param>
	public static void Log(this ILogger logger, IEnumerable<(LogScope Scope, LogEvent Event)> logs)
	{
		foreach (var log in logs) Log(logger, log);
	}
	
	/// <summary>
	/// Logs an event with an automatically disposed scope.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="log">
	/// <para> <see cref="ValueTuple"/> containing: </para>
	/// <para> Scope (<see cref="LogScope"/>): The scope of the event. </para>
	/// <para> Event (<see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>): The event to log. </para>
	/// </param>
	public static void Log(this ILogger logger, (LogScope Scope, LogEvent Event)? log)
	{
		if (log == null) return;
		logger.Log(log.Value.Scope, log.Value.Event);
	}

	/// <summary>
	/// Logs an event with an automatically disposed scope.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope of the event. </param>
	/// <param name="logEvent"> The event to log. </param>
	public static void Log(this ILogger logger, LogScope? scope, LogEvent? logEvent)
		=> logger.CreateScopeAndLog(scope, logEvent).Dispose();

	/// <summary>
	/// Logs an event with a message resolved from a resource file and returns this message translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="logEvent"> The <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogResourceEvent"/>. </param>
	/// <returns> The translated log message or an empty string if <paramref name="logEvent"/> is <b>null</b>. </returns>
	public static string Log(this ILogger logger, LogResourceEvent? logEvent)
	{
		if (logEvent is null) return String.Empty;
		return LogEventFromResource(logger, (EventId) logEvent.EventId, logEvent.Exception, logEvent.LogLevel, logEvent.ResourceManager, logEvent.ResourceName, logEvent.LogArgs, logEvent.MessageArgs, logEvent.LogCulture);
	}

	/// <summary>
	/// Logs an event with a message resolved from a resource file and returns this message translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="resourceManager"> The <see cref="ResourceManager"/>s that is queried to find the proper messages for <paramref name="resourceName"/>. </param>
	/// <param name="resourceName"> The name of the resource that is the log message. </param>
	/// <param name="logArgs"> Optional arguments passed to the log message. Those arguments are directly passed to the underlying logger instance. </param>
	/// <param name="messageArgs"> Optional arguments merged into the returned output message via <see cref="String.Format(string,object?[])"/>. If this is omitted, then <paramref name="logArgs"/> will be used. </param>
	/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. Default value is <see cref="LogCulture"/>. </param>
	/// <returns> The translated log message. </returns>
	public static string Log(this ILogger logger, int eventId, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? logArgs = null, object?[]? messageArgs = null, CultureInfo? logCulture = null)
		=> LogEventFromResource(logger, (EventId) eventId, null, logLevel, resourceManager, resourceName, logArgs, messageArgs, logCulture);

	/// <summary>
	/// Logs an event with a message resolved from a resource file together with an <paramref name="exception"/> and returns this message translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="exception"> The <see cref="Exception"/> to log. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="resourceManager"> The <see cref="ResourceManager"/>s that is queried to find the proper messages for <paramref name="resourceName"/>. </param>
	/// <param name="resourceName"> The name of the resource that is the log message. </param>
	/// <param name="logArgs"> Optional arguments passed to the log message. Those arguments are directly passed to the underlying logger instance. </param>
	/// <param name="messageArgs"> Optional arguments merged into the returned output message via <see cref="String.Format(string,object?[])"/>. If this is omitted, then <paramref name="logArgs"/> will be used. </param>
	/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. Default value is <see cref="LogCulture"/>. </param>
	/// <returns> The translated log message. </returns>
	public static string Log(this ILogger logger, int eventId, Exception exception, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? logArgs = null, object?[]? messageArgs = null, CultureInfo? logCulture = null)
		=> LogEventFromResource(logger, (EventId) eventId, exception, logLevel, resourceManager, resourceName, logArgs, messageArgs, logCulture);

	#region Helper

	/// <summary>
	/// Logs messages while catching format exceptions.
	/// </summary>
	/// <param name="logger"> The <see cref="ILogger"/> to use. </param>
	/// <param name="eventId"> The <see cref="EventId"/> of the event. </param>
	/// <param name="exception"> An optional <see cref="Exception"/> to log. Default is null. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="logMessage"> The message to log. </param>
	/// <param name="args"> Arguments passed to the log message. </param>
	internal static void Log(ILogger logger, EventId eventId, Exception? exception, LogLevel logLevel, string logMessage, params object?[] args)
	{
		try
		{
			logger.Log(logLevel, eventId, exception, logMessage, args);
		}
		catch (AggregateException ex) when (ex.Flatten().InnerExceptions.Select(e => e.GetType()).Contains(typeof(IndexOutOfRangeException)))
		{
			var arguments = args.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", args);
			logger.Log(logLevel, eventId, exception, $"Could not format the message '{logMessage.Replace("{", "{{").Replace("}", "}}")}' because of a mismatch with the supplied arguments {arguments}.", args);
		}
		catch (Exception ex)
		{
			var arguments = args.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", args);
			System.Diagnostics.Debug.WriteLine($"Could not write log for the message '{logMessage}' with arguments '{arguments}'.");
		}
	}

	/// <summary>
	/// Logs an event with a message resolved from a resource file and returns this message translated into the current ui culture (or its nearest fallback).
	/// </summary>
	/// <param name="logger"> The <see cref="ILogger"/> to use. </param>
	/// <param name="eventId"> The id of the event. </param>
	/// <param name="exception"> An optional <see cref="Exception"/> to log. Default is null. </param>
	/// <param name="logLevel"> The <see cref="LogLevel"/> of the event. </param>
	/// <param name="resourceManager"> The <see cref="ResourceManager"/>s that is queried to find the proper messages for <paramref name="resourceName"/>. </param>
	/// <param name="resourceName"> The name of the resource that is the log message. </param>
	/// <param name="logArgs"> Optional arguments passed to the log message. Those arguments are directly passed to the underlying logger instance. </param>
	/// <param name="messageArgs"> Optional arguments merged into the returned output message via <see cref="String.Format(string,object?[])"/>. If this is omitted, then <paramref name="logArgs"/> will be used. </param>
	/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. Default value is <see cref="LogCulture"/>. </param>
	/// <returns> The translated log message. </returns>
	private static string LogEventFromResource(ILogger logger, EventId eventId, Exception? exception, LogLevel logLevel, ResourceManager resourceManager, string resourceName, object?[]? logArgs = null, object?[]? messageArgs = null, CultureInfo? logCulture = null)
	{
		logArgs ??= Array.Empty<object?>();
		messageArgs ??= logArgs;
		var (logMessage, unformattedOutput) = GetMessages(resourceManager, resourceName, logCulture ?? LogCulture, logArgs, messageArgs, eventId);

		// Log
		Log(logger, eventId, exception, logLevel, logMessage, logArgs);

		// Format output message
		try
		{
			return String.Format(unformattedOutput, messageArgs);
		}
		catch (FormatException)
		{
			var arguments = messageArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", messageArgs);
			return $"Could not format the message '{unformattedOutput}' because of a mismatch with the format arguments '{arguments}'.";
		}
	}
	
	/// <summary> The <see cref="CultureInfo"/> used for logging. </summary>
	/// <remarks> Default value is the culture <b>lo</b>. </remarks>
	public static CultureInfo LogCulture
	{
		get => InternalLogCulture;
		set => InternalLogCulture = value ?? CultureInfo.CreateSpecificCulture("lo");
	}
	private static CultureInfo InternalLogCulture = CultureInfo.CreateSpecificCulture("lo");

	/// <summary>
	/// Obtains log- and output message from <paramref name="resourceManager"/> identified by <paramref name="resourceName"/>.
	/// </summary>
	/// <param name="resourceManager"> The <see cref="ResourceManager"/>s that is queried to find the proper messages for <paramref name="resourceName"/>. </param>
	/// <param name="resourceName"> The name of the resource that represents the log message. </param>
	/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. </param>
	/// <param name="logArgs"> Arguments passed to the log message. </param>
	/// <param name="messageArgs"> Arguments merged into the returned output message. </param>
	/// <param name="eventId"> The <see cref="EventId"/>. </param>
	/// <returns>
	/// <para> A <see cref="ValueTuple"/> containing: </para>
	/// <para> • LogMessage (<see cref="string"/>): The message that is logged. </para>
	/// <para> • OutputMessage (<see cref="string"/>): The message that is returned. </para>
	/// </returns>
	private static (string LogMessage, string OutputMessage) GetMessages(ResourceManager resourceManager, string resourceName, CultureInfo logCulture, object?[] logArgs, object?[] messageArgs, EventId eventId)
	{
		string? outputMessage = null;
		var logMessage = resourceManager.GetString(resourceName, logCulture);
		if (logMessage is not null)
			outputMessage = resourceManager.GetString(resourceName);

		logMessage ??= $"No log-message found for resource '{resourceName}' of event id {eventId}. Check if the resource manager containing this resource has been passed as constructor parameter. Arguments where: {(logArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", logArgs))}";
		outputMessage ??= $"No output-message found for resource '{resourceName}' of event id {eventId}. Check if the resource manager containing this resource has been passed as constructor parameter. Arguments where: {(messageArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", messageArgs))}";

		return new(logMessage!, outputMessage!);
	}

	/// <summary>
	/// Obtains log- and output message from the first <see cref="ResourceManager"/> in <paramref name="resourceManagers"/> identified by <paramref name="resourceName"/>.
	/// </summary>
	/// <param name="resourceManagers"> A collection of <see cref="ResourceManager"/>s that are queried one by one to find the proper messages for <paramref name="resourceName"/>. </param>
	/// <param name="resourceName"> The name of the resource that represents the log message. </param>
	/// <param name="logCulture"> Optional <see cref="CultureInfo"/> used to resolve log messages from resource files. </param>
	/// <param name="logArgs"> Arguments passed to the log message. </param>
	/// <param name="messageArgs"> Arguments merged into the returned output message. </param>
	/// <param name="eventId"> The <see cref="EventId"/>. </param>
	/// <returns>
	/// <para> A <see cref="ValueTuple"/> containing: </para>
	/// <para> • LogMessage (<see cref="string"/>): The message that is logged. </para>
	/// <para> • OutputMessage (<see cref="string"/>): The message that is returned. </para>
	/// </returns>
	internal static (string LogMessage, string OutputMessage) GetMessages(ICollection<ResourceManager> resourceManagers, string resourceName, CultureInfo? logCulture, object?[] logArgs, object?[] messageArgs, EventId eventId)
	{
		string? logMessage = null;
		string? outputMessage = null;
		foreach (var resourceManager in resourceManagers)
		{
			logMessage = resourceManager.GetString(resourceName, logCulture);
			if (logMessage is not null)
			{
				outputMessage = resourceManager.GetString(resourceName);
				break;
			}
		}
		logMessage ??= $"No log-message found for resource '{resourceName}' of event id {eventId}. Check if the resource manager containing this resource has been passed as constructor parameter. Arguments where: {(logArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", logArgs))}";
		outputMessage ??= $"No output-message found for resource '{resourceName}' of event id {eventId}. Check if the resource manager containing this resource has been passed as constructor parameter. Arguments where: {(messageArgs.Length == 0 ? "<NO ARGUMENTS>" : String.Join(",", messageArgs))}";

		return (logMessage!, outputMessage!);
	}

	#endregion

	#endregion

	#region Groups

	/// <summary>
	/// Adds the <paramref name="logger"/> to all groups identified by <paramref name="groupIdentifiers"/>.
	/// </summary>
	/// <typeparam name="TIdentifier"> The type of the <paramref name="groupIdentifiers"/>. </typeparam>
	/// <param name="logger"> The <see cref="ILogger"/> to add. </param>
	/// <param name="applyExistingScope"> Should existing scopes be applied tho the <paramref name="logger"/>. Default is <b>true</b>. </param>
	/// <param name="groupIdentifiers"> A collection of group identifiers. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger AddToGroups<TIdentifier>(this ILogger logger, bool applyExistingScope = true, params TIdentifier[] groupIdentifiers)
		where TIdentifier : notnull
	{
		foreach (var groupIdentifier in groupIdentifiers) logger.AddToGroup(groupIdentifier, applyExistingScope);
		return logger;
	}
	
	/// <summary>
	/// Adds the <paramref name="logger"/> to the group identified by <paramref name="groupIdentifier"/>.
	/// </summary>
	/// <typeparam name="TIdentifier"> The type of the <paramref name="groupIdentifier"/>. </typeparam>
	/// <param name="logger"> The <see cref="ILogger"/> to add. </param>
	/// <param name="groupIdentifier"> The group identifier used when adding. </param>
	/// <param name="applyExistingScope"> Should existing scopes be applied tho the <paramref name="logger"/>. Default is <b>true</b>. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger AddToGroup<TIdentifier>(this ILogger logger, TIdentifier groupIdentifier, bool applyExistingScope = true)
		where TIdentifier : notnull
		=> LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier, applyExistingScope);

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
	/// Creates a new logging <paramref name="scope"/>.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScope(this ILogger logger, LogScope? scope)
		=> scope == null ? NoDisposable.Instance : logger.BeginScope((IDictionary<string, object?>) scope);

	/// <summary>
	/// Creates a new logging <paramref name="scope"/> for a log group.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The logging scope. </returns>
	/// <remarks> It may be better to get the groups of a logger with the <see cref="AsGroup{TIdentifier}"/> extension method and then applying the scope with one of the group methods like <see cref="ILoggerGroup.CreateScope(LogScope)"/>. </remarks>
	public static IDisposable CreateScope<TIdentifier>(this ILogger logger, LogScope<TIdentifier>? scope)
		where TIdentifier : notnull
		=> scope == null ? NoDisposable.Instance : logger.AsGroup(scope.Identifier).CreateScope(scope);

	/// <summary>
	/// Creates a new logging scope with named values.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> Collection of named values. </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScope(this ILogger logger, params (string Identifier, object? Value)[] scopedValues)
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary(scopedValues);
		return logger.BeginScope(scopes);
	}

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScope(this ILogger logger, params Expression<Func<object>>[] scopedValues)
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary(scopedValues);
		return logger.BeginScope(scopes);
	}

#if NETCOREAPP3_0_OR_GREATER

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="value2"> See: <paramref name="name1"/>. </param>
	/// <param name="name2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="value1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <b>true</b>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
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
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		);
		return logger.BeginScope(scopes);
	}

#endif

	/// <summary>
	/// Creates a new logging <paramref name="scope"/> that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger PinScope(this ILogger logger, LogScope? scope)
	{
		if (scope is not null)
			logger.BeginScope((IDictionary<string, object?>) scope);
		return logger;
	}

	/// <summary>
	/// Creates a new logging scope with named values that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> Collection of named values. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger PinScope(this ILogger logger, params (string Identifier, object? Value)[] scopedValues)
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary(scopedValues);
		logger.BeginScope(scopes);
		return logger;
	}

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	public static ILogger PinScope(this ILogger logger, params Expression<Func<object>>[] scopedValues)
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary(scopedValues);
		logger.BeginScope(scopes);
		return logger;
	}

#if NETCOREAPP3_0_OR_GREATER

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s that is not removable.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="value2"> See: <paramref name="name1"/>. </param>
	/// <param name="name2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="value1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <b>true</b>. </param>
	/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
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
	{
		var scopes = LogScopeBuilder.BuildScopeDictionary
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		);
		logger.BeginScope(scopes);
		return logger;
	}

#endif

	/// <summary>
	/// Creates a new logging scope and writes a log event afterwards.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="log">
	/// <para> <see cref="ValueTuple"/> containing: </para>
	/// <para> Scope (<see cref="LogScope"/>): The scope to create. </para>
	/// <para> Event (<see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>): The event to log. </para>
	/// </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScopeAndLog(this ILogger logger, (LogScope Scope, LogEvent Event)? log)
	{
		if (log is null) return NoDisposable.Instance;

		/*
		//! Bellow check if the log scope is generic is needed because automatic type inference does not work if the generic type parameter is inside a ValueTuple.
		//! So, instead the 'CreateScopeAndLog<TIdentifier>(this ILogger logger, (LogScope<TIdentifier> Scope, LogEvent Event)? log)' overload getting invoked that should be called if the log scope is generic, this overload is invoked.
		//! This leads to unexpected behavior. Therefor the log scope is checked and the correct method is invoked if it is generic.
		*/
		var logScope = log.Value.Scope;
		var logScopeType = logScope.GetType();
		if (logScopeType.IsGenericType && CreateScopeForGroupsAndLogMethod is not null)
		{
			var genericType = logScopeType.GenericTypeArguments.First();
			var genericMethod = CreateScopeForGroupsAndLogMethodCache.GetOrAdd(genericType, _ => CreateScopeForGroupsAndLogMethod.MakeGenericMethod(genericType));
			return (genericMethod.Invoke(null, parameters: new object[] {logger, logScope, log.Value.Event}) as IDisposable)!;
		}
		else
		{
			return logger.CreateScopeAndLog(log.Value.Scope, log.Value.Event);
		}
	}

	/// <summary>
	/// Creates a new logging scope and writes log events afterwards.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvents"> A collection of <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>s. </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScopeAndLog(this ILogger logger, LogScope? scope, IEnumerable<LogEvent> logEvents)
	{
		var disposable = scope is null ? NoDisposable.Instance : logger.BeginScope((IDictionary<string, object?>) scope);
		Log(logger, logEvents);
		return disposable;
	}

	/// <summary>
	/// Creates a new logging scope and writes a log event afterwards.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvent"> The event to log. </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScopeAndLog(this ILogger logger, LogScope? scope, LogEvent? logEvent)
	{
		var disposable = scope is null ? NoDisposable.Instance : logger.BeginScope((IDictionary<string, object?>) scope);
		logger.Log(logEvent);
		return disposable;
	}

	/// <summary>
	/// Creates a new logging scope and writes a log event afterwards.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="log">
	/// <para> <see cref="ValueTuple"/> containing: </para>
	/// <para> Scope (<see cref="LogScope"/>): The scope to create. </para>
	/// <para> Event (<see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>): The event to log. </para>
	/// </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScopeAndLog<TIdentifier>(this ILogger logger, (LogScope<TIdentifier> Scope, LogEvent Event)? log)
		where TIdentifier : notnull
		=> log is null ? NoDisposable.Instance : logger.CreateScopeAndLog(log.Value.Scope, log.Value.Event);
		
	/// <summary>
	/// Creates a new logging scope and writes log events afterwards.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// /// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvents"> A collection of <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LogEvent"/>s. </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScopeAndLog<TIdentifier>(this ILogger logger, LogScope<TIdentifier>? scope, IEnumerable<LogEvent> logEvents)
		where TIdentifier : notnull
	{
		var disposable = scope is null ? NoDisposable.Instance : logger.AsGroup(scope.Identifier).CreateScope(scope);
		Log(logger, logEvents);
		return disposable;
	}

	/// <summary>
	/// Creates a new logging scope and writes a log event afterwards.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="scope"> The scope to apply. </param>
	/// <param name="logEvent"> The event to log. </param>
	/// <returns> The logging scope. </returns>
	public static IDisposable CreateScopeAndLog<TIdentifier>(this ILogger logger, LogScope<TIdentifier>? scope, LogEvent? logEvent)
		where TIdentifier : notnull
	{
		var disposable = scope is null ? NoDisposable.Instance : logger.AsGroup(scope.Identifier).CreateScope(scope);
		logger.Log(logEvent);
		return disposable;
	}

	#endregion
}