using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class PayloadTest
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
	public void AfterAllTests() { }

	#endregion

	#region Data

	internal string StringProperty => "Alice";

	internal int NumericProperty => 42;

	#endregion

	#region Tests

	#region ScopeType

	[Test]
	[Category("ScopeType")]
	public void PayloadTypeIsAlwaysExecutionContextAwareWhenCreatedFromNamedTuples()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(("Key", "Value"));

		// Assert
		Assert.That(payload.Type, Is.EqualTo(LogScopeType.ExecutionContextAware));
	}

	[Test]
	[Category("ScopeType")]
	public void PayloadTypeIsAlwaysExecutionContextAwareWhenCreatedFromDictionary()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(new Dictionary<string, object?> { ["Key"] = "Value" });

		// Assert
		Assert.That(payload.Type, Is.EqualTo(LogScopeType.ExecutionContextAware));
	}

	[Test]
	[Category("ScopeType")]
	public void PayloadTypeIsAlwaysExecutionContextAwareWhenCreatedFromExpressions()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(() => this.StringProperty);

		// Assert
		Assert.That(payload.Type, Is.EqualTo(LogScopeType.ExecutionContextAware));
	}

#if NETCOREAPP3_0_OR_GREATER
	[Test]
	[Category("ScopeType")]
	public void PayloadTypeIsAlwaysExecutionContextAwareWhenCreatedFromCallerArgumentExpression()
	{
		// Arrange
		var value = _fixture.Create<string>();

		// Act
		var payload = (ILogScope) Payload.Create(value);

		// Assert
		Assert.That(payload.Type, Is.EqualTo(LogScopeType.ExecutionContextAware));
	}
#endif

	#endregion

	#region Interface

	[Test]
	[Category("Interface")]
	public void CreateReturnsIPayload()
	{
		// Arrange + Act
		var payload = Payload.Create(("Key", "Value"));

		// Assert
		Assert.That(payload, Is.InstanceOf<IPayload>());
		Assert.That(payload, Is.InstanceOf<ILogScope>());
	}

	#endregion

	#region Create From Named Tuples

	[Test]
	[Category("CreateFromNamedTuples")]
	public void CreateWithNamedTuplesContainsCorrectValues()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(("Name", "Alice"), ("Age", 30));

		// Assert
		Assert.That(payload, Has.Count.EqualTo(2));
		Assert.That(payload["Name"], Is.EqualTo("Alice"));
		Assert.That(payload["Age"], Is.EqualTo(30));
	}

	[Test]
	[Category("CreateFromNamedTuples")]
	public void CreateWithNamedTuplesSupportsNullValue()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(("NullKey", (object?) null));

		// Assert
		Assert.That(payload, Has.Count.EqualTo(1));
		Assert.That(payload["NullKey"], Is.Null);
	}

	[Test]
	[Category("CreateFromNamedTuples")]
	public void CreateWithNamedTuplesSupportsMultipleEntries()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(("A", 1), ("B", 2), ("C", 3));

		// Assert
		Assert.That(payload, Has.Count.EqualTo(3));
		Assert.That(payload["A"], Is.EqualTo(1));
		Assert.That(payload["B"], Is.EqualTo(2));
		Assert.That(payload["C"], Is.EqualTo(3));
	}

	#endregion

	#region Create From Dictionary

	[Test]
	[Category("CreateFromDictionary")]
	public void CreateWithDictionaryContainsCorrectValues()
	{
		// Arrange
		var dictionary = new Dictionary<string, object?> { ["Name"] = "Alice", ["Age"] = 30 };

		// Act
		var payload = (ILogScope) Payload.Create(dictionary);

		// Assert
		Assert.That(payload, Has.Count.EqualTo(2));
		Assert.That(payload["Name"], Is.EqualTo("Alice"));
		Assert.That(payload["Age"], Is.EqualTo(30));
	}

	[Test]
	[Category("CreateFromDictionary")]
	public void CreateWithDictionarySupportsNullValue()
	{
		// Arrange
		var dictionary = new Dictionary<string, object?> { ["NullKey"] = null };

		// Act
		var payload = (ILogScope) Payload.Create(dictionary);

		// Assert
		Assert.That(payload, Has.Count.EqualTo(1));
		Assert.That(payload["NullKey"], Is.Null);
	}

	#endregion

	#region Create From Expressions

	[Test]
	[Category("CreateFromExpressions")]
	public void CreateWithExpressionsContainsCorrectValues()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(() => this.StringProperty, () => this.NumericProperty);

		// Assert
		Assert.That(payload, Has.Count.EqualTo(2));
		Assert.That(payload[nameof(this.StringProperty)], Is.EqualTo(this.StringProperty));
		Assert.That(payload[nameof(this.NumericProperty)], Is.EqualTo(this.NumericProperty));
	}

	[Test]
	[Category("CreateFromExpressions")]
	public void CreateWithExpressionSupportsSingleEntry()
	{
		// Arrange + Act
		var payload = (ILogScope) Payload.Create(() => this.StringProperty);

		// Assert
		Assert.That(payload, Has.Count.EqualTo(1));
		Assert.That(payload[nameof(this.StringProperty)], Is.EqualTo(this.StringProperty));
	}

	#endregion

#if NETCOREAPP3_0_OR_GREATER

	#region Create From CallerArgumentExpression

	[Test]
	[Category("CreateFromCallerArgumentExpression")]
	public void CreateWithCallerArgumentExpressionContainsCorrectValues()
	{
		// Arrange
		var userName = "Alice";
		var age = 30;

		// Act
		var payload = (ILogScope) Payload.Create(userName, age);

		// Assert
		Assert.That(payload, Has.Count.EqualTo(2));
		// Dictionary access with string keys is case-sensitive, so LINQ needs to be used to check for the presence of the expected key-value pairs in a case-insensitive manner.
		Assert.That(payload.Any(pair => pair.Key.Equals("UserName", StringComparison.OrdinalIgnoreCase) && Equals(pair.Value, "Alice")), Is.True);
		Assert.That(payload.Any(pair => pair.Key.Equals("Age", StringComparison.OrdinalIgnoreCase) && Equals(pair.Value, 30)), Is.True);
	}

	[Test]
	[Category("CreateFromCallerArgumentExpression")]
	public void CreateWithCallerArgumentExpressionSupportsSingleEntry()
	{
		// Arrange
		var userId = 42;

		// Act
		var payload = (ILogScope) Payload.Create(userId);

		// Assert
		Assert.That(payload, Has.Count.EqualTo(1));
		Assert.That(payload.Any(pair => pair.Key.Equals("UserId", StringComparison.OrdinalIgnoreCase) && Equals(pair.Value, 42)), Is.True);
	}

	#endregion

#endif

	#endregion
}
