#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

internal static class LogScopeBuilder
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties
	#endregion

	#region (De)Constructors
	#endregion

	#region Methods

#if NETCOREAPP3_0_OR_GREATER
	
	public static Dictionary<string, object?> BuildScopeDictionary
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
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = default,
		bool cleanCallerArgument = true
	)
	{
		static void TryAdd(Dictionary<string, object?> collection, string name, object? value, bool clean)
		{
			/*
			* Check if the value is an enumeration and if the name contains the value.
			* Later is necessary in cases where the enumeration value is passed from a variable instead of directly:
			* • Direct		: LogScopeBuilder.BuildScopeDictionary(MyEnum.EnumValue) ⇒ Name: MyEnum, Value: EnumValue
			* • Variable	: LogScopeBuilder.BuildScopeDictionary(myEnumVariable) ⇒ Name: MyEnumVariable, Value: EnumValue
			*/
			if (value is Enum && name.Contains(value?.ToString() ?? String.Empty))
			{
				var indexOfLastDot = name.LastIndexOf('.');
				if (indexOfLastDot != -1) name = name.Substring(0, indexOfLastDot);
			}
			if (clean) name = Clean(name);
			name = ToPascalCase(name);
			if (!collection.TryAdd(name, value)) collection[name] = value;
		}

		var scopes = new Dictionary<string, object?>();

		if (name1 is not null) TryAdd(scopes, name1, value1, cleanCallerArgument);
		if (name2 is not null) TryAdd(scopes, name2, value2, cleanCallerArgument);
		if (name3 is not null) TryAdd(scopes, name3, value3, cleanCallerArgument);
		if (name4 is not null) TryAdd(scopes, name4, value4, cleanCallerArgument);
		if (name5 is not null) TryAdd(scopes, name5, value5, cleanCallerArgument);
		if (name6 is not null) TryAdd(scopes, name6, value6, cleanCallerArgument);
		if (name7 is not null) TryAdd(scopes, name7, value7, cleanCallerArgument);
		if (name8 is not null) TryAdd(scopes, name8, value8, cleanCallerArgument);
		if (name9 is not null) TryAdd(scopes, name9, value9, cleanCallerArgument);
		if (name10 is not null) TryAdd(scopes, name10, value10, cleanCallerArgument);

		return scopes;
	}

#endif

	internal static Dictionary<string, object?> BuildScopeDictionary(params (string Identifier, object? Value)[] scopedValues)
	{
		return scopedValues
			.ToDictionary
			(
				tuple => tuple.Identifier,
				tuple => tuple.Value
			)
			;
	}

	internal static Dictionary<string, object?> BuildScopeDictionary(params Expression<Func<object>>[] scopedValues)
	{
		return scopedValues
			.Select(LogScopeBuilder.GetExpressionData)
			.ToDictionary
			(
				tuple => tuple.Name,
				tuple => tuple.Value
			)
			;
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
	/// <remarks> https://stackoverflow.com/a/65110000 </remarks>
	internal static (string Name, object? Value) GetExpressionData(Expression<Func<object>> scopedValue)
	{
		var lambda = scopedValue as LambdaExpression;
		var expression = lambda.Body;

		/*
        This is for constant expressions (e.g. constants or functions) like:
         • const string MyConstant = "Value"
		   () => MyConstant	→ ("System.String", "value")
         • () => "Hello"	→ ("System.String", "Hello")
         • () => 2			→ ("System.Int32", 2)
         • () => 2 * 2		→ ("System.Int32", 4)
        */
		if (((expression as UnaryExpression)?.Operand ?? expression) is ConstantExpression constantExpression)
		{
			if (typeof(Enum).IsAssignableFrom(constantExpression.Type))
			{
				return (constantExpression.Type.Name, constantExpression.Value);
			}
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
		var memberExpression = (MemberExpression) ((expression as UnaryExpression)?.Operand ?? expression);
		var name = ToPascalCase(memberExpression.Member.Name);

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
				if (!isStatic && value is null)
					break;

				value = propertyInfo.GetValue(value);
			}
			else if (member is FieldInfo fieldInfo)
			{
				if (!fieldInfo.IsStatic && value is null)
					break;
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
	/// Cleans <paramref name="value"/> by removing everything but the last section of a <b>dot</b> separated string.
	/// </summary>
	/// <param name="value"> The string to clean. </param>
	/// <returns> A new cleaned string. </returns>
	internal static string Clean(string value)
	{
		return value.Split('.').Last();
	}

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