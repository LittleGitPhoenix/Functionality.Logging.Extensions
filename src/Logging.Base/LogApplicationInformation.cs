#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Contains information about an application.
/// </summary>
public record LogApplicationInformation()
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <summary> Null-object </summary>
	public static LogApplicationInformation None { get; }

	/// <summary> <see cref="LogApplicationInformation"/> instance build with information obtained by the entry assembly. </summary>
	public static LogApplicationInformation Default { get; }

	/// <summary>
	/// The name of the application.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// A unique numeric identifier build from <see cref="Name"/> that can be used for example to register the application with a log target or to enrich log events.
	/// </summary>
	public int NumericIdentifier { get; init; }

	/// <summary>
	/// A unique 20 chars long alpha-numeric identifier build from <see cref="Name"/> that can be used for example to register the application with a log target or to enrich log events.
	/// </summary>
	public string AlphanumericIdentifier { get; init; }

	/// <summary> The assembly version of the running executable, which is specified in the project file as <b>AssemblyVersion</b> (https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-version). </summary>
	/// <remarks> If the version couldn't be obtained, this will be a zero-version. </remarks>
	public Version? AssemblyVersion { get; init; }

	/// <summary> The file version of the running executable, which is specified in the project file as <b>FileVersion</b> (https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-file-version). </summary>
	/// <remarks> If the version couldn't be obtained, this will be a zero-version. </remarks>
	public Version? FileVersion { get; init; }

	/// <summary> The informational version of the running executable, which is specified in the project file as <b>InformationalVersion</b> (https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-informational-version). </summary>
	/// <remarks> If the version couldn't be obtained, this will be <b>UNKNOWN</b>. </remarks>
	public string? InformationalVersion { get; init; }

	#endregion

	#region (De)Constructors

	static LogApplicationInformation()
	{
		None = new LogApplicationInformation()
		{
			Name = String.Empty,
			NumericIdentifier = 0,
			AlphanumericIdentifier = String.Empty,
			AssemblyVersion = null,
			FileVersion = null,
			InformationalVersion = null
		};

		Default = LogApplicationInformation.Create().StartingWithApplicationName().Build();
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="name"> <inheritdoc cref="Name"/> </param>
	internal LogApplicationInformation(string name) : this()
	{
		// Save parameters.
		this.Name = name;

		// Initialize fields.
		(this.NumericIdentifier, this.AlphanumericIdentifier) = IdentifierBuilder.BuildNumericAndAlphanumericIdentifier(name);
		(this.AssemblyVersion, this.FileVersion, this.InformationalVersion) = VersionProvider.GetVersions();
	}

	#endregion

	#region Methods

	/// <summary>
	/// Starts creating <see cref="LogApplicationInformation"/> via builder pattern.
	/// </summary>
	public static ILogApplicationInformationBuilder Create() => new LogApplicationInformationBuilder();

	#endregion
}