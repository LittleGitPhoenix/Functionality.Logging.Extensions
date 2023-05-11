#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging;

/// <summary>
/// Contains information about an application.
/// </summary>
/// <param name="Name"> The name of the application. </param>
/// <param name="NumericIdentifier"> A unique numeric identifier of the application that can be used for example to register the application with a log target or to enrich log events. </param>
/// <param name="AlphanumericIdentifier"> A unique alpha-numeric identifier of the application that can be used for example to register the application with a log target or to enrich log events. </param>
public record LogApplicationInformation(string Name, int NumericIdentifier, string AlphanumericIdentifier)
{
	/// <summary>
	/// Starts creating <see cref="LogApplicationInformation"/> via builder pattern.
	/// </summary>
	public static ILogApplicationInformationBuilder Create() => new LogApplicationInformationBuilder();
}