#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Serilog;

/// <summary>
/// <see cref="ILogEventEnricher"/> that adds different information about an application to log events.
/// </summary>
public sealed class ApplicationInformationEnricher : ILogEventEnricher
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	
	private readonly LogEventProperty[] _logEventProperties;

	#endregion

	#region Properties
	#endregion

	#region Enumerations

	/// <summary>
	/// Defines the properties of <see cref="LogApplicationInformation"/> that will enrich log events.
	/// </summary>
	[Flags]
	public enum LogApplicationInformationParts
	{
		/// <summary> Used if no property from <see cref="LogApplicationInformation.Name"/> should be used to enrich log events. </summary>
		None = 0b000_0000,
		/// <summary> Enriches log events with the <see cref="LogApplicationInformation.Name"/> as <b>ApplicationName</b> property. </summary>
		Name = 0b000_0001,
		/// <summary> Enriches log events with the <see cref="LogApplicationInformation.NumericIdentifier"/> as <b>ApplicationId</b> property. </summary>
		NumericIdentifier = 0b000_0010,
		/// <summary> Enriches log events with the <see cref="LogApplicationInformation.AlphanumericIdentifier"/> as <b>ApplicationIdentifier</b> property. </summary>
		AlphanumericIdentifier = 0b000_0100,
		/// <summary> Enriches log events with the <see cref="LogApplicationInformation.AssemblyVersion"/> as <b>ApplicationVersion</b> property. </summary>
		/// <remarks> Even though this is a flags enumeration, only one of the <b>Version flags</b> may be used. In case multiple such flags are defined, always the lowest will be used. </remarks>
		AssemblyVersion = 0b000_1000,
		/// <summary> Enriches log events with the <see cref="LogApplicationInformation.FileVersion"/> as <b>ApplicationVersion</b> property. </summary>
		/// <remarks> Even though this is a flags enumeration, only one of the <b>Version flags</b> may be used. In case multiple such flags are defined, always the lowest will be used. </remarks>
		FileVersion = 0b001_0000,
		/// <summary> Enriches log events with the <see cref="LogApplicationInformation.InformationalVersion"/> as <b>ApplicationVersion</b> property. </summary>
		/// <remarks> Even though this is a flags enumeration, only one of the <b>Version flags</b> may be used. In case multiple such flags are defined, always the lowest will be used. </remarks>
		InformationalVersion = 0b010_0000,
	}

	#endregion

	#region (De)Constructors
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="applicationInformation"> The <see cref="LogApplicationInformation"/> used to enrich log events. </param>
	/// <param name="propertiesToLog"> The properties of <paramref name="applicationInformation"/> that should enrich log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	public ApplicationInformationEnricher(LogApplicationInformation applicationInformation, LogApplicationInformationParts propertiesToLog, Func<string?, string?>? versionModificationCallback = null)
	{
		IEnumerable<LogEventProperty> BuildLogEventProperties()
		{
			if (propertiesToLog.HasFlag(LogApplicationInformationParts.Name)) yield return new LogEventProperty("ApplicationName", new ScalarValue(applicationInformation.Name));
			if (propertiesToLog.HasFlag(LogApplicationInformationParts.NumericIdentifier)) yield return new LogEventProperty("ApplicationId", new ScalarValue(applicationInformation.NumericIdentifier));
			if (propertiesToLog.HasFlag(LogApplicationInformationParts.AlphanumericIdentifier)) yield return new LogEventProperty("ApplicationIdentifier", new ScalarValue(applicationInformation.AlphanumericIdentifier));
			if
			(
				propertiesToLog.HasFlag(LogApplicationInformationParts.AssemblyVersion)
				|| propertiesToLog.HasFlag(LogApplicationInformationParts.FileVersion)
				|| propertiesToLog.HasFlag(LogApplicationInformationParts.InformationalVersion)
			)
			{
				string? version = null;
				if (propertiesToLog.HasFlag(LogApplicationInformationParts.AssemblyVersion))
				{
					version = applicationInformation.AssemblyVersion?.ToString();
				}
				else if (propertiesToLog.HasFlag(LogApplicationInformationParts.FileVersion))
				{
					version = applicationInformation.FileVersion?.ToString();
				}
				else if (propertiesToLog.HasFlag(LogApplicationInformationParts.InformationalVersion))
				{
					version = applicationInformation.InformationalVersion;
				}
				
				if (versionModificationCallback is not null) version = versionModificationCallback.Invoke(version);
				yield return new LogEventProperty("ApplicationVersion", new ScalarValue(version ?? "unknown"));
			}
		}

		_logEventProperties = BuildLogEventProperties().ToArray();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
	{
		foreach (var eventProperty in _logEventProperties) logEvent.AddPropertyIfAbsent(eventProperty);
	}
	
	#endregion
}

/// <summary>
/// Provides extension methods for <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static partial class LoggerEnrichmentConfigurationExtensions
{
	/// <summary>
	/// Adds application information to log events using <see cref="LogApplicationInformation.Default"/>.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="propertiesToLog"> The properties of the default <see cref="LogApplicationInformation"/> instance that should enrich log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	public static LoggerConfiguration WithApplicationInformation(this LoggerEnrichmentConfiguration enrich, ApplicationInformationEnricher.LogApplicationInformationParts propertiesToLog, Func<string?, string?>? versionModificationCallback = null)
		=> enrich.WithApplicationInformation(LogApplicationInformation.Default, propertiesToLog, versionModificationCallback);

	/// <summary>
	/// Adds application information to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="applicationInformation"> Custom <see cref="LogApplicationInformation"/> used to enrich log events. </param>
	/// <param name="propertiesToLog"> The properties of <paramref name="applicationInformation"/> that should enrich log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	public static LoggerConfiguration WithApplicationInformation(this LoggerEnrichmentConfiguration enrich, LogApplicationInformation applicationInformation, ApplicationInformationEnricher.LogApplicationInformationParts propertiesToLog, Func<string?, string?>? versionModificationCallback = null)
	{
		if (enrich is null) throw new ArgumentNullException(nameof(enrich));
		return enrich.With(new ApplicationInformationEnricher(applicationInformation, propertiesToLog, versionModificationCallback));
	}
}