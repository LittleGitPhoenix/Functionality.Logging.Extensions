using System;
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

		public bool BoolProperty => _boolProperty;
		private readonly bool _boolProperty = true;

		public int NumericProperty => _numericProperty;
		private readonly int _numericProperty = 5;

		public string StringProperty => _stringProperty;
		private readonly string _stringProperty = Guid.NewGuid().ToString();

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
	}
}