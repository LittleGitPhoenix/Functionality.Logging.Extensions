#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;
using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog;

/// <summary>
/// <see cref="ILogEventEnricher"/> that adds an application version to log events.
/// </summary>
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
			VersionType.AssemblyVersion => GetAssemblyVersion()?.ToString(),
			VersionType.FileVersion => GetFileVersion()?.ToString(),
			VersionType.InformationalVersion => GetInformationalVersion(),
			_ => GetFileVersion()?.ToString()
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
	
	/// <summary>
	/// Gets the assembly version of the running executable, which is specified in the project file as 'AssemblyVersion'.
	/// </summary>
	/// <returns> The assembly <see cref="Version"/> or null. </returns>
	private static Version? GetAssemblyVersion()
	{
		var entryAssembly = Assembly.GetEntryAssembly();
		var assemblyVersion = entryAssembly?.GetName().Version ?? new Version();
		return assemblyVersion;
	}

	/// <summary>
	/// Gets the file version of the running executable, which is specified in the project file as 'FileVersion'.
	/// </summary>
	/// <returns> The file <see cref="Version"/> or null. </returns>
	private static Version? GetFileVersion()
	{
		var entryAssembly = Assembly.GetEntryAssembly();
		var fileVersionString = entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		if (String.IsNullOrWhiteSpace(fileVersionString)) return null;
		try
		{
			var fileVersion = new Version(fileVersionString);
			return fileVersion;
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>
	/// Gets the informational version of the running executable, which is specified in the project file as 'InformationalVersion'.
	/// </summary>
	/// <returns> The informational <see cref="Version"/> or null. </returns>
	private static string? GetInformationalVersion()
	{
		var entryAssembly = Assembly.GetEntryAssembly();
		var informationalVersion = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		return String.IsNullOrWhiteSpace(informationalVersion) ? null : informationalVersion;
	}

	#endregion
}

/// <summary>
/// Provides extension methods for <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static partial class LoggerEnrichmentConfigurationExtensions
{
	/// <summary>
	/// Adds a the application version to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="versionType"> The version type to use. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	public static LoggerConfiguration WithApplicationVersion(this LoggerEnrichmentConfiguration enrich, ApplicationVersionEnricher.VersionType versionType = ApplicationVersionEnricher.VersionType.FileVersion, Func<string?, string?>? versionModificationCallback = null)
	{
		if (enrich is null) throw new ArgumentNullException(nameof(enrich));
		return enrich.With(new ApplicationVersionEnricher(versionType, versionModificationCallback));
	}
}