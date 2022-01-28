using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

namespace Serilog.Seq.Test;

public class SeqServerTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
		_title = Guid.NewGuid().ToString();
		_apiKey = Guid.NewGuid().ToString().Replace("-", String.Empty);
		_seqHost = "http://localhost";
		_seqPort = (ushort) 5341;
    }

	#endregion

	#region Data

	private string _title;

	private string _apiKey;

	private string _seqHost;

	private ushort _seqPort;

	private const string ConfigurationApiKey = "pYHlGsUQw5RsLSFTJHKF";

	#endregion

	[Test]
    public void Check_Register_Application_Can_Be_Canceled()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var seqServer = new SeqServer(_seqHost, 5342, "***");

        // Act + Assert
        var exception = Assert.CatchAsync<SeqServerApplicationRegisterException>(() => seqServer.RegisterApplicationAsync("MyApplication", cancellationTokenSource.Token));

        // Assert
        Assert.That(exception?.InnerException, Is.AssignableTo(typeof(OperationCanceledException)));
    }
}