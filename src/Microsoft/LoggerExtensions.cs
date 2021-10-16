#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
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
		/// <inheritdoc cref="LoggerGroupManager.AddLoggerToGroup"/>
		public static ILogger AddToGroups(this ILogger logger, params object[] groupIdentifiers)
		{
			foreach (var groupIdentifier in groupIdentifiers)
			{
				LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier);
			}
			return logger;
		}

		/// <inheritdoc cref="LoggerGroupManager.AddLoggerToGroup"/>
		public static ILogger AddToGroup(this ILogger logger, object groupIdentifier)
		{
			return LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier);
		}

		/// <inheritdoc cref="LoggerGroupManager.GetAllGroups(ILogger)"/>
		public static IReadOnlyCollection<(object GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetGroups(this ILogger logger)
		{
			return LoggerGroupManager.GetAllGroups(logger).ToArray();
		}

		/// <inheritdoc cref="LoggerGroupManager.GetAllLoggers"/>
		public static IReadOnlyCollection<ILogger> AsGroup(this ILogger _, object groupIdentifier)
		{
			return LoggerGroupManager.GetAllLoggers(groupIdentifier).ToArray();
		}

		/// <summary>
		/// Creates a new logging scope with named values.
		/// </summary>
		/// <param name="loggers"> The collection of <see cref="ILogger"/>s that will get the scope. </param>
		/// <param name="scopedValues"> Collection of named values. </param>
		/// <returns> A <see cref="IDisposable"/> that removes the scope from all <paramref name="loggers"/> upon disposal. </returns>
		public static IDisposable CreateScope(this IReadOnlyCollection<ILogger> loggers, params (string Identifier, object? Value)[] scopedValues)
		{
			return loggers
				.Select(logger => logger.CreateScope(scopedValues))
				.ToSingleDisposable()
				;
		}

		/// <summary>
		/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
		/// </summary>
		/// <param name="loggers"> The collection of <see cref="ILogger"/>s that will get the scope. </param>
		/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
		/// <returns> A <see cref="IDisposable"/> that removes the scope from all <paramref name="loggers"/> upon disposal. </returns>
		public static IDisposable CreateScope(this IReadOnlyCollection<ILogger> loggers, params Expression<Func<object>>[] scopedValues)
		{
			return loggers
				.Select(logger => logger.CreateScope(scopedValues))
				.ToSingleDisposable()
				;
		}

		/// <summary>
		/// "Squashes" the <paramref name="disposables"/> into a single one.
		/// </summary>
		/// <param name="disposables"> The <see cref="IDisposable"/>s that will be chained together as a single one. </param>
		/// <returns> A new <see cref="IDisposable"/>. </returns>
		private static IDisposable ToSingleDisposable(this IEnumerable<IDisposable> disposables)
		{
			return new LoggerGroupManager.Disposables(disposables);
		}

		/// <summary>
		/// Creates a new logging scope with named values.
		/// </summary>
		/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
		/// <param name="scopedValues"> Collection of named values. </param>
		/// <returns> The logging scope. </returns>
		public static IDisposable CreateScope(this ILogger logger, params (string Identifier, object? Value)[] scopedValues)
		{
			var scopes = ConvertTuplesToDictionary(scopedValues);
			return logger.BeginScope(scopes);
		}

		internal static Dictionary<string, object?> ConvertTuplesToDictionary(params (string Identifier, object? Value)[] scopedValues)
		{
			var scopes = scopedValues
				.ToDictionary
				(
					tuple => tuple.Identifier,
					tuple => tuple.Value
				)
				;
			return scopes;
		}

		/// <summary>
		/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
		/// </summary>
		/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
		/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
		/// <returns> The logging scope. </returns>
		public static IDisposable CreateScope(this ILogger logger, params Expression<Func<object>>[] scopedValues)
		{
			var scopes = ConvertExpressionsToDictionary(scopedValues);
			return logger.BeginScope(scopes);
		}

		internal static Dictionary<string, object?> ConvertExpressionsToDictionary(params Expression<Func<object>>[] scopedValues)
		{
			var scopes = scopedValues
				.Select(LoggerExtensions.GetExpressionData)
				.ToDictionary
				(
					tuple => tuple.Name,
					tuple => tuple.Value
				)
				;
			return scopes;
		}

		#region Helper
		
		/// <summary>
		/// Gets the name and the value of <paramref name="scopedValue"/>.
		/// </summary>
		/// <param name="scopedValue"> The expression of the value. </param>
		/// <returns>
		/// <para> A <see cref="System.ValueTuple"/> with: </para>
		/// <para> • The name of <paramref name="scopedValue"/> as <see cref="string"/>. </para>
		/// <para> • The value of <paramref name="scopedValue"/> as <see cref="object"/>. </para>
		/// </returns>
		/// <remarks> https://stackoverflow.com/a/65110000 </remarks>
		internal static (string Name, object? Value) GetExpressionData(Expression<Func<object>> scopedValue)
		{
			var lambda = scopedValue as LambdaExpression;
			var expression = lambda.Body;

			/*
			This is for functions like:
			 • () => "Hello"	→ ("System.String", "Hello")
			 • () => 2			→ ("System.Int32", 2)
			 • () => 2 * 2		→ ("System.Int32", 4)
			*/
			if (((expression as UnaryExpression)?.Operand ?? expression) is ConstantExpression constantExpression)
			{
				return (constantExpression.Type.ToString(), constantExpression.Value);
			}

			/*
			This is for properties and members:
			 • () => _counter		→ ("Counter", 3)
			 • () => this.Counter	→ ("Counter", 3)
			 • () => _name			→ ("Name", "Bob")
			 • () => this.Name		→ ("Name", "Bob")
			 • () => this.User.Name	→ ("Name", "Bob")
			*/
			MemberExpression memberExpression = (MemberExpression) ((expression as UnaryExpression)?.Operand ?? expression);
			var name = LoggerExtensions.ToPascalCase(memberExpression.Member.Name);

			var dependencyChain = new List<MemberExpression>();
			var pointingExpression = memberExpression;
			while (pointingExpression != null)
			{
				dependencyChain.Add(pointingExpression);
				pointingExpression = pointingExpression.Expression as MemberExpression;
			}
			
			// Expression may be null if the scoped value represents a static member.
			var baseExpression = dependencyChain.Last().Expression as ConstantExpression;
			var value = baseExpression?.Value;
			for (var i = dependencyChain.Count; i > 0; i--)
			{
				var member = dependencyChain[i - 1].Member;
				if (member is PropertyInfo propertyInfo)
				{
					var isStatic = propertyInfo.GetAccessors(nonPublic: true).Any(x => x.IsStatic);
					if (!isStatic && value is null) break;
					
					value = propertyInfo.GetValue(value);
				}
				else if (member is FieldInfo fieldInfo)
				{
					if (!fieldInfo.IsStatic && value is null) break;
					value = fieldInfo.GetValue(value);
				}
			}
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

		#endregion
	}
}