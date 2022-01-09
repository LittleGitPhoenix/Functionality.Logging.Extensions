#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// Special exception used by the seq sink or the seq server helper classes.
/// </summary>
public class SeqServerException : AggregateException
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"> <see cref="Exception.Message"/> </param>
    public SeqServerException(string message) : this(message, Array.Empty<Exception>()) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"> <see cref="Exception.Message"/> </param>
    /// <param name="innerExceptions"> <see cref="AggregateException.InnerExceptions"/> </param>
    public SeqServerException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions) { }
}

/// <summary>
/// Special exception used if registering an application with an seq server failed.
/// </summary>
public class SeqServerApplicationRegisterException : SeqServerException
{
    /// <summary>
    /// The api key that was used for registration.
    /// </summary>
    public string ApiKey { get; }

    /// <inheritdoc />
    /// <param name="apiKey"> <see cref="ApiKey"/> </param>
    /// <param name="message"> <see cref="Exception.Message"/> </param>
    public SeqServerApplicationRegisterException(string apiKey, string message) : this(apiKey, message, Array.Empty<Exception>()) { }

    /// <inheritdoc />
    /// <param name="apiKey"> <see cref="ApiKey"/> </param>
    /// <param name="message"> <see cref="Exception.Message"/> </param>
    /// <param name="innerExceptions"> <see cref="Exception.InnerException"/> </param>
    public SeqServerApplicationRegisterException(string apiKey, string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
    {
        this.ApiKey = apiKey;
    }

    /// <summary>
    /// Creates a new instance from an existing <see cref="SeqServerException"/>.
    /// </summary>
    /// <param name="apiKey"> <see cref="ApiKey"/> </param>
    /// <param name="exception"> The original <see cref="SeqServerException"/>. </param>
    /// <returns> A new <see cref="SeqServerApplicationRegisterException "/> instance. </returns>
    public static SeqServerApplicationRegisterException CreateFromSeqServerException(string apiKey, SeqServerException exception)
    {
        return new SeqServerApplicationRegisterException(apiKey, exception.Message, exception.InnerExceptions);
    }
}