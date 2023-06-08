#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Functionality.Logging.Base;

internal static class VersionProvider
{
	/// <summary>
	/// Gets the assembly-, file- and informational version of the running executable.
	/// </summary>
	/// <returns> A <see cref="ValueTuple"/> containing the versions. </returns>
	internal static (Version? AssemblyVersion, Version? FileVersion, string? InformationalVersion) GetVersions()
	{
		var entryAssembly = Assembly.GetEntryAssembly();
		if (entryAssembly is null) return (null, null, null);
		return (GetAssemblyVersion(entryAssembly), GetFileVersion(entryAssembly), GetInformationalVersion(entryAssembly));
	}
	/// <summary>
	/// Gets the assembly version of the running executable, which is specified in the project file as 'AssemblyVersion'.
	/// </summary>
	/// <returns> The assembly <see cref="Version"/> or null. </returns>
	private static Version? GetAssemblyVersion(Assembly entryAssembly)
	{
		var assemblyVersion = entryAssembly.GetName().Version ?? null;
		return assemblyVersion;
	}

	/// <summary>
	/// Gets the file version of the running executable, which is specified in the project file as 'FileVersion'.
	/// </summary>
	/// <returns> The file <see cref="Version"/> or null. </returns>
	private static Version? GetFileVersion(Assembly entryAssembly)
	{
		var fileVersionString = entryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
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
	private static string? GetInformationalVersion(Assembly entryAssembly)
	{
		var informationalVersion = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		return String.IsNullOrWhiteSpace(informationalVersion) ? null : informationalVersion;
	}
}