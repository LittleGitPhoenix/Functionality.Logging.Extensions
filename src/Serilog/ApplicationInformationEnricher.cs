using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Reflection;

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

	private static readonly LogApplicationInformation NoLogApplicationInformation;

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
	}

	/// <summary>
	/// Defines different version types that will enrich log events as <b>ApplicationVersion</b> property.
	/// </summary>
	public enum VersionType
	{
		/// <summary> Used if application version should not enrich log events. </summary>
		None,
		/// <summary> https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-version </summary>
		AssemblyVersion,
		/// <summary> https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-file-version </summary>
		FileVersion,
		/// <summary> https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-informational-version </summary>
		InformationalVersion
	}

	#endregion

	#region (De)Constructors

	static ApplicationInformationEnricher()
	{
		NoLogApplicationInformation = new LogApplicationInformation(String.Empty, 0, String.Empty);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="applicationInformation"> The <see cref="LogApplicationInformation"/> used to enrich log events. </param>
	/// <param name="propertiesToLog"> The properties of <paramref name="applicationInformation"/> that should enrich log events. </param>
	public ApplicationInformationEnricher(LogApplicationInformation applicationInformation, LogApplicationInformationParts propertiesToLog)
		: this(applicationInformation, propertiesToLog, VersionType.None) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="versionToLog"> The <see cref="VersionType"/> to use for enriching log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	public ApplicationInformationEnricher(VersionType versionToLog, Func<string?, string?>? versionModificationCallback = null)
		: this(NoLogApplicationInformation, LogApplicationInformationParts.None, versionToLog, versionModificationCallback) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="applicationInformation"> The <see cref="LogApplicationInformation"/> used to enrich log events. </param>
	/// <param name="propertiesToLog"> The properties of <paramref name="applicationInformation"/> that should enrich log events. </param>
	/// <param name="versionToLog"> The <see cref="VersionType"/> to use for enriching log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	public ApplicationInformationEnricher(LogApplicationInformation applicationInformation, LogApplicationInformationParts propertiesToLog, VersionType versionToLog, Func<string?, string?>? versionModificationCallback = null)
	{
		IEnumerable<LogEventProperty> BuildLogEventProperties()
		{
			if (propertiesToLog.HasFlag(LogApplicationInformationParts.Name)) yield return new LogEventProperty("ApplicationName", new ScalarValue(applicationInformation.Name));
			if (propertiesToLog.HasFlag(LogApplicationInformationParts.NumericIdentifier)) yield return new LogEventProperty("ApplicationId", new ScalarValue(applicationInformation.NumericIdentifier));
			if (propertiesToLog.HasFlag(LogApplicationInformationParts.AlphanumericIdentifier)) yield return new LogEventProperty("ApplicationIdentifier", new ScalarValue(applicationInformation.AlphanumericIdentifier));
			if (versionToLog != VersionType.None)
			{
				var version = versionToLog switch
				{
					VersionType.AssemblyVersion => GetAssemblyVersion()?.ToString(),
					VersionType.FileVersion => GetFileVersion()?.ToString(),
					VersionType.InformationalVersion => GetInformationalVersion(),
					_ => GetFileVersion()?.ToString()
				};

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

	#region Helper

	/// <summary>
	/// Gets the assembly version of the running executable, which is specified in the project file as 'AssemblyVersion'.
	/// </summary>
	/// <returns> The assembly <see cref="Version"/> or null. </returns>
	internal static Version? GetAssemblyVersion()
	{
		var entryAssembly = Assembly.GetEntryAssembly();
		var assemblyVersion = entryAssembly?.GetName().Version ?? new Version();
		return assemblyVersion;
	}

	/// <summary>
	/// Gets the file version of the running executable, which is specified in the project file as 'FileVersion'.
	/// </summary>
	/// <returns> The file <see cref="Version"/> or null. </returns>
	internal static Version? GetFileVersion()
	{
		var entryAssembly = Assembly.GetEntryAssembly();
		var fileVersionString = entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		if (String.IsNullOrWhiteSpace(fileVersionString))
			return null;
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
	internal static string? GetInformationalVersion()
	{
		var entryAssembly = Assembly.GetEntryAssembly();
		var informationalVersion = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		return String.IsNullOrWhiteSpace(informationalVersion) ? null : informationalVersion;
	}
	
	#endregion

	#endregion
}

/// <summary>
/// Provides extension methods for <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static partial class LoggerEnrichmentConfigurationExtensions
{
	/// <summary>
	/// Adds application information to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="applicationInformation"> The <see cref="LogApplicationInformation"/> used to enrich log events. </param>
	/// <param name="propertiesToLog"> The properties of <paramref name="applicationInformation"/> that should enrich log events. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	public static LoggerConfiguration WithApplicationInformation(this LoggerEnrichmentConfiguration enrich, LogApplicationInformation applicationInformation, ApplicationInformationEnricher.LogApplicationInformationParts propertiesToLog)
	{
		if (enrich is null) throw new ArgumentNullException(nameof(enrich));
		return enrich.With(new ApplicationInformationEnricher(applicationInformation, propertiesToLog));
	}

	/// <summary>
	/// Adds application information to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="versionToLog"> The <see cref="ApplicationInformationEnricher.VersionType"/> to use for enriching log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	public static LoggerConfiguration WithApplicationInformation(this LoggerEnrichmentConfiguration enrich, ApplicationInformationEnricher.VersionType versionToLog, Func<string?, string?>? versionModificationCallback = null)
	{
		if (enrich is null) throw new ArgumentNullException(nameof(enrich));
		return enrich.With(new ApplicationInformationEnricher(versionToLog, versionModificationCallback));
	}

	/// <summary>
	/// Adds application information to log events.
	/// </summary>
	/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
	/// <param name="applicationInformation"> The <see cref="LogApplicationInformation"/> used to enrich log events. </param>
	/// <param name="propertiesToLog"> The properties of <paramref name="applicationInformation"/> that should enrich log events. </param>
	/// <param name="versionToLog"> The <see cref="ApplicationInformationEnricher.VersionType"/> to use for enriching log events. </param>
	/// <param name="versionModificationCallback"> An optional callback that can be used to modify the obtained version. </param>
	/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
	public static LoggerConfiguration WithApplicationInformation(this LoggerEnrichmentConfiguration enrich, LogApplicationInformation applicationInformation, ApplicationInformationEnricher.LogApplicationInformationParts propertiesToLog, ApplicationInformationEnricher.VersionType versionToLog, Func<string?, string?>? versionModificationCallback = null)
	{
		if (enrich is null) throw new ArgumentNullException(nameof(enrich));
		return enrich.With(new ApplicationInformationEnricher(applicationInformation, propertiesToLog, versionToLog, versionModificationCallback));
	}
}