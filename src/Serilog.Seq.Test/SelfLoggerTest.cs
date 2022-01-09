using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

namespace Serilog.Seq.Test;

public class SelfLoggerTest
{
    [Test]
    public void Check_Log_Level_None_Is_Respected()
    {
        // Arrange
        
        // Act + Assert
        var selfLogger = (SelfLogger) SeqSinkErrorLogLevel.None;

        // Assert
#pragma warning disable CS8974 // Converting method group to non-delegate type
        Assert.AreEqual(SelfLogger.LogNone, selfLogger.LogCallback);
#pragma warning restore CS8974 // Converting method group to non-delegate type
    }
    
    [Test]
    public void Check_Log_Level_Simple_Is_Respected()
    {
        // Arrange

        // Act + Assert
        var selfLogger = (SelfLogger) SeqSinkErrorLogLevel.Simple;

        // Assert
#pragma warning disable CS8974 // Converting method group to non-delegate type
        Assert.AreEqual(SelfLogger.LogSimple, selfLogger.LogCallback);
#pragma warning restore CS8974 // Converting method group to non-delegate type
    }
    
    [Test]
    public void Check_Log_Level_Full_Is_Respected()
    {
        // Arrange

        // Act + Assert
        var selfLogger = (SelfLogger) SeqSinkErrorLogLevel.Full;

        // Assert
#pragma warning disable CS8974 // Converting method group to non-delegate type
        Assert.AreEqual(SelfLogger.LogFull, selfLogger.LogCallback);
#pragma warning restore CS8974 // Converting method group to non-delegate type
    }
}