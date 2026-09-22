using Phoenix.Functionality.Logging.Extensions.Serilog;
using Serilog.Events;

namespace Serilog.Test;

[Category("SerilogLogLevelConverter")]
public class SerilogLogLevelConverterTest
{
	#region Tests

	/// <summary>
	/// Verifies that the converter exposes one shared instance.
	/// </summary>
	[Test]
	public void InstanceReturnsSingleton()
	{
		// Act
		var firstInstance = SerilogLogLevelConverter.Instance;
		var secondInstance = SerilogLogLevelConverter.Instance;

		// Assert
		Assert.That(secondInstance, Is.SameAs(firstInstance));
	}

	/// <summary>
	/// Verifies that converting a Serilog level to the target level preserves the value.
	/// </summary>
	[TestCase(LogEventLevel.Verbose)]
	[TestCase(LogEventLevel.Debug)]
	[TestCase(LogEventLevel.Information)]
	[TestCase(LogEventLevel.Warning)]
	[TestCase(LogEventLevel.Error)]
	[TestCase(LogEventLevel.Fatal)]
	public void ConvertSourceToTargetPreservesLevel(LogEventLevel level)
	{
		// Act
		var convertedLevel = SerilogLogLevelConverter.Instance.ConvertSourceToTarget(level);

		// Assert
		Assert.That(convertedLevel, Is.EqualTo(level));
	}

	/// <summary>
	/// Verifies that converting a target level to the Serilog level preserves the value.
	/// </summary>
	[TestCase(LogEventLevel.Verbose)]
	[TestCase(LogEventLevel.Debug)]
	[TestCase(LogEventLevel.Information)]
	[TestCase(LogEventLevel.Warning)]
	[TestCase(LogEventLevel.Error)]
	[TestCase(LogEventLevel.Fatal)]
	public void ConvertTargetToSourcePreservesLevel(LogEventLevel level)
	{
		// Act
		var convertedLevel = SerilogLogLevelConverter.Instance.ConvertTargetToSource(level);

		// Assert
		Assert.That(convertedLevel, Is.EqualTo(level));
	}

	#endregion
}
