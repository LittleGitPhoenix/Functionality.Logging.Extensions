#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Linq.Expressions;

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Specifies the type of scope used for logging operations.
/// </summary>
/// <remarks>
/// This enumeration indicates whether an <see cref="ILogScope"/> is independent of the current execution context or if it is aware of and flows with the execution context.
/// This affects how log data is correlated across asynchronous or multi-threaded operations.
/// </remarks>
public enum LogScopeType
{
	/// <summary> Represents an <see cref="ILogScope"/> that is not influenced or controlled by external factors. </summary>
	Independent,
	/// <summary> Provides an <see cref="ILogScope"/> that is bound to the current execution context. </summary>
	ExecutionContextAware,
}

/// <summary>
/// Interface for log scopes.
/// </summary>
public interface ILogScope : IDictionary<string, object?>
{
	/// <summary> The type of the current log scope. </summary>
	public LogScopeType Type { get; }
}

/// <summary>
/// Wrapper containing data about a logging scope.
/// </summary>
public class LogScope : Dictionary<string, object?>, ILogScope
{
	#region Properties
	
	/// <inheritdoc />
	public LogScopeType Type { get; init; }
	
	#endregion

	#region Constructors

#if NETCOREAPP3_0_OR_GREATER
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
	[Obsolete($"Use one of the static factories that implicitly specify the {nameof(LogScopeType)} of the log scope. This constructor will use {nameof(LogScopeType.Independent)} as default value.")]
	public LogScope
	(
		object? value1,
		object? value2 = null,
		object? value3 = null,
		object? value4 = null,
		object? value5 = null,
		object? value6 = null,
		object? value7 = null,
		object? value8 = null,
		object? value9 = null,
		object? value10 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = null,
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
		)
	{
		this.Type = LogScopeType.Independent;
	}
#endif

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="scopedValues"> Collection of named values. </param>
	[Obsolete($"Use one of the static factories that implicitly specify the {nameof(LogScopeType)} of the log scope. This constructor will use {nameof(LogScopeType.Independent)} as default value.")]
	public LogScope(params (string Identifier, object? Value)[] scopedValues)
		: base(LogScopeBuilder.BuildScopeDictionary(scopedValues))
	{
		this.Type = LogScopeType.Independent;
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	[Obsolete($"Use one of the static factories that implicitly specify the {nameof(LogScopeType)} of the log scope. This constructor will use {nameof(LogScopeType.Independent)} as default value.")]
	public LogScope(params Expression<Func<object>>[] scopedValues)
		: base(LogScopeBuilder.BuildScopeDictionary(scopedValues))
	{
		this.Type = LogScopeType.Independent;
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="type"> The <see cref="LogScopeType"/> of the new scope. </param>
	/// <param name="dictionary"> A dictionary of scope values. </param>
	/// <remarks>
	/// <para> This constructor can be used to directly pass a <see cref="Dictionary{TKey,TValue}"/> to the base class of this class. </para>
	/// <para> It is required as otherwise the constructor with the object parameters would be used, leading to the scope's key/value pairs being merged into a single entry in a new dictionary created by that constructor. </para>
	/// </remarks>
	protected LogScope(LogScopeType type, IDictionary<string, object?> dictionary) : base(dictionary)
	{
		this.Type = type;
	}

	#endregion

	#region Factory Methods

#if NETCOREAPP3_0_OR_GREATER

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with the specified <paramref name="type"/>.
	/// </summary>
	/// <param name="type"> The <see cref="LogScopeType"/> of the new scope. </param>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
	public static ILogScope Create
	(
		LogScopeType type,
		object? value1,
		object? value2 = null,
		object? value3 = null,
		object? value4 = null,
		object? value5 = null,
		object? value6 = null,
		object? value7 = null,
		object? value8 = null,
		object? value9 = null,
		object? value10 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = null,
		bool cleanCallerArgument = true
	)
	{
		return new LogScope
		(
			type,
			LogScopeBuilder.BuildScopeDictionary
			(
				value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
				name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
				cleanCallerArgument
			)
		);
	}

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.Independent"/>.
	/// </summary>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
	public static ILogScope CreateIndependent
	(
		object? value1,
		object? value2 = null,
		object? value3 = null,
		object? value4 = null,
		object? value5 = null,
		object? value6 = null,
		object? value7 = null,
		object? value8 = null,
		object? value9 = null,
		object? value10 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = null,
		bool cleanCallerArgument = true
	)
	{
		return new LogScope
		(
			LogScopeType.Independent,
			LogScopeBuilder.BuildScopeDictionary
			(
				value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
				name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
				cleanCallerArgument
			)
		);
	}

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.ExecutionContextAware"/>.
	/// </summary>
	/// <param name="value1"> The value that will be added to the scope. </param>
	/// <param name="value2"> See: <paramref name="value1"/>. </param>
	/// <param name="value3"> See: <paramref name="value1"/>. </param>
	/// <param name="value4"> See: <paramref name="value1"/>. </param>
	/// <param name="value5"> See: <paramref name="value1"/>. </param>
	/// <param name="value6"> See: <paramref name="value1"/>. </param>
	/// <param name="value7"> See: <paramref name="value1"/>. </param>
	/// <param name="value8"> See: <paramref name="value1"/>. </param>
	/// <param name="value9"> See: <paramref name="value1"/>. </param>
	/// <param name="value10"> See: <paramref name="value1"/>. </param>
	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
	/// <param name="name2"> See: <paramref name="name1"/>. </param>
	/// <param name="name3"> See: <paramref name="name1"/>. </param>
	/// <param name="name4"> See: <paramref name="name1"/>. </param>
	/// <param name="name5"> See: <paramref name="name1"/>. </param>
	/// <param name="name6"> See: <paramref name="name1"/>. </param>
	/// <param name="name7"> See: <paramref name="name1"/>. </param>
	/// <param name="name8"> See: <paramref name="name1"/>. </param>
	/// <param name="name9"> See: <paramref name="name1"/>. </param>
	/// <param name="name10"> See: <paramref name="name1"/>. </param>
	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
	/// <returns> The logging scope. </returns>
	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
	public static ILogScope CreateAware
	(
		object? value1,
		object? value2 = null,
		object? value3 = null,
		object? value4 = null,
		object? value5 = null,
		object? value6 = null,
		object? value7 = null,
		object? value8 = null,
		object? value9 = null,
		object? value10 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = null,
		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = null,
		bool cleanCallerArgument = true
	)
	{
		return new LogScope
		(
			LogScopeType.ExecutionContextAware,
			LogScopeBuilder.BuildScopeDictionary
			(
				value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
				name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
				cleanCallerArgument
			)
		);
	}
#endif

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.Independent"/>.
	/// </summary>
	/// <param name="scopedValues"> Collection of named values. </param>
	public static ILogScope CreateIndependent(params (string Identifier, object? Value)[] scopedValues)
		=> Create(LogScopeType.Independent, scopedValues);

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.ExecutionContextAware"/>.
	/// </summary>
	/// <param name="scopedValues"> Collection of named values. </param>
	public static ILogScope CreateAware(params (string Identifier, object? Value)[] scopedValues)
		=> Create(LogScopeType.ExecutionContextAware, scopedValues);

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with the specified <paramref name="type"/>.
	/// </summary>
	/// <param name="type"> The <see cref="LogScopeType"/> of the new scope. </param>
	/// <param name="scopedValues"> Collection of named values. </param>
	public static ILogScope Create(LogScopeType type, params (string Identifier, object? Value)[] scopedValues)
		=> new LogScope(type, LogScopeBuilder.BuildScopeDictionary(scopedValues));

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.Independent"/>.
	/// </summary>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	public static ILogScope CreateIndependent(params Expression<Func<object>>[] scopedValues)
		=> Create(LogScopeType.Independent, scopedValues);

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.ExecutionContextAware"/>.
	/// </summary>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	public static ILogScope CreateAware(params Expression<Func<object>>[] scopedValues)
		=> Create(LogScopeType.ExecutionContextAware, scopedValues);

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with the specified <paramref name="type"/>.
	/// </summary>
	/// <param name="type"> The <see cref="LogScopeType"/> of the new scope. </param>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	public static ILogScope Create(LogScopeType type, params Expression<Func<object>>[] scopedValues)
		=> new LogScope(type, LogScopeBuilder.BuildScopeDictionary(scopedValues));

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.Independent"/>.
	/// </summary>
	/// <param name="dictionary"> A dictionary of scope values. </param>
	public static ILogScope CreateIndependent(IDictionary<string, object?> dictionary)
		=> new LogScope(LogScopeType.Independent, dictionary);

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with <see cref="LogScopeType.ExecutionContextAware"/>.
	/// </summary>
	/// <param name="dictionary"> A dictionary of scope values. </param>
	public static ILogScope CreateAware(IDictionary<string, object?> dictionary)
		=> new LogScope(LogScopeType.ExecutionContextAware, dictionary);

	/// <summary>
	/// Creates a new <see cref="ILogScope"/> instance with the specified <paramref name="type"/>.	
	/// </summary>
	/// <param name="type"> The <see cref="LogScopeType"/> of the new scope. </param>
	/// <param name="dictionary"> A dictionary of scope values. </param>
	public static ILogScope Create(LogScopeType type, IDictionary<string, object?> dictionary)
		=> new LogScope(type, dictionary);

	#endregion
}