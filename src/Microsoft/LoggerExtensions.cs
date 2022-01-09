#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Provides extension methods for <see cref="ILogger"/>.
/// </summary>
public static partial class LoggerExtensions
{
	/// <inheritdoc cref="LoggerGroupManager.AddLoggerToGroup{TIdentifier}"/>
	public static ILogger AddToGroups<TIdentifier>(this ILogger logger, params TIdentifier[] groupIdentifiers)
		where TIdentifier : notnull
	{
		foreach (var groupIdentifier in groupIdentifiers)
		{
			LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier);
		}
		return logger;
	}

	/// <inheritdoc cref="LoggerGroupManager.AddLoggerToGroup{TIdentifier}"/>
	public static ILogger AddToGroup<TIdentifier>(this ILogger logger, TIdentifier groupIdentifier)
		where TIdentifier : notnull
	{
		return LoggerGroupManager.AddLoggerToGroup(logger, groupIdentifier);
	}

	/// <inheritdoc cref="LoggerGroupManager.GetAllGroups(ILogger)"/>
	public static IReadOnlyCollection<(object GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetGroups(this ILogger logger)
	{
		return LoggerGroupManager.GetAllGroups(logger).ToArray();
	}

	/// <inheritdoc cref="LoggerGroupManager.GetAllLoggers{TIdentifier}"/>
	public static IReadOnlyCollection<ILogger> AsGroup<TIdentifier>(this ILogger _, TIdentifier groupIdentifier)
		where TIdentifier : notnull
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

#if NETCOREAPP3_0_OR_GREATER

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
	/// </summary>
	/// <param name="logger"> The extended <see cref="ILogger"/>. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="value2"> See: <paramref name="name1"/>. </param>
	/// <param name="name2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="value1"/>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
	public static IDisposable CreateScope
	(
		this ILogger logger,
		object? value1,
		object? value2 = default,
		object? value3 = default,
		object? value4 = default,
		object? value5 = default,
		object? value6 = default,
		object? value7 = default,
		object? value8 = default,
		object? value9 = default,
		object? value10 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default
	)
	{
		var scopes = LoggerExtensions.BuildScopeDictionary
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10
		);
		return logger.BeginScope(scopes);
	}

	internal static Dictionary<string, object?> BuildScopeDictionary
	(
		object? value1,
		object? value2 = default,
		object? value3 = default,
		object? value4 = default,
		object? value5 = default,
		object? value6 = default,
		object? value7 = default,
		object? value8 = default,
		object? value9 = default,
		object? value10 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = default,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default
	)
	{
		static void TryAdd(Dictionary<string, object?> collection, string name, object? value)
		{
			name = LoggerExtensions.ToPascalCase(name);
			if (!collection.TryAdd(name, value)) collection[name] = value;
		}

		var scopes = new Dictionary<string, object?>();

		if (name1 is not null) TryAdd(scopes, name1, value1);
		if (name2 is not null) TryAdd(scopes, name2, value2);
		if (name3 is not null) TryAdd(scopes, name3, value3);
		if (name4 is not null) TryAdd(scopes, name4, value4);
		if (name5 is not null) TryAdd(scopes, name5, value5);
		if (name6 is not null) TryAdd(scopes, name6, value6);
		if (name7 is not null) TryAdd(scopes, name7, value7);
		if (name8 is not null) TryAdd(scopes, name8, value8);
		if (name9 is not null) TryAdd(scopes, name9, value9);
		if (name10 is not null) TryAdd(scopes, name10, value10);

		return scopes;
	}

#endif


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
		MemberExpression memberExpression = (MemberExpression)((expression as UnaryExpression)?.Operand ?? expression);
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
			.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
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