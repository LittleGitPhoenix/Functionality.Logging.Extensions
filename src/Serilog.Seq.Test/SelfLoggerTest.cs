using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

namespace Serilog.Seq.Test;

public class SelfLoggerTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	#endregion

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