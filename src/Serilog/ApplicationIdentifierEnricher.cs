#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog;

/// <summary>
/// <see cref="ILogEventEnricher"/> that adds a unique alpha-numeric application identifier as property <b>ApplicationIdentifier</b> to log events.
/// </summary>
[Obsolete("Use 'ApplicationInformationEnricher' instead.")]
public sealed class ApplicationIdentifierEnricher : ILogEventEnricher
{
    #region Delegates / Events
    #endregion

    #region Constants
		
    /// <summary> The property name used when enriching events. </summary>
    public const string PropertyName = "ApplicationIdentifier";

    #endregion

    #region Fields

    private readonly LogEventProperty _logEventProperty;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="values"> A collection of strings from which a unique identifier is created via <see cref="IdentifierBuilder.BuildAlphanumericIdentifier"/>. </param>
	/// <remarks> The order of the values is not relevant for building the identifier. </remarks>
	public ApplicationIdentifierEnricher(params string[] values)
		: this(LogApplicationInformation.Create().StartingWith(String.Join(String.Empty, values.OrderBy(value => value))).Build().AlphanumericIdentifier)
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="applicationIdentifier"> The unique application identifier. </param>
    public ApplicationIdentifierEnricher(string applicationIdentifier)
    {
        _logEventProperty = new LogEventProperty(PropertyName, new ScalarValue(applicationIdentifier));
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(_logEventProperty);
    }
		
    #endregion
}

/// <summary>
/// Provides extension methods for <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static partial class LoggerEnrichmentConfigurationExtensions
{
	/// <summary>
	/// Adds a unique <paramref name="applicationIdentifier"/> as property <b>ApplicationIdentifier</b> to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="applicationIdentifier"> The unique identifier. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	[Obsolete("Use 'WithApplicationInformation' instead.")]
	public static LoggerConfiguration WithApplicationIdentifier(this LoggerEnrichmentConfiguration enrich, string applicationIdentifier)
    {
        if (enrich is null) throw new ArgumentNullException(nameof(enrich));
        return enrich.With(new ApplicationIdentifierEnricher(applicationIdentifier));
    }

	/// <summary>
	/// Adds a unique alphanumeric identifier that is created from <paramref name="values"/> as property <b>ApplicationIdentifier</b> to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="values"> A collection of strings from which an unique alphanumeric identifier is created. The order of the values is not relevant for building the identifier. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	[Obsolete("Use 'WithApplicationInformation' instead.")]
	public static LoggerConfiguration WithApplicationIdentifier(this LoggerEnrichmentConfiguration enrich, params string[] values)
    {
        if (enrich is null) throw new ArgumentNullException(nameof(enrich));
        return enrich.With(new ApplicationIdentifierEnricher(values));
    }
}