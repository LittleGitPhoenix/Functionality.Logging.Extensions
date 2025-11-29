#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using IFrameworkLogger = Microsoft.Extensions.Logging.ILogger;
using ISerilogLogger = Serilog.ILogger;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

/// <summary>
/// Implementation of an <see cref="IFrameworkLogger"/> that pipes events through <see cref="Serilog"/>.
/// </summary>
/// <remarks> Based on https://github.com/serilog/serilog-extensions-logging/blob/v3.1.0/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs (v3.1.0) </remarks>
public class FrameworkLogger : IFrameworkLogger
{
    #region Delegates / Events
    #endregion

    #region Constants

    internal const string OriginalFormatPropertyName = "{OriginalFormat}";

    #endregion

    #region Fields

	private readonly ISerilogLogger _serilogLogger;

    private static readonly CachingMessageTemplateParser MessageTemplateParser = new();

	/// <summary>
	/// A cache of log event properties representing event IDs.
	/// </summary>
	/// <remarks> This is shared between all instances because event ids from an application do not change during runtime and can be reused from any logger instance. </remarks>
	private static readonly ConcurrentDictionary<int, LogEventProperty> EventIdCache = new();

	internal static readonly ConcurrentDictionary<string, string> DestructureDictionary = new();
	
	internal static readonly ConcurrentDictionary<string, string> StringifyDictionary = new();

	#endregion

	#region Properties

	internal ILogScopeManager ScopeManager { get; }

    #endregion

    #region (De)Constructors
	
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serilogLogger"> The <see cref="ISerilogLogger"/> that will be used to output log events. </param>
    /// <param name="name"> An optional name for the logger. </param>
    /// <param name="propertyName"> <see cref="Constants.SourceContextPropertyName"/>, is only used if <paramref name="name"/> is not null. </param>
    public FrameworkLogger(ISerilogLogger serilogLogger, string? name = null, string propertyName = Constants.SourceContextPropertyName)
		: this(serilogLogger, new LogScopeManager(), name, propertyName) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="serilogLogger"> The <see cref="ISerilogLogger"/> that will be used to output log events. </param>
	/// <param name="scopeManager"> The <see cref="ILogScopeManager"/> that handles log scopes. </param>
	/// <param name="name"> An optional name for the logger. </param>
	/// <param name="propertyName"> <see cref="Constants.SourceContextPropertyName"/>, is only used if <paramref name="name"/> is not null. </param>
	public FrameworkLogger(ISerilogLogger serilogLogger, ILogScopeManager scopeManager, string? name = null, string propertyName = Constants.SourceContextPropertyName)
    {
        // Save parameters.

        // Initialize fields.
        this.ScopeManager = scopeManager;
		_serilogLogger = serilogLogger.ForContext([new FrameworkLoggerEnricher(scopeManager)]);
        if (name is not null) _serilogLogger = _serilogLogger.ForContext(propertyName, name);
    }

    #endregion

    #region Methods

    #region Implementation of Microsoft.Extensions.Logging.ILogger

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
		if (logLevel == LogLevel.None) return false;
		return _serilogLogger.IsEnabled(SerilogToMicrosoftLogLevelConverter.ToSerilogLevel(logLevel));
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => this.ScopeManager.AddScope(state);

	/// <inheritdoc />
	/// <remarks> Adapted from https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs → <b>Log</b> </remarks>
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (logLevel == LogLevel.None) return;
		
		var level = SerilogToMicrosoftLogLevelConverter.ToSerilogLevel(logLevel);
		if (!_serilogLogger.IsEnabled(level)) return;

		LogEvent? evt = null;
		try
		{
			evt = this.CreateLogEvent(level, eventId, state, exception, formatter);
		}
		catch (Exception ex)
		{
			global::Serilog.Debugging.SelfLog.WriteLine($"Failed to write event through {nameof(FrameworkLogger)}: {ex}");
		}

