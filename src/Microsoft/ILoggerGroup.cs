#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Interface bundling a collection of <see cref="ILogger"/>s.
/// </summary>
public interface ILoggerGroup : IReadOnlyCollection<ILogger>, IDisposable
{
	/// <summary>
	/// Adds the <paramref name="logger"/> to the group.
	/// </summary>
	/// <param name="logger"> The <see cref="ILogger"/> to add. </param>
	/// <param name="applyExistingScope"> Should existing scopes be applied tho the <paramref name="logger"/>. Default is <b>true</b>. </param>
	void AddLogger(ILogger logger, bool applyExistingScope);

	/// <summary>
	/// Removes the <paramref name="logger"/> from the group.
	/// </summary>
	/// <param name="logger"> The <see cref="ILogger"/> to remove. </param>
	void RemoveLogger(ILogger logger);

	/// <summary>
	/// Creates a new logging <paramref name="scope"/>.
	/// </summary>
	/// <param name="scope"> The scope to apply. </param>
	/// <returns> The logging scope. </returns>
	IDisposable CreateScope(LogScope scope);

	/// <summary>
	/// Creates a new logging scope with named values.
	/// </summary>
	/// <param name="scopedValues"> Collection of named values. </param>
	/// <returns> A <see cref="IDisposable"/> that removes the scope from all grouped loggers upon disposal. </returns>
	IDisposable CreateScope(params (string Identifier, object? Value)[] scopedValues);

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
	/// </summary>
	/// <param name="scopedValues"> The <see cref="Expression"/>s used to build the named values. </param>
	/// <returns> A <see cref="IDisposable"/> that removes the scope from all grouped loggers upon disposal. </returns>
	IDisposable CreateScope(params Expression<Func<object>>[] scopedValues);

#if NETCOREAPP3_0_OR_GREATER

	/// <summary>
	/// Creates a new logging scope with named values extracted from the given <see cref="Expression"/>s.
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
	IDisposable CreateScope
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
	);

#endif

	/// <summary>
	/// <see cref="IReadOnlyCollection{T}.Count"/>
	/// </summary>
#if NETCOREAPP3_0_OR_GREATER
	int Length => this.Count;
#else
	int Length { get; }
#endif
}