#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

/// <summary>
/// An <see cref="ILogEventEnricher"/> that enriches log events with scopes provided by <see cref="LogScopeManager"/>.
/// </summary>
/// <remarks> Based on https://github.com/serilog/serilog-extensions-logging/blob/v3.1.0/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerProvider.cs (v3.1.0) </remarks>
[ProviderAlias("Serilog")]
//? better name: class FrameworkLoggerScopeEnricher
internal class FrameworkLoggerEnricher : ILogEventEnricher
{
    #region Delegates / Events
    #endregion

    #region Constants

    private const string ScopePropertyName = "Scope";

	private const string NoName = "None";

	#endregion

	#region Fields

	private readonly ILogScopeManager _scopeManager;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="scopeManager"> The <see cref="ILogScopeManager"/> containing the log scopes that this class uses to enrich <see cref="LogEvent"/>s. </param>
	public FrameworkLoggerEnricher(ILogScopeManager scopeManager)
    {
        // Save parameters.
        _scopeManager = scopeManager;

        // Initialize fields.
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
		// Get all scope values.
		IEnumerable<object> scopeValues = _scopeManager.GetScopeValues();

		// Convert scope values into LogEventPropertyValues.
		var scopeItems = CreateLogEventPropertyValues(scopeValues, logEvent, propertyFactory)
			//? Why is this reversed in Serilog.Extensions.Logging.SerilogLoggerProvider?
			//? Could it be so that newer entries override older ones?
			.Reverse()
			.ToArray()
            ;

		// Add the created scope items as a sequence property.
		logEvent.AddPropertyIfAbsent(new LogEventProperty(ScopePropertyName, new SequenceValue(scopeItems)));
    }

	/// <summary>
	/// Converts all internal scopes into <see cref="LogEventPropertyValue"/>s to be used by <see cref="Serilog"/>.
	/// </summary>
	/// <returns> The <see cref="LogEventPropertyValue"/>s of this scope. </returns>
	internal static IEnumerable<LogEventPropertyValue> CreateLogEventPropertyValues(IEnumerable<object> scopeValues, LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		foreach (var state in scopeValues)
		{
			EnrichWithStateAndCreateScopeItem(logEvent, propertyFactory, state, update: false, out var scopeItem);
			if (scopeItem is null) continue;
			yield return scopeItem;
		}
	}

	/// <remarks>
	/// Adapted from https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerScope.cs → <b>EnrichWithStateAndCreateScopeItem</b>
	/// <para>
	/// Changes include:
	/// <list type="bullet">
	/// <item><description> <see cref="FrameworkLogger.OriginalFormatPropertyName"/> from <see cref="FrameworkLogger"/> class is used. </description></item>
	/// <item><description> Conditional compilation for <b>FEATURE_ITUPLE</b> has been changed to <b>NETSTANDARD2_1 || NET8_0_OR_GREATER</b>. </description></item>
	/// </list>
	/// </para>
	/// </remarks>
	public static void EnrichWithStateAndCreateScopeItem(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, object? state, bool update, out LogEventPropertyValue? scopeItem)
	{
		if (state == null)
		{
			scopeItem = null;
			return;
		}

		// Eliminates boxing of Dictionary<TKey, TValue>.Enumerator for the most common use case
		if (state is Dictionary<string, object> dictionary)
		{
			// Separate handling of this case eliminates boxing of Dictionary<TKey, TValue>.Enumerator.
			scopeItem = null;
			foreach (var stateProperty in dictionary)
			{
				AddProperty(logEvent, propertyFactory, stateProperty.Key, stateProperty.Value, update);
			}
		}
		else if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
		{
			scopeItem = null;
			foreach (var stateProperty in stateProperties)
			{
				if (stateProperty is { Key: FrameworkLogger.OriginalFormatPropertyName, Value: string })
				{
					// `_state` is most likely `FormattedLogValues` (a MEL internal type).
					scopeItem = new ScalarValue(state.ToString());
				}
				else
				{
					AddProperty(logEvent, propertyFactory, stateProperty.Key, stateProperty.Value, update);
				}
			}
		}
#if NETSTANDARD2_1 || NET8_0_OR_GREATER
		else if (state is System.Runtime.CompilerServices.ITuple tuple && tuple.Length == 2 && tuple[0] is string s)
		{
			scopeItem = null; // Unless it's `FormattedLogValues`, these are treated as property bags rather than scope items.
			AddProperty(logEvent, propertyFactory, s, tuple[1], update);
		}
#else
		else if (state is ValueTuple<string, object?> tuple)
		{
			scopeItem = null;
			AddProperty(logEvent, propertyFactory, tuple.Item1, tuple.Item2, update);
		}
#endif
		else
		{
			scopeItem = propertyFactory.CreateProperty(NoName, state).Value;
		}
	}

	/// <remarks>
	/// Adapted from https://github.com/serilog/serilog-extensions-logging/blob/v9.0.2/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerScope.cs → <b>AddProperty</b>
	/// <para>
	/// Changes include:
	/// <list type="bullet">
	/// <item><description> Excanged usage of static <b>SerilogLogger</b> to <see cref="FrameworkLogger"/>. </description></item>
	/// <item><description> Conditional compilation for features not available in <b>.NET Standard 2.0</b>. </description></item>
	/// </list>
	/// </para>
	/// </remarks>
	static void AddProperty(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, string key, object? value, bool update)
	{
		var destructureObjects = false;

#if NET8_0_OR_GREATER
		if (key.StartsWith('@'))
#else
		if (key.StartsWith("@"))
#endif
		{
			key = FrameworkLogger.GetKeyWithoutFirstSymbol(FrameworkLogger.DestructureDictionary, key);
			destructureObjects = true;
		}
#if NET8_0_OR_GREATER
		else if (key.StartsWith('$'))
#else
		else if (key.StartsWith("$"))
#endif
		{
			key = FrameworkLogger.GetKeyWithoutFirstSymbol(FrameworkLogger.StringifyDictionary, key);
			value = value?.ToString();
		}

		var property = propertyFactory.CreateProperty(key, value, destructureObjects);
		if (update)
		{
			logEvent.AddOrUpdateProperty(property);
		}
		else
		{
			logEvent.AddPropertyIfAbsent(property);
		}
	}

	#endregion
}