		// Do not swallow exceptions from here because Serilog takes care of them in case of WriteTo and throws them back to the caller in case of AuditTo.
		if (evt is not null) _serilogLogger.Write(evt);
	}

	#endregion

	#region Helper

	/// <remarks>
	/// Adapted from https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs → <b>PrepareWrite</b>
	/// <para>
	/// Changes include:
	/// <list type="bullet">
	/// <item><description> <see cref="OriginalFormatPropertyName"/> from this class is used. </description></item>
	/// <item><description> Conditional compilation for features not available in <b>.NET Standard 2.0</b>. </description></item>
	/// <item><description> Obtaining log event property from shared cache via <see cref="GetOrCreateEventIdProperty"/>. </description></item>
	/// </list>
	/// </para>
	/// </remarks>
	LogEvent CreateLogEvent<TState>(LogEventLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		string? messageTemplate = null;

		var properties = new Dictionary<string, LogEventPropertyValue>();

		if (state is IEnumerable<KeyValuePair<string, object?>> structure)
		{
			foreach (var property in structure)
			{
				if (property is { Key: OriginalFormatPropertyName, Value: string value })
				{
					messageTemplate = value;
				}
#if NET8_0_OR_GREATER
				else if (property.Key.StartsWith('@'))
#else
				else if (property.Key.StartsWith("@"))
#endif
				{
					if (_serilogLogger.BindProperty(GetKeyWithoutFirstSymbol(DestructureDictionary, property.Key), property.Value, true, out var destructured)) properties[destructured.Name] = destructured.Value;
				}
#if NET8_0_OR_GREATER				
				else if (property.Key.StartsWith('$'))
#else
				else if (property.Key.StartsWith("$"))
#endif
				{
					if (_serilogLogger.BindProperty(GetKeyWithoutFirstSymbol(StringifyDictionary, property.Key), property.Value?.ToString(), true, out var stringified)) properties[stringified.Name] = stringified.Value;
				}
				else
				{
					// Simple micro-optimization for the most common and reliably scalar values; could go further here.
					if (property.Value is null or string or int or long && LogEventProperty.IsValidName(property.Key)) properties[property.Key] = new ScalarValue(property.Value);
					else if (_serilogLogger.BindProperty(property.Key, property.Value, false, out var bound)) properties[bound.Name] = bound.Value;
				}
			}

			var stateType = state.GetType();
			var stateTypeInfo = stateType.GetTypeInfo();
			// Imperfect, but at least eliminates `1 names
			if (messageTemplate == null && !stateTypeInfo.IsGenericType)
			{
				messageTemplate = "{" + stateType.Name + ":l}";
				if (_serilogLogger.BindProperty(stateType.Name, AsLoggableValue(state, formatter), false, out var stateTypeProperty)) properties[stateTypeProperty.Name] = stateTypeProperty.Value;
			}
		}

		if (messageTemplate == null)
		{
			string? propertyName = null;
			if (state != null)
			{
				propertyName = "State";
				messageTemplate = "{State:l}";
			}
			// `formatter` was originally accepted as nullable, so despite the new annotation, this check should still
			// be made.
			else if (formatter != null!)
			{
				propertyName = "Message";
				messageTemplate = "{Message:l}";
			}

			if (propertyName != null)
			{
				if (_serilogLogger.BindProperty(propertyName, AsLoggableValue(state, formatter!), false, out var property)) properties[property.Name] = property.Value;
			}
		}

		// Get and apply the log event property.
		var logEventProperty = GetOrCreateEventIdProperty(eventId);
		if (logEventProperty is not null) properties[logEventProperty.Name] = logEventProperty.Value;

		var (traceId, spanId) = Activity.Current is { } activity
			? (activity.TraceId, activity.SpanId)
			: (default(ActivityTraceId), default(ActivitySpanId))
			;

		var parsedTemplate = messageTemplate != null ? MessageTemplateParser.Parse(messageTemplate) : MessageTemplate.Empty;
		return LogEvent.UnstableAssembleFromParts(DateTimeOffset.Now, level, exception, parsedTemplate, properties, traceId, spanId);
	}

	/// <remarks> Equal to https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs → <b>AsLoggableValue</b> </remarks>
	static object? AsLoggableValue<TState>(TState state, Func<TState, Exception?, string>? formatter)
	{
		object? stateObj = null;
		if (formatter != null) stateObj = formatter(state, null);
		return stateObj ?? state;
	}

	/// <remarks> Equal to https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs → <b>GetKeyWithoutFirstSymbol</b> </remarks>
	internal static string GetKeyWithoutFirstSymbol(ConcurrentDictionary<string, string> source, string key)
	{
		if (source.TryGetValue(key, out var value)) return value;
		if (source.Count < 1000) return source.GetOrAdd(key, k => k.Substring(1));
		return key.Substring(1);
	}

	private static LogEventProperty? GetOrCreateEventIdProperty(EventId eventId)
    {
		if (eventId.Id == 0 && eventId.Name is null) return null;

		// Using the cache saves allocations.
		var eventIdProperty = EventIdCache.GetOrAdd
		(
			eventId.Id, _ =>
			{
				var properties = new List<LogEventProperty>(2);
				if (eventId.Id != 0)
				{
					var idProperty = new LogEventProperty("Id", new ScalarValue(eventId.Id));
					properties.Add(idProperty);
				}

				if (eventId.Name is not null)
				{
					properties.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));
				}

				return new LogEventProperty("EventId", new StructureValue(properties));
			}
		);

		return eventIdProperty;
	}

	#endregion

	#endregion

	#region Nested Types

	/// <remarks> Equal to https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/CachingMessageTemplateParser.cs </remarks>
	class CachingMessageTemplateParser
	{
		readonly MessageTemplateParser _innerParser = new();

		readonly object _templatesLock = new();
		readonly Hashtable _templates = new();

		const int MaxCacheItems = 1000;
		const int MaxCachedTemplateLength = 1024;

		public MessageTemplate Parse(string messageTemplate)
		{
			if (messageTemplate == null) throw new ArgumentNullException(nameof(messageTemplate));

			if (messageTemplate.Length > MaxCachedTemplateLength) return _innerParser.Parse(messageTemplate);

			// ReSharper disable once InconsistentlySynchronizedField
			// ignored warning because this is by design
			var result = (MessageTemplate?) _templates[messageTemplate];
			if (result != null) return result;

			result = _innerParser.Parse(messageTemplate);

			lock (_templatesLock)
			{
				// Exceeding MaxCacheItems is *not* the sunny day scenario; all we're doing here is preventing out-of-memory
				// conditions when the library is used incorrectly. Correct use (templates, rather than
				// direct message strings) should barely, if ever, overflow this cache.

				// Changing workloads through the lifecycle of an app instance mean we can gain some ground by
				// potentially dropping templates generated only in startup, or only during specific infrequent
				// activities.

				if (_templates.Count == MaxCacheItems) _templates.Clear();

				_templates[messageTemplate] = result;
			}

			return result;
		}
	}

	#endregion
}