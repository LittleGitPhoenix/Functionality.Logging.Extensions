//#region LICENSE NOTICE
////! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
//#endregion

//using System.Linq.Expressions;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

///// <summary>
///// Wrapper containing data about a logging scope.
///// </summary>
//public class LogScope : Dictionary<string, object?>, Phoenix.Functionality.Logging.Base.ILogScope
//{
//#if NETCOREAPP3_0_OR_GREATER
//	/// <summary>
//	/// Constructor
//	/// </summary>
//	/// <param name="value1"> The value that will be added to the scope. </param>
//	/// <param name="name1"> The expression name of <paramref name="value1"/> obtained via 'System.Runtime.CompilerServices.CallerArgumentExpression'. </param>
//	/// <param name="value2"> See: <paramref name="name1"/>. </param>
//	/// <param name="name2"> See: <paramref name="value1"/>. </param>
//	/// <param name="value3"> See: <paramref name="name1"/>. </param>
//	/// <param name="name3"> See: <paramref name="value1"/>. </param>
//	/// <param name="value4"> See: <paramref name="name1"/>. </param>
//	/// <param name="name4"> See: <paramref name="value1"/>. </param>
//	/// <param name="value5"> See: <paramref name="name1"/>. </param>
//	/// <param name="name5"> See: <paramref name="value1"/>. </param>
//	/// <param name="value6"> See: <paramref name="name1"/>. </param>
//	/// <param name="name6"> See: <paramref name="value1"/>. </param>
//	/// <param name="value7"> See: <paramref name="name1"/>. </param>
//	/// <param name="name7"> See: <paramref name="value1"/>. </param>
//	/// <param name="value8"> See: <paramref name="name1"/>. </param>
//	/// <param name="name8"> See: <paramref name="value1"/>. </param>
//	/// <param name="value9"> See: <paramref name="name1"/>. </param>
//	/// <param name="name9"> See: <paramref name="value1"/>. </param>
//	/// <param name="value10"> See: <paramref name="name1"/>. </param>
//	/// <param name="name10"> See: <paramref name="value1"/>. </param>
//	/// <param name="cleanCallerArgument"> Should the caller argument parameter be cleaned (removes everything but the last section of a <b>dot</b> separated string). Default is <see langword="true"/>. </param>
//	/// <returns> The logging scope. </returns>
//	/// <exception cref="ArgumentNullException"> Is thrown if any name could not be automatically obtained while its value is specified. </exception>
//	public LogScope
//	(
//		object? value1,
//		object? value2 = null,
//		object? value3 = null,
//		object? value4 = null,
//		object? value5 = null,
//		object? value6 = null,
//		object? value7 = null,
//		object? value8 = null,
//		object? value9 = null,
//		object? value10 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = null,
//		bool cleanCallerArgument = true
//	)
//		: base
//		(
//			LogScopeBuilder.BuildScopeDictionary
//			(
//				value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
//				name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
//				cleanCallerArgument
//			)
//		) { }
//#endif

//	/// <summary>
//	/// Constructor
//	/// </summary>
//	/// <param name="scopedValues"> Collection of named values. </param>
//	public LogScope(params (string Identifier, object? Value)[] scopedValues)
//		: base(LogScopeBuilder.BuildScopeDictionary(scopedValues)) { }

//	/// <summary>
//	/// Constructor
//	/// </summary>
//	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
//	public LogScope(params Expression<Func<object>>[] scopedValues)
//		: base(LogScopeBuilder.BuildScopeDictionary(scopedValues)) { }

//	/// <summary>
//	/// Constructor
//	/// </summary>
//	/// <param name="dictionary"> A dictionary of scope values. </param>
//	/// <remarks>
//	/// <para> This constructor can be used to directly pass a <see cref="Dictionary{TKey,TValue}"/> to the base class of this class. </para>
//	/// <para> It is required as otherwise the constructor with the object parameters would be used, leading to the scope's key/value pairs being merged into a single entry in a new dictionary created by that constructor. </para>
//	/// </remarks>
//	protected LogScope(IDictionary<string, object?> dictionary)
//		: base(dictionary) { }
//}

