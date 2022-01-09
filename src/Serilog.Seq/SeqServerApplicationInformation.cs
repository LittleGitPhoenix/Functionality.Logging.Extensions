using System;
using System.Reflection;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// Contains information used to register an application with a seq server.
/// </summary>
public record SeqServerApplicationInformation
{
    /// <summary>
    /// The identifier of the application that will be used to registering the application with the seq server and create an new api key.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="identifier"> <see cref="Identifier"/> </param>
    public SeqServerApplicationInformation(string identifier)
    {
        this.Identifier = identifier;
    }

    /// <summary>
    /// Creates a new <see cref="SeqServerApplicationInformation"/> consisting of the application name suffixed by the name of the machine the application is running on.
    /// </summary>
    /// <returns> A <see cref="SeqServerApplicationInformation"/> with an <see cref="Identifier"/> in the form 'ApplicationName@MachineName'. </returns>
    public static SeqServerApplicationInformation CreateWithMachineName()
    {
        var applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "[UNKNOWN]";
        var machineName = Environment.MachineName;
        return new SeqServerApplicationInformation($"{applicationName}@{machineName}");
    }
}