#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using IFrameworkLogger = Microsoft.Extensions.Logging.ILogger;
using ISerilogLogger = Serilog.ILogger;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

/// <summary>
/// Implementation of an <see cref="IFrameworkLogger"/> that pipes events through <see cref="Serilog"/>.
/// </summary>
/// <remarks> Based on https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLogger.cs </remarks>
public class FrameworkLogger : IFrameworkLogger
{
    #region Delegates / Events
    #endregion

    #region Constants

    internal const string OriginalFormatPropertyName = "{OriginalFormat}";

    #endregion

    #region Fields

    static readonly CachingMessageTemplateParser MessageTemplateParser;

    // It's rare to see large event ids, as they are category-specific
    static readonly LogEventProperty[] LowEventIdValues;

    private readonly ISerilogLogger _serilogLogger;

    #endregion

    #region Properties

    internal FrameworkLoggerScopes Scopes { get; }

    #endregion

    #region (De)Constructors

    static FrameworkLogger()
    {
        MessageTemplateParser = new CachingMessageTemplateParser();
        LowEventIdValues = Enumerable.Range(0, 48)
            .Select(number => new LogEventProperty("Id", new ScalarValue(number)))
            .ToArray()
            ;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serilogLogger"> The <see cref="ISerilogLogger"/> that will be used to output log events. </param>
    /// <param name="name"> An optional name for the logger. </param>
    /// <param name="propertyName"> <see cref="Constants.SourceContextPropertyName"/>, is only used if <paramref name="name"/> is not null. </param>
    public FrameworkLogger(ISerilogLogger serilogLogger, string? name = null, string propertyName = Constants.SourceContextPropertyName)
    {
        // Save parameters.

        // Initialize fields.
        this.Scopes = new FrameworkLoggerScopes();
        _serilogLogger = serilogLogger.ForContext(new[] { new FrameworkLoggerEnricher(this.Scopes) });
        if (name is not null) _serilogLogger = _serilogLogger.ForContext(propertyName, name);
    }

    #endregion

    #region Methods

    #region Implementation of Microsoft.Extensions.Logging.ILogger

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return _serilogLogger.IsEnabled(LogLevelConverter.ToSerilogLevel(logLevel));
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return this.Scopes.AddScope(state);
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        var level = LogLevelConverter.ToSerilogLevel(logLevel);
        if (!_serilogLogger.IsEnabled(level))
        {
            return;
        }

        try
        {
            this.Write(level, eventId, state, exception, formatter);
        }
        catch (Exception ex)
        {
            global::Serilog.Debugging.SelfLog.WriteLine($"Failed to write event through {nameof(FrameworkLogger)}: {ex}");
        }
    }

    #endregion

    #region Helper

    private void Write<TState>(LogEventLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        var logger = _serilogLogger;
        string messageTemplate = null;

        var properties = new List<LogEventProperty>();

        if (state is IEnumerable<KeyValuePair<string, object>> structure)
        {
            foreach (var property in structure)
            {
                if (property.Key == FrameworkLogger.OriginalFormatPropertyName && property.Value is string value)
                {
                    messageTemplate = value;
                }
                else if (property.Key.StartsWith("@"))
                {
                    if (logger.BindProperty(property.Key.Substring(1), property.Value, true, out var destructured))
                        properties.Add(destructured);
                }
                else if (property.Key.StartsWith("$"))
                {
                    if (logger.BindProperty(property.Key.Substring(1), property.Value?.ToString(), true, out var stringified))
                        properties.Add(stringified);
                }
                else
                {
                    if (logger.BindProperty(property.Key, property.Value, false, out var bound))
                        properties.Add(bound);
                }
            }

            var stateType = state.GetType();
            var stateTypeInfo = stateType.GetTypeInfo();
            // Imperfect, but at least eliminates `1 names
            if (messageTemplate == null && !stateTypeInfo.IsGenericType)
            {
                messageTemplate = "{" + stateType.Name + ":l}";
                if (logger.BindProperty(stateType.Name, AsLoggableValue(state, formatter), false, out var stateTypeProperty))
                    properties.Add(stateTypeProperty);
            }
        }

        if (messageTemplate == null)
        {
            string propertyName = null;
            if (state is not null)
            {
                propertyName = "State";
                messageTemplate = "{State:l}";
            }
            else if (formatter is not null)
            {
                propertyName = "Message";
                messageTemplate = "{Message:l}";
            }

            if (propertyName is not null)
            {
#pragma warning disable 8604
                if (logger.BindProperty(propertyName, AsLoggableValue(state, formatter), false, out var property)) properties.Add(property);
#pragma warning restore 8604
			}
        }

        if (eventId.Id != 0 || eventId.Name is not null) properties.Add(CreateEventIdProperty(eventId));

        var parsedTemplate = MessageTemplateParser.Parse(messageTemplate ?? "");
        var evt = new LogEvent(DateTimeOffset.Now, level, exception, parsedTemplate, properties);
        logger.Write(evt);
    }

    private static object AsLoggableValue<TState>(TState state, Func<TState, Exception, string> formatter)
    {
        object sobj = state;
        if (formatter is not null) sobj = formatter(state, null);
        return sobj;
    }

    private static LogEventProperty CreateEventIdProperty(EventId eventId)
    {
        var properties = new List<LogEventProperty>(2);

        if (eventId.Id != 0)
        {
            if (eventId.Id >= 0 && eventId.Id < LowEventIdValues.Length)
                // Avoid some allocations
                properties.Add(LowEventIdValues[eventId.Id]);
            else
                properties.Add(new LogEventProperty("Id", new ScalarValue(eventId.Id)));
        }

        if (eventId.Name != null)
        {
            properties.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));
        }

        return new LogEventProperty("EventId", new StructureValue(properties));
    }

    #endregion

    #endregion

    #region Nested Types

    class CachingMessageTemplateParser
    {
        readonly global::Serilog.Parsing.MessageTemplateParser _innerParser = new global::Serilog.Parsing.MessageTemplateParser();

        readonly object _templatesLock = new ();
        readonly System.Collections.Hashtable _templates = new ();

        const int MaxCacheItems = 1000;
        const int MaxCachedTemplateLength = 1024;

        public MessageTemplate Parse(string messageTemplate)
        {
            if (messageTemplate == null) throw new ArgumentNullException(nameof(messageTemplate));

            if (messageTemplate.Length > MaxCachedTemplateLength)
                return _innerParser.Parse(messageTemplate);

            // ReSharper disable once InconsistentlySynchronizedField
            // ignored warning because this is by design
            var result = (MessageTemplate) _templates[messageTemplate];
            if (result != null)
                return result;

            result = _innerParser.Parse(messageTemplate);

            lock (_templatesLock)
            {
                // Exceeding MaxCacheItems is *not* the sunny day scenario; all we're doing here is preventing out-of-memory
                // conditions when the library is used incorrectly. Correct use (templates, rather than
                // direct message strings) should barely, if ever, overflow this cache.

                // Changing workloads through the lifecycle of an app instance mean we can gain some ground by
                // potentially dropping templates generated only in startup, or only during specific infrequent
                // activities.

                if (_templates.Count == MaxCacheItems)
                    _templates.Clear();

                _templates[messageTemplate] = result;
            }

            return result;
        }
    }

    #endregion
}