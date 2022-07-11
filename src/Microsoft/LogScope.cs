using System.Linq.Expressions;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Wrapper containing data about a logging scope.
/// </summary>
public class LogScope : Dictionary<string, object?>
{
#if NETCOREAPP3_0_OR_GREATER

	/// <summary>
	/// Constructor
	/// </summary>
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
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <b>true</b>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
	public LogScope
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
		: base
		(
			LogScopeBuilder.BuildScopeDictionary
			(
				value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
				name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
				cleanCallerArgument
			)
		) { }
#endif

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="scopedValues"> Collection of named values. </param>
	public LogScope(params (string Identifier, object? Value)[] scopedValues)
		: base(LogScopeBuilder.BuildScopeDictionary(scopedValues)) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	public LogScope(params Expression<Func<object>>[] scopedValues)
		: base(LogScopeBuilder.BuildScopeDictionary(scopedValues)) { }
}

/// <summary>
/// Wrapper containing data about a logging scope applied to a whole <see cref="LoggerGroup"/> identified by <typeparamref name="TIdentifier"/>.
/// </summary>
/// <typeparam name="TIdentifier"> The type of the group identifier. </typeparam>
public class LogScope<TIdentifier> : LogScope
	where TIdentifier : notnull
{
	/// <summary> The group identifier. </summary>
	internal TIdentifier Identifier { get; }

#if NETCOREAPP3_0_OR_GREATER

	/// <inheritdoc cref="LogScope"/>
	public LogScope
	(
		TIdentifier groupIdentifier,
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
		: base(LogScopeBuilder.BuildScopeDictionary
		(
			value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
			name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
			cleanCallerArgument
		))
	{
		this.Identifier = groupIdentifier;
	}
#endif
	
	/// <inheritdoc cref="LogScope"/>
	public LogScope(TIdentifier groupIdentifier, params (string Identifier, object? Value)[] scopedValues)
		: base(scopedValues)
	{
		this.Identifier = groupIdentifier;
	}

	/// <inheritdoc cref="LogScope"/>
	public LogScope(TIdentifier groupIdentifier, params Expression<Func<object>>[] scopedValues)
		: base(scopedValues)
	{
		this.Identifier = groupIdentifier;
	}
}