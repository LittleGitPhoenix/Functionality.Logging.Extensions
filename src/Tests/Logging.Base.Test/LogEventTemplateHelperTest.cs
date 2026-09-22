using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class LogEventTemplateHelperTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Data
	#endregion

	#region Tests

	#region ShouldFilterSecondElement

	[Test]
	public void SuperfluousGenericParameterIsDetected()
	{
		SuperfluousGenericParameterIsDetected((123, "Something"), false);
		SuperfluousGenericParameterIsDetected((123, Unit.Value), true);
	}

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
	private static void SuperfluousGenericParameterIsDetected(System.Runtime.CompilerServices.ITuple tuple, bool target)
#else
	private static void SuperfluousGenericParameterIsDetected<T>(T tuple, bool target) where T : struct
#endif
	{
		// Act
		var actual = LogEventTemplateHelper.ShouldFilterSecondElement(tuple.GetType());

		// Assert
		Assert.That(actual, Is.EqualTo(target));
	}

	[Test]
	[Category("ShouldFilterSecondElement")]
	public void NonGenericTypeIsNotFiltered()
	{
		// Arrange + Act
		var actual = LogEventTemplateHelper.ShouldFilterSecondElement(typeof(int));

		// Assert
		Assert.That(actual, Is.False);
	}

	[Test]
	[Category("ShouldFilterSecondElement")]
	public void TupleWithMoreThanTwoElementsIsNotFiltered()
	{
		// Arrange + Act
		var actual = LogEventTemplateHelper.ShouldFilterSecondElement(typeof((int, string, bool)));

		// Assert
		Assert.That(actual, Is.False);
	}

	#endregion

	#region ConvertTupleToObjectArray

	[Test]
	[Category("ConvertTupleToObjectArray")]
	public void TupleIsConvertedToObjectArray()
	{
		// Arrange
		var args = (First: 1, Second: "two", Third: true);

		// Act
		var result = LogEventTemplateHelper<(int, string, bool)>.ConvertTupleToObjectArray(args);

		// Assert: Check that the resulting array has the correct length and contains the expected values in the correct order.
		Assert.That(result, Has.Length.EqualTo(3));
		Assert.That(result[0], Is.EqualTo(1));
		Assert.That(result[1], Is.EqualTo("two"));
		Assert.That(result[2], Is.EqualTo(true));
	}

	[Test]
	[Category("ConvertTupleToObjectArray")]
	public void UnitPlaceholderIsFilteredFromConvertedObjectArray()
	{
		// Arrange
		var args = (Value: 42, Unit.Value);

		// Act
		var result = LogEventTemplateHelper<(int, Unit)>.ConvertTupleToObjectArray(args);

		// Assert
		Assert.That(result, Has.Length.EqualTo(1));
		Assert.That(result[0], Is.EqualTo(42));
	}

	[Test]
	[Category("ConvertTupleToObjectArray")]
	public void UnitIsNotFilteredWhenNotSecondAndLastElement()
	{
		// Arrange: Create a tuple where all elements are of type `Unit`. If this is even an actual case is questionable, but it should be handled correctly by the method.
		var args = (Unit.Value, Unit.Value, Unit.Value);

		// Act
		var result = LogEventTemplateHelper<(Unit, Unit, Unit)>.ConvertTupleToObjectArray(args);

		// Assert
		Assert.That(result, Has.Length.EqualTo(3));
	}
	
	#endregion

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER

	#region ThrowIfNotValueTupleType

	[Test]
	[Category("ThrowIfNotValueTupleType")]
	public void ThrowIfNotValueTupleTypeThrowsForNonTupleType()
	{
		// Arrange + Act + Assert
		Assert.Throws<InvalidOperationException>(() => LogEventTemplateHelper.ThrowIfNotValueTupleType(typeof(int), typeof(object)));
	}

	[Test]
	[Category("ThrowIfNotValueTupleType")]
	public void ThrowIfNotValueTupleTypeDoesNotThrowForValueTupleType()
	{
		// Arrange + Act + Assert
		Assert.DoesNotThrow(() => LogEventTemplateHelper.ThrowIfNotValueTupleType(typeof((int, string)), typeof(object)));
	}

	#endregion

#endif

	#endregion
}