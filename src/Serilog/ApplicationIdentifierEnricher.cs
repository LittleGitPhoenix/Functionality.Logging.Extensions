using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog
{
	/// <summary>
	/// <see cref="ILogEventEnricher"/> that adds a unique application identifier to log events.
	/// </summary>
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
		/// <param name="values"> A collection of strings from which a unique identifier is created via <see cref="BuildAlphanumericIdentifier"/>. </param>
		/// <remarks> The order of the values is not relevant for building the identifier. </remarks>
		public ApplicationIdentifierEnricher(params string[] values)
			: this(IdentifierBuilder.BuildAlphanumericIdentifier(values))
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
	public static class LoggerEnrichmentConfigurationExtensions
	{
		/// <summary>
		/// Adds a unique <paramref name="applicationIdentifier"/> to log events.
		/// </summary>
		/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
		/// <param name="applicationIdentifier"> The unique identifier. </param>
		/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
		public static LoggerConfiguration WithApplicationIdentifier(this LoggerEnrichmentConfiguration enrich, string applicationIdentifier)
		{
			if (enrich is null) throw new ArgumentNullException(nameof(enrich));
			return enrich.With(new ApplicationIdentifierEnricher(applicationIdentifier));
		}

		/// <summary>
		/// Adds a unique alphanumeric to log events that is created from <paramref name="values"/>.
		/// </summary>
		/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
		/// <param name="values"> A collection of strings from which an unique alphanumeric identifier is created. The order of the values is not relevant for building the identifier. </param>
		/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
		public static LoggerConfiguration WithApplicationIdentifier(this LoggerEnrichmentConfiguration enrich, params string[] values)
		{
			if (enrich is null) throw new ArgumentNullException(nameof(enrich));
			return enrich.With(new ApplicationIdentifierEnricher(values));
		}
	}
}