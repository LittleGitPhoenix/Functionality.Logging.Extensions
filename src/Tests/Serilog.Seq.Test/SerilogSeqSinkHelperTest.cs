using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

namespace Serilog.Seq.Test;

public class SerilogSeqSinkHelperTest
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
	public void GetSeqSinkViaReflectionSucceeds()
	{
		// Act
		var success = SerilogSeqSinkHelper.TryGetSeqRequirements(out _, out _, SelfLogger.DefaultSelfLogger, "http://localhost");

		// Assert
		Assert.That(success, Is.True);
	}
}