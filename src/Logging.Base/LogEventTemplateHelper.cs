#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

internal class LogEventTemplateHelper
{
#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER
	/// <summary>
	/// Validates that the specified type is a <see cref="ValueTuple"/> type and throws an <see cref="InvalidOperationException"/> if it is not.
	/// </summary>
	/// <param name="type"> The type to validate. </param>
	/// <param name="encompassingType"> The type that contains the generic parameter being validated. Used for exception messaging. </param>
	/// <exception cref="InvalidOperationException"> Thrown if type is not a <see cref="ValueTuple"/>. </exception>
	internal static void ThrowIfNotValueTupleType(Type type, Type encompassingType)
	{
		if
		(
			!type.IsValueType
			|| (type.FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == false && type != typeof(ValueTuple))
		)
			throw new InvalidOperationException($"The generic parameter '{type.FullName}' of '{encompassingType.FullName}' must be a {nameof(ValueTuple)}.");
	}
#endif

	/// <summary>
	/// Determines if the second element of a two-element tuple should be filtered out because it is a placeholder (identified by the type being <see cref="Unit"/>).
	/// </summary>
	/// <param name="tupleType"> The type of the tuple to inspect. </param>
	/// <returns> <see langword="true"/> if the second element is only a placeholder, otherwise <see langword="false"/>. </returns>
	internal static bool ShouldFilterSecondElement(Type tupleType)
	{
		// Check if TArgs is a generic type (which ValueTuples are)		
		if (!tupleType.IsGenericType) return false;

		// Get the generic type arguments of the tuple.
		var genericArguments = tupleType.GetGenericArguments();

		// Check if this is a tuple with exactly 2 elements.
		// Since tuples must contain at least two elements, more elements are on purpose.
		// If it contains two elements, it could actually mean one is a placeholder.
		if (genericArguments.Length != 2) return false;

		// Check if the second element (index 1) is of type Unit.
		return genericArguments[1] == typeof(Unit);
	}
}

internal class LogEventTemplateHelper<TArgs>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
	where TArgs : System.Runtime.CompilerServices.ITuple
#else
	where TArgs : struct
#endif
{
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static object?[] ConvertTupleToObjectArray(TArgs args) => TupleConverter.Invoke(args);

	private static readonly Func<TArgs, object?[]> TupleConverter = CreateTupleConverter();

	private static Func<TArgs, object?[]> CreateTupleConverter()
	{
		var shouldFilterSecondElement = LogEventTemplateHelper.ShouldFilterSecondElement(typeof(TArgs));

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
		return tuple =>
		{
			// Convert the value tuple to an object array by iterating over ITuple.
			// Special handling for two-element tuples: filter out placeholder second element by reducing the number of elements that are iterated.
			var argsLength = tuple.Length - (shouldFilterSecondElement ? 1 : 0);
			var argsArray = new object?[argsLength];
			for (var i = 0; i < argsLength; i++) argsArray[i] = tuple[i];
			return argsArray;
		};
#else
		// Use reflection to get the fields of the ValueTuple. This is done once and the field is captured by the returned function.
		var type = typeof(TArgs);
		var fields = type.GetFields();

		// Special handling for two-element tuples: Filter out placeholder second element by removing it from the fields that will be iterated.
		if (shouldFilterSecondElement && fields.Length == 2) fields = [fields[0]];

		// Cache field accessors for better performance.
		return tuple =>
		{
			var result = new object?[fields.Length];
			for (var i = 0; i < fields.Length; i++)
			{
				result[i] = fields[i].GetValue(tuple);
			}
			return result;
		};
#endif
	}
}