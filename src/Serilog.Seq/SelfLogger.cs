#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Serilog.Debugging;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// The log level used for internal error logging via <see cref="SelfLog"/>.
/// </summary>
public enum SeqSinkErrorLogLevel
{
    /// <summary> No log messages. </summary>
    None,
    /// <summary> Simple log messages. </summary>
    Simple,
    /// <summary> Full log messages including error stack trace. </summary>
    Full,
}

internal class SelfLogger
{
    #region Delegates / Events
    #endregion

    #region Constants
    #endregion

    #region Fields

    internal static SelfLogger DefaultSelfLogger;

    #endregion

    #region Properties
    
    internal Action<string, Exception?> LogCallback { get; }
    
    #endregion

    #region (De)Constructors

    static SelfLogger()
    {
        DefaultSelfLogger = new SelfLogger(SeqSinkErrorLogLevel.Full);
    }

    public SelfLogger(SeqSinkErrorLogLevel errorLogLevel)
    {
        switch (errorLogLevel)
        {
            case SeqSinkErrorLogLevel.None:
            {
                this.LogCallback = LogNone;
                break;
            }
            case SeqSinkErrorLogLevel.Simple:
            {
                this.LogCallback = LogSimple;
                break;
            }
            default:
            case SeqSinkErrorLogLevel.Full:
            {
                this.LogCallback = LogFull;
                break;
            }
        }
    }
		
    #endregion

    #region Methods

    public static explicit operator SelfLogger(SeqSinkErrorLogLevel errorLogLevel) => new SelfLogger(errorLogLevel);

    internal void Log(string message) => this.LogCallback.Invoke(message, null);
        
    internal void Log(string message, Exception exception) => this.LogCallback.Invoke(message, exception);
		
    internal static void LogNone(string message, Exception? exception) { }

    internal static void LogSimple(string message, Exception? exception) => SelfLog.WriteLine(message);

    internal static void LogFull(string message, Exception? exception) => SelfLog.WriteLine($"{message}{(exception is null ? String.Empty : $" Exception was:\n{exception}")}");

    #endregion
}