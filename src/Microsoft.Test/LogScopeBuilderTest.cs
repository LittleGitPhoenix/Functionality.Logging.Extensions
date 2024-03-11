using System.Linq.Expressions;
using System.Text;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test;

public class LogScopeBuilderTest
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

	enum MyEnum
	{
		EnumValue,
	}

    internal bool BoolProperty => _boolField;
    private readonly bool _boolField = true;

    internal int NumericProperty => _numericField;
    private readonly int _numericField = 5;

    internal string StringProperty => _stringField;
    private readonly string _stringField = Guid.NewGuid().ToString();
		
    internal static string StaticStringProperty => StaticStringField;
    private static readonly string StaticStringField = Guid.NewGuid().ToString();

	private const string StringConstant = "Value";
		
    internal string? NullStringProperty { get; } = null;

    internal Nested NestedProperty { get; } = new Nested();

    internal class Nested
    {
        internal Guid Guid { get; } = Guid.NewGuid();
			
        internal static Guid StaticGuid { get; } = Guid.NewGuid();
    }

	#endregion

	#region Tests
	
#if NET5_0_OR_GREATER

	[Test]
    public void Check_CallerArgumentExpression_Single_Value()
    {
        // Arrange
		var message = Guid.NewGuid().ToString();

        // Act
        var scopes = LogScopeBuilder.BuildScopeDictionary(message);

        // Assert
        var pair = scopes.ElementAt(0);
        Assert.That(String.Equals(pair.Key, nameof(message), StringComparison.OrdinalIgnoreCase), Is.True);
        Assert.That(pair.Value, Is.EqualTo(message));
    }

    [Test]
	public void Check_CallerArgumentExpression_All_Values()
    {
        // Arrange
		var one = ushort.MaxValue;
		var two = StringComparison.OrdinalIgnoreCase;
		var three = new StringBuilder();
        var four = new Exception();
        var five = new object();
        var six = Guid.NewGuid().ToString();
		var seven = Guid.NewGuid().ToString();
		var eight = Guid.NewGuid().ToString();
		var nine = Guid.NewGuid().ToString();
		var ten = Guid.NewGuid().ToString();

        // Act
        var scopes = LogScopeBuilder.BuildScopeDictionary(one, two, three, four, five, six, seven, eight, nine, ten);

        // Assert
        var pair = scopes.ElementAt(0);
        Assert.That(String.Equals(pair.Key, nameof(one), StringComparison.OrdinalIgnoreCase), Is.True);
        Assert.That(pair.Value, Is.EqualTo(one));
        
		pair = scopes.ElementAt(1);
		Assert.That(String.Equals(pair.Key, nameof(two), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(two));

		pair = scopes.ElementAt(2);
		Assert.That(String.Equals(pair.Key, nameof(three), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(three));

		pair = scopes.ElementAt(3);
		Assert.That(String.Equals(pair.Key, nameof(four), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(four));

		pair = scopes.ElementAt(4);
		Assert.That(String.Equals(pair.Key, nameof(five), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(five));

		pair = scopes.ElementAt(5);
		Assert.That(String.Equals(pair.Key, nameof(six), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(six));

		pair = scopes.ElementAt(6);
		Assert.That(String.Equals(pair.Key, nameof(seven), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(seven));

		pair = scopes.ElementAt(7);
		Assert.That(String.Equals(pair.Key, nameof(eight), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(eight));

		pair = scopes.ElementAt(8);
		Assert.That(String.Equals(pair.Key, nameof(nine), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(nine));

		pair = scopes.ElementAt(9);
		Assert.That(String.Equals(pair.Key, nameof(ten), StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(pair.Value, Is.EqualTo(ten));
    }

	[Test]
	public void Check_CallerArgumentExpression_Direct()
	{
		// Arrange

		// Act
		var scopes = LogScopeBuilder.BuildScopeDictionary("Test", Guid.NewGuid());

		// Assert
		var pair = scopes.ElementAt(0);
		Assert.That(pair.Key, Is.EqualTo("Test"));
		Assert.That(pair.Value, Is.EqualTo("Test"));

		pair = scopes.ElementAt(1);
		Assert.That(pair.Key, Is.EqualTo("NewGuid"));
		Assert.That(pair.Value, Is.TypeOf<Guid>());
    }
	
	[Test]
	public void Check_CallerArgumentExpression_Enum()
	{
		// Act
		var scopes = LogScopeBuilder.BuildScopeDictionary(MyEnum.EnumValue);

		// Assert
		var pair = scopes.ElementAt(0);
		Assert.That(pair.Key, Is.EqualTo(nameof(MyEnum)));
		Assert.That(pair.Value, Is.EqualTo(MyEnum.EnumValue));
	}

	[Test]
	public void Check_CallerArgumentExpression_Is_Cleaned()
	{
		// Act
		var scopes = LogScopeBuilder.BuildScopeDictionary(this.BoolProperty, cleanCallerArgument: true);

		// Assert
		var pair = scopes.ElementAt(0);
		Assert.That(pair.Key, Is.EqualTo(nameof(this.BoolProperty)));
		Assert.That(pair.Value, Is.EqualTo(this.BoolProperty));
	}

	[Test]
	public void Check_CallerArgumentExpression_Is_Not_Cleaned()
	{
		// Act
		var scopes = LogScopeBuilder.BuildScopeDictionary(this.BoolProperty, cleanCallerArgument: false);

		// Assert
		var pair = scopes.ElementAt(0);
		Assert.That(pair.Key, Is.EqualTo($"This{nameof(this.BoolProperty)}"));
		Assert.That(pair.Value, Is.EqualTo(this.BoolProperty));
	}

#endif

    #region Expression Data

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Bool_Property()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => this.BoolProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(BoolProperty)));
        Assert.That(value, Is.EqualTo(this.BoolProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Numeric_Property()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => this.NumericProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(NumericProperty)));
        Assert.That(value, Is.EqualTo(this.NumericProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_String_Property()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => this.StringProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(StringProperty)));
        Assert.That(value, Is.EqualTo(this.StringProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Static_String_Property()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => StaticStringProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(StaticStringProperty)));
        Assert.That(value, Is.EqualTo(StaticStringProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Null_String_Property()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => this.NullStringProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(NullStringProperty)));
        Assert.That(value, Is.EqualTo(this.NullStringProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Nested_Property()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => this.NestedProperty.Guid);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(NestedProperty.Guid)));
        Assert.That(value, Is.EqualTo(this.NestedProperty.Guid));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Nested_Static_Property()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => Nested.StaticGuid);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(Nested.StaticGuid)));
        Assert.That(value, Is.EqualTo(Nested.StaticGuid));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Null_Instance()
    {
        // Act
        Nested? nullInstance = null;
        var (name, value) = LogScopeBuilder.GetExpressionData(() => nullInstance.Guid);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(Nested.Guid)));
        Assert.That(value, Is.EqualTo(null));
    }

	[Test]
	public void Check_GetExpressionData_Succeeds_For_String_Constant()
	{
		// Act
		var (name, value) = LogScopeBuilder.GetExpressionData(() => StringConstant);

		// Assert
		//! When using a constant, the name of the variable cannot be extracted, as the actual call will only contain the value of the constant.
		Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.String)}"));
		Assert.That(value, Is.EqualTo(StringConstant));
	}

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_Numeric_Value()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => 2);

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.Int32)}"));
        Assert.That(value, Is.EqualTo(2));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_Calculation_Value()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => 2 * 2);

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.Int32)}"));
        Assert.That(value, Is.EqualTo(4));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_String_Value()
    {
        // Act																		
        var (name, value) = LogScopeBuilder.GetExpressionData(() => "Hello");

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.String)}"));
        Assert.That(value, Is.EqualTo("Hello"));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_Null_Value()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => null);

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.Object)}"));
        Assert.That(value, Is.EqualTo(null));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Bool_Member()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => _boolField);

        // Assert
        Assert.That(name, Is.EqualTo("BoolField"));
        Assert.That(value, Is.EqualTo(_boolField));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Numeric_Member()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => _numericField);

        // Assert
        Assert.That(name, Is.EqualTo("NumericField"));
        Assert.That(value, Is.EqualTo(_numericField));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_String_Member()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => _stringField);

        // Assert
        Assert.That(name, Is.EqualTo("StringField"));
        Assert.That(value, Is.EqualTo(_stringField));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Static_String_Member()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => StaticStringField);

        // Assert
        Assert.That(name, Is.EqualTo("StaticStringField"));
        Assert.That(value, Is.EqualTo(StaticStringField));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Enum()
    {
        // Act
        var (name, value) = LogScopeBuilder.GetExpressionData(() => MyEnum.EnumValue);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(MyEnum)));
        Assert.That(value, Is.EqualTo(MyEnum.EnumValue));
    }

    [Test]
    [TestCase("_member", "Member")]
    [TestCase(".hidden", "Hidden")]
    [TestCase("lowercase", "Lowercase")]
    [TestCase("camelCase", "CamelCase")]
    [TestCase("PascalCase", "PascalCase")]
    [TestCase("Upper_Case_Underscore", "UpperCaseUnderscore")]
    [TestCase("ALL_UPPER_CASE_UNDERSCORE", "AllUpperCaseUnderscore")]
    [TestCase("StringProperty", "StringProperty")]
    [TestCase("Who am I?", "WhoAmI")]
    [TestCase("I ate before you got here", "IAteBeforeYouGotHere")]
    [TestCase("Hello|Who|Am|I?", "HelloWhoAmI")]
    [TestCase("Live long and prosper", "LiveLongAndProsper")]
    [TestCase("Lorem ipsum dolor...", "LoremIpsumDolor")]
    [TestCase("CoolSP", "CoolSp")]
    [TestCase("AB9CD", "Ab9Cd")]
    [TestCase("CCCTrigger", "CccTrigger")]
    [TestCase("CIRC", "Circ")]
    [TestCase("ID_SOME", "IdSome")]
    [TestCase("ID_SomeOther", "IdSomeOther")]
    [TestCase("ID_SOMEOther", "IdSomeOther")]
    [TestCase("CCC_SOME_2Phases", "CccSome2Phases")]
    [TestCase("AlreadyGoodPascalCase", "AlreadyGoodPascalCase")]
    [TestCase("999 999 99 9 ", "999999999")]
    [TestCase("1 2 3 ", "123")]
    [TestCase("1 AB cd EFDDD 8", "1AbCdEfddd8")]
    [TestCase("INVALID VALUE AND _2THINGS", "InvalidValueAnd2Things")]
    public void Check_GetExpressionData_Succeeds_For_String_Member(string value, string target)
    {
        // Act
        var actual = LogScopeBuilder.ToPascalCase(value);

        // Assert
        Assert.That(actual, Is.EqualTo(target));
    }
    
    #endregion
	
    /// <summary> Checks that <see cref="Expression"/>s are properly converted into <see cref="Dictionary{string, object}"/> to later be used for logger scope creation. </summary>
    [Test]
    public void Check_Expression_Conversion()
    {
        // Arrange
        var nested = new Nested();
        var scopedExpressions = new Expression<Func<object>>[]
        {
            () => this.StringProperty,
            () => this.BoolProperty,
            () => this.NumericProperty
        };

        // Act
        var scopedValues = LogScopeBuilder.BuildScopeDictionary(scopedExpressions);

        // Assert
        Assert.That(scopedValues, Has.Count.EqualTo(scopedExpressions.Length));
	}

	#endregion
}