#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Linq.Expressions;

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Marker interface for log scopes that are used as <see cref="ILogEvent{TLogLevel,TEventId}.Payload"/> for log events. This is used to easily distinguish them from other scopes and to implicitly set their <see cref="LogScopeType"/> to <see cref="LogScopeType.ExecutionContextAware"/>. As such, the values of this interface are directly linked only to a single log event and must not influence anything else (e.g. in parallel workflows).
/// </summary>
public interface IPayload : ILogScope;

/// <summary>
/// This is a specialized <see cref="LogScope"/> that is used as <see cref="ILogEvent{TLogLevel,TEventId}.Payload"/> for log events. As such it's values are directly linked only to a single log event and must not influence anything else (e.g. in parallel workflows). Therefore, its <see cref="LogScopeType"/> is implicitly set to <see cref="LogScopeType.ExecutionContextAware"/> and cannot be changed.
/// </summary>
public class Payload : LogScope, IPayload
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="dictionary"> A dictionary of scope values. </param>
	/// <remarks>
	/// <para> This constructor can be used to directly pass a <see cref="Dictionary{TKey,TValue}"/> to the base class of this class. </para>
	/// <para> It is required as otherwise the constructor with the object parameters would be used, leading to the scope's key/value pairs being merged into a single entry in a new dictionary created by that constructor. </para>
	/// </remarks>
	protected Payload(IDictionary<string, object?> dictionary) : base(LogScopeType.ExecutionContextAware, dictionary) { }

#if NETCOREAPP3_0_OR_GREATER
	/// <summary>
	/// Creates a new <see cref="IPayload"/>.
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
	public static IPayload Create
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
		return new Payload
		(
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
	/// Creates a new <see cref="IPayload"/> instance.
	/// </summary>
	/// <param name="scopedValues"> Collection of named values. </param>
	public static IPayload Create(params (string Identifier, object? Value)[] scopedValues)
		=> new Payload(LogScopeBuilder.BuildScopeDictionary(scopedValues));

	/// <summary>
	/// Creates a new <see cref="IPayload"/> instance.
	/// </summary>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	public static IPayload Create(params Expression<Func<object>>[] scopedValues)
		=> new Payload(LogScopeBuilder.BuildScopeDictionary(scopedValues));

	/// <summary>
	/// Creates a new <see cref="IPayload"/> instance.
	/// </summary>
	/// <param name="dictionary"> A dictionary of scope values. </param>
	public static IPayload Create(IDictionary<string, object?> dictionary)
		=> new Payload(dictionary);
}