#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft;

/// <summary>
/// An <see cref="ILogEventEnricher"/> that enriches log events with scopes provided by <see cref="FrameworkLoggerScopes"/>.
/// </summary>
/// <remarks> Based on https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerProvider.cs </remarks>
[ProviderAlias("Serilog")]
internal class FrameworkLoggerEnricher : ILogEventEnricher
{
    #region Delegates / Events
    #endregion

    #region Constants

    private const string ScopePropertyName = "Scope";

    #endregion

    #region Fields

    private readonly FrameworkLoggerScopes _scopes;

    #endregion

    #region Properties
    #endregion

    #region (De)Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scopes"> The <see cref="FrameworkLoggerScopes"/> that enrich <see cref="LogEvent"/>s. </param>
    public FrameworkLoggerEnricher(FrameworkLoggerScopes scopes)
    {
        // Save parameters.
        _scopes = scopes;

        // Initialize fields.
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var scopeItems = _scopes
            .CreateLogEventPropertyValues(logEvent, propertyFactory)
            //.Reverse() //? Why is this reversed in Serilog.Extensions.Logging.SerilogLoggerProvider?
            .ToArray()
            ;

        logEvent.AddPropertyIfAbsent(new LogEventProperty(ScopePropertyName, new SequenceValue(scopeItems)));
    }

    #endregion
}