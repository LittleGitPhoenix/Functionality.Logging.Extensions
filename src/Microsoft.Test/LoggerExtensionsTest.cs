using System.Linq.Expressions;
using System.Net;
using System.Text;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;
using LoggerExtensions = Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerExtensions;

namespace Microsoft.Test;

public class LoggerExtensionsTest
{
    #region Data

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
    private IFixture _fixture;
#pragma warning restore 8618

    internal bool BoolProperty => _boolField;
    private readonly bool _boolField = true;

    internal int NumericProperty => _numericField;
    private readonly int _numericField = 5;

    internal string StringProperty => _stringField;
    private readonly string _stringField = Guid.NewGuid().ToString();
		
    internal static string StaticStringProperty => _staticStringField;
    private static readonly string _staticStringField = Guid.NewGuid().ToString();
		
    internal string? NullStringProperty { get; } = null;

    internal Nested NestedProperty { get; } = new Nested();

    internal class Nested
    {
        internal Guid Guid { get; } = Guid.NewGuid();
			
        internal static Guid StaticGuid { get; } = Guid.NewGuid();
    }

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    #endregion

#if NET5_0_OR_GREATER

    [Test]
    public void Check_CallerArgumentExpression_Single_Value()
    {
        // Arrange
		var message = Guid.NewGuid().ToString();

        // Act
        var scopes = LoggerExtensions.BuildScopeDictionary(message);

        // Assert
        var pair = scopes.ElementAt(0);
        Assert.True(String.Equals(pair.Key, nameof(message), StringComparison.OrdinalIgnoreCase));
        Assert.AreEqual(pair.Value, message);
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
        var scopes = LoggerExtensions.BuildScopeDictionary(one, two, three, four, five, six, seven, eight, nine, ten);

        // Assert
        var pair = scopes.ElementAt(0);
        Assert.True(String.Equals(pair.Key, nameof(one), StringComparison.OrdinalIgnoreCase));
        Assert.AreEqual(pair.Value, one);
        
		pair = scopes.ElementAt(1);
		Assert.True(String.Equals(pair.Key, nameof(two), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, two);

		pair = scopes.ElementAt(2);
		Assert.True(String.Equals(pair.Key, nameof(three), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, three);

		pair = scopes.ElementAt(3);
		Assert.True(String.Equals(pair.Key, nameof(four), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, four);

		pair = scopes.ElementAt(4);
		Assert.True(String.Equals(pair.Key, nameof(five), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, five);

		pair = scopes.ElementAt(5);
		Assert.True(String.Equals(pair.Key, nameof(six), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, six);

		pair = scopes.ElementAt(6);
		Assert.True(String.Equals(pair.Key, nameof(seven), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, seven);

		pair = scopes.ElementAt(7);
		Assert.True(String.Equals(pair.Key, nameof(eight), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, eight);

		pair = scopes.ElementAt(8);
		Assert.True(String.Equals(pair.Key, nameof(nine), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, nine);

		pair = scopes.ElementAt(9);
		Assert.True(String.Equals(pair.Key, nameof(ten), StringComparison.OrdinalIgnoreCase));
		Assert.AreEqual(pair.Value, ten);
    }

	[Test]
	public void Check_CallerArgumentExpression_Direct()
	{
		// Arrange

		// Act
		var scopes = LoggerExtensions.BuildScopeDictionary("Test", Guid.NewGuid());

		// Assert
		var pair = scopes.ElementAt(0);
		Assert.AreEqual(pair.Key, "Test");
		Assert.AreEqual(pair.Value, "Test");

		pair = scopes.ElementAt(1);
		Assert.AreEqual(pair.Key, "GuidNewGuid");
		Assert.That(pair.Value, Is.TypeOf<Guid>());
    }

#endif

    #region Expression Data

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Bool_Property()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => this.BoolProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(BoolProperty)));
        Assert.That(value, Is.EqualTo(this.BoolProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Numeric_Property()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => this.NumericProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(NumericProperty)));
        Assert.That(value, Is.EqualTo(this.NumericProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_String_Property()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => this.StringProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(StringProperty)));
        Assert.That(value, Is.EqualTo(this.StringProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Static_String_Property()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => LoggerExtensionsTest.StaticStringProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(LoggerExtensionsTest.StaticStringProperty)));
        Assert.That(value, Is.EqualTo(LoggerExtensionsTest.StaticStringProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Null_String_Property()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => this.NullStringProperty);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(NullStringProperty)));
        Assert.That(value, Is.EqualTo(this.NullStringProperty));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Nested_Property()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => this.NestedProperty.Guid);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(NestedProperty.Guid)));
        Assert.That(value, Is.EqualTo(this.NestedProperty.Guid));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Nested_Static_Property()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => LoggerExtensionsTest.Nested.StaticGuid);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(LoggerExtensionsTest.Nested.StaticGuid)));
        Assert.That(value, Is.EqualTo(LoggerExtensionsTest.Nested.StaticGuid));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Null_Instance()
    {
        // Act
        Nested? nullInstance = null;
        var (name, value) = LoggerExtensions.GetExpressionData(() => nullInstance.Guid);

        // Assert
        Assert.That(name, Is.EqualTo(nameof(Nested.Guid)));
        Assert.That(value, Is.EqualTo(null));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_Numeric_Value()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => 2);

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.Int32)}"));
        Assert.That(value, Is.EqualTo(2));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_Calculation_Value()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => 2 * 2);

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.Int32)}"));
        Assert.That(value, Is.EqualTo(4));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_String_Value()
    {
        // Act																		
        var (name, value) = LoggerExtensions.GetExpressionData(() => "Hello");

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.String)}"));
        Assert.That(value, Is.EqualTo("Hello"));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Direct_Null_Value()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => null);

        // Assert
        Assert.That(name, Is.EqualTo($"{nameof(System)}.{nameof(System.Object)}"));
        Assert.That(value, Is.EqualTo(null));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Bool_Member()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => _boolField);

        // Assert
        Assert.That(name, Is.EqualTo("BoolField"));
        Assert.That(value, Is.EqualTo(_boolField));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Numeric_Member()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => _numericField);

        // Assert
        Assert.That(name, Is.EqualTo("NumericField"));
        Assert.That(value, Is.EqualTo(_numericField));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_String_Member()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => _stringField);

        // Assert
        Assert.That(name, Is.EqualTo("StringField"));
        Assert.That(value, Is.EqualTo(_stringField));
    }

    [Test]
    public void Check_GetExpressionData_Succeeds_For_Static_String_Member()
    {
        // Act
        var (name, value) = LoggerExtensions.GetExpressionData(() => LoggerExtensionsTest._staticStringField);

        // Assert
        Assert.That(name, Is.EqualTo("StaticStringField"));
        Assert.That(value, Is.EqualTo(LoggerExtensionsTest._staticStringField));
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
        var actual = LoggerExtensions.ToPascalCase(value);

        // Assert
        Assert.That(actual, Is.EqualTo(target));
    }
    
    #endregion

    /// <summary> Checks that <see cref="System.ValueTuple"/>s are properly converted into <see cref="Dictionary{string, object}"/> to later be used for logger scope creation. </summary>
    [Test]
    public void Check_Tuple_Conversion()
    {
        // Arrange
        var scopedTuples = new (string Identifier, object? Value)[]
        {
            ("Value", this.StringProperty),
            ("Bool", this.BoolProperty),
            ("Number", this.NumericProperty),
            ("Object", null),
        };

        // Act
        var scopedValues = LoggerExtensions.ConvertTuplesToDictionary(scopedTuples);

        // Assert
        Assert.That(scopedValues, Has.Count.EqualTo(scopedTuples.Length));
        Assert.That(scopedValues.Keys, Is.EqualTo(scopedTuples.Select(tuple => tuple.Identifier)));
        Assert.That(scopedValues.Values, Is.EqualTo(scopedTuples.Select(tuple => tuple.Value)));
    }

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
        var scopedValues = LoggerExtensions.ConvertExpressionsToDictionary(scopedExpressions);

        // Assert
        Assert.That(scopedValues, Has.Count.EqualTo(scopedExpressions.Length));
    }
}