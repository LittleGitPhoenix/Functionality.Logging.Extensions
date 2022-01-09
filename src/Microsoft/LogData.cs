#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

public readonly struct LogData : IEquatable<LogData>
{
    public EventId EventId { get; }

    public string Message { get; }

    public string UnformattedOutput { get; }

    public LogData(EventId eventId, string message, string unformattedOutput)
    {
        this.EventId = eventId;
        this.Message = message;
        this.UnformattedOutput = unformattedOutput;
    }

    public void Deconstruct(out string logMessage, out string unformattedOutput)
        => this.Deconstruct(out _, out logMessage, out unformattedOutput);

    public void Deconstruct(out EventId eventId, out string logMessage, out string unformattedOutput)
    {
        eventId = this.EventId;
        logMessage = this.Message;
        unformattedOutput = this.UnformattedOutput;
    }

    #region IEquatable

    /// <summary> The default hash method. </summary>
    /// <returns> A hash value for the current object. </returns>
    public override int GetHashCode()
    {
        return this.EventId.GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is LogData other && this.Equals(other);
    }

    /// <inheritdoc />
    public bool Equals(LogData other)
    {
        return this.EventId.Equals(other.EventId);
    }

    /// <summary>
    /// Compares the two instance <paramref name="x"/> and <paramref name="y"/> for equality.
    /// </summary>
    /// <param name="x"> The first instance to compare. </param>
    /// <param name="y"> The second instance to compare. </param>
    /// <returns> <c>True</c> if <paramref name="x"/> equals <paramref name="y"/>, otherwise <c>False</c>. </returns>		
    public static bool operator ==(LogData x, LogData y)
    {
        return x.GetHashCode() == y.GetHashCode();
    }

    /// <summary>
    /// Compares the two instance <paramref name="x"/> and <paramref name="y"/> for in-equality.
    /// </summary>
    /// <param name="x"> The first instance to compare. </param>
    /// <param name="y"> The second instance to compare. </param>
    /// <returns> <c>True</c> if <paramref name="x"/> doesn't equal <paramref name="y"/>, otherwise <c>False</c>. </returns>
    public static bool operator !=(LogData x, LogData y)
    {
        return !(x == y);
    }

    #endregion
}