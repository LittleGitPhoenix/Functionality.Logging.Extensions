#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// Contains information used to register an application with a seq server.
/// </summary>
/// <param name="Identifier"> The identifier of the application that will be used to registering the application with the seq server and create an new api key. </param>
[Obsolete("If possible use 'SerilogApplicationInformation' from the 'Phoenix.Functionality.Logging.Extensions.Serilog' package.", false)]
public record SeqServerApplicationInformation(string Identifier)
{
	/// <summary>
	/// Starts creating <see cref="SeqServerApplicationInformation"/> via builder pattern.
	/// </summary>
	public static ISeqServerApplicationInformationBuilder Create() => new SeqServerApplicationInformationBuilder();

    /// <summary>
    /// Creates a new <see cref="SeqServerApplicationInformation"/> consisting of the application name suffixed by the name of the machine the application is running on.
    /// </summary>
    /// <returns> A <see cref="SeqServerApplicationInformation"/> with an <see cref="Identifier"/> in the form 'ApplicationName@MachineName'. </returns>
    [Obsolete("Use the SeqServerApplicationInformationBuilder instead. It can be accessed via SeqServerApplicationInformation.Create()...")]
    public static SeqServerApplicationInformation CreateWithMachineName()
    {
        var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "[UNKNOWN]";
        var machineName = Environment.MachineName;
        return new SeqServerApplicationInformation($"{applicationName}@{machineName}");
    }
}