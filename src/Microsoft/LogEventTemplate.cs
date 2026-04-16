#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Represents a type that has only a single value, typically used to indicate the absence of a meaningful result.
/// </summary>
/// <remarks>
/// The <see cref="Unit"/> type is commonly used in scenarios where a method or operation conceptually returns no value, such as in functional programming patterns.
/// It can be used as a placeholder for generic type parameters when no result is required.
/// </remarks>
public struct Unit
{
	/// <summary>
	/// Represents the single, default value of the <see cref="Unit"/> type.
	/// </summary>
	/// <remarks> Use this value when a method or operation requires a <see cref="Unit"/> instance, typically to indicate the absence of a meaningful result. </remarks>
	public static readonly Unit Value = new();
}

/// <summary>
/// Represents a reusable template for creating <see cref="ILogEvent"/> instances from predefined event information.
/// </summary>
public class LogEventTemplate
{
	/// <inheritdoc cref="ILogEvent.EventId"/>"
	public EventId EventId { get; init; }

	/// <inheritdoc cref="ILogEvent.LogLevel"/>"
	public LogLevel LogLevel { get; init; }

	/// <inheritdoc cref="ILogEvent.LogMessage"/>"
	public required string LogMessage { get; init; }

	/// <summary>
	/// Builds a log event using the pre-defined log level and configuration.
	/// </summary>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(ILogScope? payload = null)
		=> CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, null, payload);

	/// <summary>
	/// Builds a log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(Exception exception, ILogScope? payload = null)
		=> CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, exception, payload);

	/// <summary>
	/// Builds a log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(LogLevel actualLogLevel, ILogScope? payload = null)
		=> CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, null, payload);

	/// <summary>
	/// Builds a log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(LogLevel actualLogLevel, Exception exception, ILogScope? payload = null)
		=> CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, exception, payload);

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static ILogEvent CreateLogEvent(EventId eventId, string logMessage, LogLevel actualLogLevel, Exception? exception, ILogScope? payload, params object?[] argsArray)
	{
		return exception is null
			? new LogEvent(eventId, actualLogLevel, logMessage, argsArray) { PayLoad =  payload }
			: new LogEvent(eventId, exception, actualLogLevel, logMessage, argsArray) { PayLoad = payload }
			;
	}
}

/// <summary>
/// Represents a reusable template for creating <see cref="ILogEvent"/> instances with a strongly-typed template for creating structured log events with a fixed set of arguments.
/// </summary>
/// <remarks>
/// Use this class to define reusable log event templates with a specific argument structure, enabling efficient and type-safe logging.
/// The generic parameter enforces the shape of arguments passed to the log event, reducing runtime errors and improving code clarity.
/// </remarks>
/// <typeparam name="TArgs"> The type of the arguments tuple to be used with the log event. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the log message. </typeparam>
public class LogEventTemplate<TArgs>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
	where TArgs : System.Runtime.CompilerServices.ITuple
#else
	where TArgs : struct
#endif
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <inheritdoc cref="ILogEvent.EventId"/>"
	public EventId EventId { get; init; }

	/// <inheritdoc cref="ILogEvent.LogLevel"/>"
	public LogLevel LogLevel { get; init; }

	/// <inheritdoc cref="ILogEvent.LogMessage"/>"
	public required string LogMessage { get; init; }

	#endregion

	#region (De)Constructors

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER
	/// <summary>
	/// Constructor
	/// </summary>
	/// <remarks>
	/// This constructor enforces that the generic type parameter <typeparamref name="TArgs"/> must be a <see cref="ValueTuple"/>.
	/// Attempting to instantiate this class with a non-ValueTuple type for <typeparamref name="TArgs"/> will result in an exception.
	/// </remarks>
	/// <exception cref="InvalidOperationException"> Thrown if the generic parameter <typeparamref name="TArgs"/> is not a <see cref="ValueTuple"/>. </exception>
	public LogEventTemplate()
	{
		// Check if the generic parameter is a value tuple.
		LogTemplateHelper.ThrowIfNotValueTupleType(typeof(TArgs), this.GetType());
	}
#endif

	#endregion

	#region Methods

	/// <summary>
	/// Builds a log event using the specified arguments and the pre-defined log level.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(TArgs args, ILogScope? payload = null)
		=> LogEventTemplate.CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, null, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	/// <summary>
	/// Builds a log event containing the specified <paramref name="exception"/> using the specified arguments and the pre-defined log level.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(TArgs args, Exception exception, ILogScope? payload = null)
		=> LogEventTemplate.CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, exception, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	/// <summary>
	/// Builds a log event using the specified arguments and the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(TArgs args, LogLevel actualLogLevel, ILogScope? payload = null)
		=> LogEventTemplate.CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, null, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	/// <summary>
	/// Builds a log event containing the specified <paramref name="exception"/> using the specified arguments and the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <see cref="ILogEvent"/>. </returns>
	public ILogEvent Build(TArgs args, LogLevel actualLogLevel, Exception exception, ILogScope? payload = null)
		=> LogEventTemplate.CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, exception, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	#endregion
}

internal class LogTemplateHelper
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
	/// Determines if the second element of a two-element tuple should be filtered out because it is a placeholder (identified by the type bein <see cref="Unit"/>.
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

internal class LogTemplateHelper<TArgs>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
	where TArgs : System.Runtime.CompilerServices.ITuple
#else
	where TArgs : struct
#endif
{
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static object?[] ConvertTupleToObjectArray(TArgs args)
		=> TupleConverter.Invoke(args);
	
	private static readonly Func<TArgs, object?[]> TupleConverter = CreateTupleConverter();

	private static Func<TArgs, object?[]> CreateTupleConverter()
	{
		var shouldFilterSecondElement = LogTemplateHelper.ShouldFilterSecondElement(typeof(TArgs));

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