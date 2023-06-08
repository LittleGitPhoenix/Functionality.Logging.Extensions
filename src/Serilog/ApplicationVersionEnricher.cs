#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Functionality.Logging.Base;
using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog;

/// <summary>
/// <see cref="ILogEventEnricher"/> that adds the application version as property <b>ApplicationVersion</b> to log events.
/// </summary>
[Obsolete("Use 'ApplicationInformationEnricher' instead.")]
public sealed class ApplicationVersionEnricher : ILogEventEnricher
{
	#region Delegates / Events
	#endregion

	#region Constants

	/// <summary> The property name used when enriching events. </summary>
	public const string PropertyName = "ApplicationVersion";

	#endregion

	#region Fields

	private readonly LogEventProperty _logEventProperty;

	#endregion

	#region Properties
	#endregion

	#region Enumerations

	/// <summary>
	/// Defines different version types.
	/// </summary>
	public enum VersionType
	{
		/// <summary> https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-version </summary>
		AssemblyVersion,
		/// <summary> https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-file-version </summary>
		FileVersion,
		/// <summary> https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-informational-version </summary>
		InformationalVersion
	}

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="versionType"> The <see cref="VersionType"/> to use for enriching log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	public ApplicationVersionEnricher(VersionType versionType, Func<string?, string?>? versionModificationCallback = null)
	{
		var version = versionType switch
		{
			VersionType.AssemblyVersion => LogApplicationInformation.Default.AssemblyVersion?.ToString(),
			VersionType.FileVersion => LogApplicationInformation.Default.FileVersion?.ToString(),
			VersionType.InformationalVersion => LogApplicationInformation.Default.InformationalVersion,
			_ => LogApplicationInformation.Default.FileVersion?.ToString()
		};

		if (versionModificationCallback is not null) version = versionModificationCallback.Invoke(version);
		_logEventProperty = new LogEventProperty(PropertyName, new ScalarValue(version ?? "unknown"));
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
	/// Adds the application version as property <b>ApplicationVersion</b> to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="versionType"> The version type to use. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	[Obsolete("Use 'WithApplicationInformation' instead.")]
	public static LoggerConfiguration WithApplicationVersion(this LoggerEnrichmentConfiguration enrich, ApplicationVersionEnricher.VersionType versionType = ApplicationVersionEnricher.VersionType.FileVersion, Func<string?, string?>? versionModificationCallback = null)
	{
		if (enrich is null) throw new ArgumentNullException(nameof(enrich));
		return enrich.With(new ApplicationVersionEnricher(versionType, versionModificationCallback));
	}
}