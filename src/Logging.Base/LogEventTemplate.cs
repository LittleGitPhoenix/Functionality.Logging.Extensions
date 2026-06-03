#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Base class for a reusable template for creating generic <typeparamref name="TLogEvent"/> instances from predefined event information.
/// </summary>
/// <typeparam name="TLogEvent"> The type of the log event. </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
public abstract class LogEventTemplateBase<TLogEvent, TLogLevel, TEventId>
	where TLogEvent : ILogEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.EventId"/>"
	public TEventId EventId { get; init; }

	/// <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogLevel"/>"
	public TLogLevel LogLevel { get; init; }

	/// <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogMessage"/>"
#if NETCOREAPP3_0_OR_GREATER
	public required string LogMessage { get; init; }
#else
	public string LogMessage { get; init; }
#endif

	#endregion

	#region (De)Constructors
	#endregion

	#region Methods
	
	/// <summary>
	/// Creates a log event based on the provided parameters. The actual implementation of this method is deferred to derived classes.
	/// </summary>
	/// <param name="eventId"> The event id. </param>
	/// <param name="logMessage"> The log message. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="argsArray"> The arguments used to construct the log event. </param>
	/// <returns> The created log event. </returns>
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	protected abstract TLogEvent CreateLogEvent(TEventId eventId, string logMessage, TLogLevel actualLogLevel, Exception? exception, IPayload? payload, params object?[] argsArray);

	#endregion
}

/// <summary>
/// Represents a reusable template for creating generic <typeparamref name="TLogEvent"/> instances from predefined event information.
/// </summary>
/// <remarks> Use this template if no placeholders are used in the message. </remarks>
/// <typeparam name="TLogEvent"> The type of the log event. </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
public abstract class LogEventTemplate<TLogEvent, TLogLevel, TEventId> : LogEventTemplateBase<TLogEvent, TLogLevel, TEventId>
	where TLogEvent : ILogEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
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

	/// <summary>
	/// Builds a log event using the pre-defined log level and configuration.
	/// </summary>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, null, payload);

	/// <summary>
	/// Builds a log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(Exception exception, IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, exception, payload);

	/// <summary>
	/// Builds a log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(TLogLevel actualLogLevel, IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, null, payload);

	/// <summary>
	/// Builds a log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(TLogLevel actualLogLevel, Exception exception, IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, exception, payload);

	#endregion
}

/// <summary>
/// Represents a reusable template for creating <typeparamref name="TLogEvent"/> instances with a strongly-typed template for creating structured log events with a fixed set of arguments.
/// </summary>
/// <remarks>
/// Use this template if placeholders are used in the message. If only a single placeholder is used, then (due to tuples requiring at least two elements), the second element must be <see cref="Unit"/>.
/// The generic parameter enforces the shape of arguments passed to the log event, reducing runtime errors and improving code clarity.
/// </remarks>
/// <typeparam name="TLogEvent"> The type of the log event. </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
/// <typeparam name="TArgs"> The type of the arguments tuple to be used with the log event. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the log message. </typeparam>
public abstract class LogEventTemplate<TLogEvent, TLogLevel, TEventId, TArgs> : LogEventTemplateBase<TLogEvent, TLogLevel, TEventId>
	where TLogEvent : ILogEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
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
	protected LogEventTemplate()
	{
		// Check if the generic parameter is a value tuple.
		LogEventTemplateHelper.ThrowIfNotValueTupleType(typeof(TArgs), this.GetType());
	}
#endif

	#endregion

	#region Methods

	/// <summary>
	/// Builds a log event using the specified arguments and the pre-defined log level.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(TArgs args, IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, null, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	/// <summary>
	/// Builds a log event containing the specified <paramref name="exception"/> using the specified arguments and the pre-defined log level.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(TArgs args, Exception exception, IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, this.LogLevel, exception, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	/// <summary>
	/// Builds a log event using the specified arguments and the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(TArgs args, TLogLevel actualLogLevel, IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, null, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	/// <summary>
	/// Builds a log event containing the specified <paramref name="exception"/> using the specified arguments and the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <returns> The created <typeparamref name="TLogEvent"/>. </returns>
	public TLogEvent Build(TArgs args, TLogLevel actualLogLevel, Exception exception, IPayload? payload = null)
		=> this.CreateLogEvent(this.EventId, this.LogMessage, actualLogLevel, exception, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args));

	#endregion
}