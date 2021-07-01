#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft
{
	/// <summary>
	/// Provides extension methods for <see cref="ILogger"/>.
	/// </summary>
	public static partial class LoggerExtensions
	{
		/// <summary>
		/// Creates a new logging scope with named values.
		/// </summary>
		/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
		/// <param name="scopedValues"> Collection of named values. </param>
		/// <returns> The logging scope. </returns>
		public static IDisposable BeginScope(this ILogger logger, params (string Identifier, object? Value)[] scopedValues)
		{
			var scopes = scopedValues
				.ToDictionary
				(
					tuple => tuple.Identifier,
					tuple => tuple.Value
				)
				;

			return logger.BeginScope(scopes);
		}

		/// <summary>
		/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
		/// </summary>
		/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
		/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
		/// <returns> The logging scope. </returns>
		public static IDisposable BeginScope(this ILogger logger, params Expression<Func<object>>[] scopedValues)
		{
			var scopes = scopedValues
				.Select(LoggerExtensions.GetExpressionData)
				.ToDictionary
				(
					tuple => tuple.Name,
					tuple => tuple.Value
				)
				;

			return logger.BeginScope(scopes);
		}

		/// <summary>
		/// Gets the name and the value of <paramref name="scopedValue"/>.
		/// </summary>
		/// <param name="scopedValue"> The expression of the value. </param>
		/// <returns>
		/// <para> A <see cref="System.ValueTuple"/> with: </para>
		/// <para> • The name of <paramref name="scopedValue"/> as <see cref="string"/>. </para>
		/// <para> • The value of <paramref name="scopedValue"/> as <see cref="object"/>. </para>
		/// </returns>
		internal static (string Name, object? Value) GetExpressionData(Expression<Func<object>> scopedValue)
		{
			var lambda = scopedValue as LambdaExpression;

			// Get name.
			var expression = lambda.Body;
			MemberExpression memberExpression = (MemberExpression)((expression as UnaryExpression)?.Operand ?? expression);
			var name = LoggerExtensions.ToPascalCase(memberExpression.Member.Name);

			// Get value.
			expression = memberExpression.Expression;
			ConstantExpression? constantExpression = (ConstantExpression?)((expression as UnaryExpression)?.Operand ?? memberExpression.Expression ?? null);
			var value = constantExpression is null ? null : (memberExpression.Member is FieldInfo fieldInfo) ? fieldInfo.GetValue(constantExpression.Value) : (memberExpression.Member is PropertyInfo propertyInfo) ? propertyInfo.GetValue(constantExpression.Value) : null;

			return (name, value);
		}
		
		private static readonly Regex InvalidCharsRegEx = new Regex("[^_a-zA-Z0-9]", RegexOptions.Compiled);

		private static readonly Regex WhiteSpaceRegEx = new Regex(@"(?<=\s)", RegexOptions.Compiled);

		private static readonly Regex StartsWithLowerCaseCharRegEx = new Regex("^[a-z]", RegexOptions.Compiled);

		private static readonly Regex FirstCharFollowedByUpperCasesOnlyRegEx = new Regex("(?<=[A-Z])[A-Z0-9]+$", RegexOptions.Compiled);

		private static readonly Regex LowerCaseNextToNumberRegEx = new Regex("(?<=[0-9])[a-z]", RegexOptions.Compiled);

		private static readonly Regex UpperCaseInsideRegEx = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))", RegexOptions.Compiled);

		/// <summary>
		/// Modifies <paramref name="value"/> into a pascal case string.
		/// </summary>
		/// <param name="value"> The string to manipulate. </param>
		/// <returns> A new pascal case string. </returns>
		/// <remarks> https://stackoverflow.com/a/46095771 </remarks>
		internal static string ToPascalCase(string value)
		{
			// replace white spaces with underscore, then replace all invalid chars with empty string
			var pascalCase = InvalidCharsRegEx.Replace(WhiteSpaceRegEx.Replace(value, "_"), string.Empty)
				// split by underscores
				.Split(new char[] {'_'}, StringSplitOptions.RemoveEmptyEntries)
				// set first letter to uppercase
				.Select(w => StartsWithLowerCaseCharRegEx.Replace(w, m => m.Value.ToUpper()))
				// replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
				.Select(w => FirstCharFollowedByUpperCasesOnlyRegEx.Replace(w, m => m.Value.ToLower()))
				// set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
				.Select(w => LowerCaseNextToNumberRegEx.Replace(w, m => m.Value.ToUpper()))
				// lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
				.Select(w => UpperCaseInsideRegEx.Replace(w, m => m.Value.ToLower()))
				;

			return string.Concat(pascalCase);
		}
	}
}