///// <summary>
///// A logging scope that is aware of the execution context.
///// </summary>
///// <remarks>
///// This class is just a marker class that indicates that the scope should flow with the execution context. If the scope is actually bound to the execution context depends on the actual logging implementation.
///// </remarks>
//public class ExecutionContextAwareLogScope : LogScope, Phoenix.Functionality.Logging.Base.IExecutionContextAwareLogScope
//{
//	/// <summary>
//	/// Initializes a new instance of the <see cref="ExecutionContextAwareLogScope"/> class by copying the values from an existing <see cref="Phoenix.Functionality.Logging.Base.ILogScope"/>.
//	/// </summary>
//	/// <param name="otherScope"> The <see cref="Phoenix.Functionality.Logging.Base.ILogScope"/> from which to copy the scope values. </param>
//	public ExecutionContextAwareLogScope(Phoenix.Functionality.Logging.Base.ILogScope otherScope) : base(otherScope) { }
//}

/// <summary>
/// A logging scope applied to a whole <see cref="LoggerGroup"/> identified by <typeparamref name="TIdentifier"/>.
/// </summary>
/// <typeparam name="TIdentifier"> The type of the group identifier. </typeparam>
[Obsolete($"This class is only available so that the signatures of other obsolete meethods that were using it are still vaild.")]
public class LogScope<TIdentifier> /*: LogScope, Phoenix.Functionality.Logging.Base.ILogScope*/
	where TIdentifier : notnull
{
//	/// <summary> The group identifier. </summary>
//	internal TIdentifier Identifier { get; }

//#if NETCOREAPP3_0_OR_GREATER
//	/// <inheritdoc />
//	public LogScope
//	(
//		TIdentifier groupIdentifier,
//		object? value1,
//		object? value2 = null,
//		object? value3 = null,
//		object? value4 = null,
//		object? value5 = null,
//		object? value6 = null,
//		object? value7 = null,
//		object? value8 = null,
//		object? value9 = null,
//		object? value10 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value1")] string? name1 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value2")] string? name2 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value3")] string? name3 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value4")] string? name4 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value5")] string? name5 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value6")] string? name6 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value7")] string? name7 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value8")] string? name8 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value9")] string? name9 = null,
//		[System.Runtime.CompilerServices.CallerArgumentExpression("value10")] string? name10 = null,
//		bool cleanCallerArgument = true
//	)
//		: base
//		(
//			LogScopeBuilder.BuildScopeDictionary
//			(
//				value1, value2, value3, value4, value5, value6, value7, value8, value9, value10,
//				name1, name2, name3, name4, name5, name6, name7, name8, name9, name10,
//				cleanCallerArgument
//			)
//		)
//	{
//		this.Identifier = groupIdentifier;
//	}

//#endif
//	/// <inheritdoc />
//	public LogScope(TIdentifier groupIdentifier, params (string Identifier, object? Value)[] scopedValues)
//		: base(scopedValues)
//	{
//		this.Identifier = groupIdentifier;
//	}

//	/// <inheritdoc />
//	public LogScope(TIdentifier groupIdentifier, params Expression<Func<object>>[] scopedValues)
//		: base(scopedValues)
//	{
//		this.Identifier = groupIdentifier;
//	}

//	/// <summary>
//	/// Constructor
//	/// </summary>
//	/// <param name="groupIdentifier"> The group identifier of type <typeparamref name="TIdentifier"/>. </param>
//	/// <param name="dictionary"> A dictionary of scope values. </param>
//	/// <remarks>
//	/// <para> This constructor can be used to directly pass a <see cref="Dictionary{TKey,TValue}"/> to the base class of this class. </para>
//	/// <para> It is required as otherwise the constructor with the object parameters would be used, leading to the scope's key/value pairs being merged into a single entry in a new dictionary created by that constructor. </para>
//	/// </remarks>
//	protected LogScope(TIdentifier groupIdentifier, IDictionary<string, object?> dictionary)
//		: base(dictionary)
//	{
//		this.Identifier = groupIdentifier;
//	}
}