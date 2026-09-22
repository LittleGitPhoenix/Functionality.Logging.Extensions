#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Contains information about an application.
/// </summary>
/// <remarks> Construct instances of this class via builder pattern by calling the static <see cref="Create"/> function that returns a <see cref="ILogApplicationInformationBuilder"/> and go from there. </remarks>
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
	public static LogApplicationInformation None { get; } = new(String.Empty)
	{
		Name = String.Empty,
		NumericIdentifier = 0,
		AlphanumericIdentifier = String.Empty,
		AssemblyVersion = null,
		FileVersion = null,
		InformationalVersion = null
	};

	/// <summary> <see cref="LogApplicationInformation"/> instance build with information obtained by the entry assembly. </summary>
	public static LogApplicationInformation Default { get; } = Create().StartingWithApplicationName().Build();

	/// <summary>
	/// The name of the application.
	/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public string Name { get; private init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	/// <summary>
	/// A unique numeric identifier build from <see cref="Name"/> that could be used to register the application with a log target or to enrich log events.
	/// </summary>
	public int NumericIdentifier { get; private init; }

	/// <summary>
	/// A unique 20 chars long alphanumeric identifier build from <see cref="Name"/> that could be used to register the application with a log target or to enrich log events.
	/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public string AlphanumericIdentifier { get; private init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	/// <summary> The assembly version of the running executable, which is specified in the project file as <b>AssemblyVersion</b> (https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-version). </summary>
	/// <remarks> If the version couldn't be obtained, this will be a zero-version. </remarks>
	public Version? AssemblyVersion { get; private init; }

	/// <summary> The file version of the running executable, which is specified in the project file as <b>FileVersion</b> (https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-file-version). </summary>
	/// <remarks> If the version couldn't be obtained, this will be a zero-version. </remarks>
	public Version? FileVersion { get; private init; }

	/// <summary> The informational version of the running executable, which is specified in the project file as <b>InformationalVersion</b> (https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning#assembly-informational-version). </summary>
	/// <remarks> If the version couldn't be obtained, this will be <b>UNKNOWN</b>. </remarks>
	public string? InformationalVersion { get; private init; }

	#endregion

	#region (De)Constructors

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