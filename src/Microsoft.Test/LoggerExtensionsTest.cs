using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

namespace Microsoft.Test
{
	public class LoggerExtensionsTest
	{
		#region Data

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
		private IFixture _fixture;
#pragma warning restore 8618

		internal bool BoolProperty => _boolProperty;
		private readonly bool _boolProperty = true;

		internal int NumericProperty => _numericProperty;
		private readonly int _numericProperty = 5;

		internal string StringProperty => _stringProperty;
		private readonly string _stringProperty = Guid.NewGuid().ToString();

		internal Nested NestedProperty => _nestedProperty;
		private readonly Nested _nestedProperty = new Nested();

		internal class Nested
		{
			public Guid Guid { get; } = Guid.NewGuid();
		}

		[SetUp]
		public void Setup()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
		}

		#endregion

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
		public void Check_GetExpressionData_Succeeds_For_Nested_Property()
		{
			// Act
			var (name, value) = LoggerExtensions.GetExpressionData(() => this.NestedProperty.Guid);

			// Assert
			Assert.That(name, Is.EqualTo(nameof(NestedProperty.Guid)));
			Assert.That(value, Is.EqualTo(this.NestedProperty.Guid));
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
		public void Check_GetExpressionData_Succeeds_For_Bool_Member()
		{
			// Act
			var (name, value) = LoggerExtensions.GetExpressionData(() => _boolProperty);

			// Assert
			Assert.That(name, Is.EqualTo(nameof(BoolProperty)));
			Assert.That(value, Is.EqualTo(_boolProperty));
		}

		[Test]
		public void Check_GetExpressionData_Succeeds_For_Numeric_Member()
		{
			// Act
			var (name, value) = LoggerExtensions.GetExpressionData(() => _numericProperty);

			// Assert
			Assert.That(name, Is.EqualTo(nameof(NumericProperty)));
			Assert.That(value, Is.EqualTo(_numericProperty));
		}

		[Test]
		public void Check_GetExpressionData_Succeeds_For_String_Member()
		{
			// Act
			var (name, value) = LoggerExtensions.GetExpressionData(() => _stringProperty);

			// Assert
			Assert.That(name, Is.EqualTo(nameof(StringProperty)));
			Assert.That(value, Is.EqualTo(_stringProperty));
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
}