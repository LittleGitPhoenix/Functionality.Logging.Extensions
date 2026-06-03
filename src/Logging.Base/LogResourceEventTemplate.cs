#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Globalization;

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Base class for a reusable template for creating <typeparamref name="TLogResourceEvent"/> instances from predefined event information obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
/// <typeparam name="TLogResourceEvent"> The type of the resource event that this template creates. </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
public abstract class LogResourceEventTemplateBase<TLogResourceEvent, TLogLevel, TEventId>
	where TLogResourceEvent : ILogResourceEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
{
	/// <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.EventId"/>"
	public TEventId EventId { get; init; }

	/// <inheritdoc cref="ILogEvent{TLogLevel,TEventId}.LogLevel"/>"
	public TLogLevel LogLevel { get; init; }

	/// <summary> The resource manager from where log and output message is obtained. </summary>
#if NETCOREAPP3_0_OR_GREATER
	public required System.Resources.ResourceManager ResourceManager { get; init; }
#else
	public System.Resources.ResourceManager ResourceManager { get; init; }
#endif

	/// <summary> The name of the resource in the <see cref="ResourceManager"/>. </summary>
#if NETCOREAPP3_0_OR_GREATER
	public required string ResourceName { get; init; }
#else
	public string ResourceName { get; init; }
#endif
	
	/// <summary>
	/// Creates a log event based on the provided parameters. The actual implementation of this method is deferred to derived classes.
	/// </summary>
	/// <param name="eventId"> The event id. </param>
	/// <param name="resourceManager"> The <see cref="System.Resources.ResourceManager"/> containing the messages. </param>
	/// <param name="resourceName"> The name of the message-resource. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="argsArray"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created log event. </returns>
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	protected internal abstract TLogResourceEvent CreateLogEvent(TEventId eventId, System.Resources.ResourceManager resourceManager, string resourceName, TLogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null);
}

/// <summary>
/// Represents a reusable template for creating <typeparamref name="TLogResourceEvent"/> instances from predefined event information obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
/// <remarks> Use this template if no placeholders are used in the message. </remarks>
/// <typeparam name="TLogResourceEvent"> The type of the resource event that this template creates. </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
public abstract class LogResourceEventTemplate<TLogResourceEvent, TLogLevel, TEventId> : LogResourceEventTemplateBase<TLogResourceEvent, TLogLevel, TEventId>
	where TLogResourceEvent : ILogResourceEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
{
	/// <summary>
	/// Builds a resource log event using the pre-defined log level and configuration.
	/// </summary>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, null, payload, null, null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(Exception exception, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, exception, payload, null, null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TLogLevel actualLogLevel, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, null, payload, null, null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TLogLevel actualLogLevel, Exception exception, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, exception, payload, null, null, actualLogCulture);
}

/// <summary>
/// Represents a reusable template for creating <typeparamref name="TLogResourceEvent"/> instances from predefined event information obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
/// <remarks>
/// Use this template if the <b>same</b> placeholders are used in the log and output message or if the output message uses no placeholders at all.
/// If only a single placeholder is used, then (due to tuples requiring at least two elements), the second element must be <see cref="Unit"/>.
/// </remarks>
/// <typeparam name="TLogResourceEvent"> The type of the resource event that this template creates. </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
/// <typeparam name="TArgs"> The type of the arguments tuple to be used with the log event. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the log message. </typeparam>
public abstract class LogResourceEventTemplate<TLogResourceEvent, TLogLevel, TEventId, TArgs> : LogResourceEventTemplateBase<TLogResourceEvent, TLogLevel, TEventId>
	where TLogResourceEvent : ILogResourceEvent<TLogLevel, TEventId>
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
	protected LogResourceEventTemplate()
	{
		// Check if the generic parameter is a value tuple.
		LogEventTemplateHelper.ThrowIfNotValueTupleType(typeof(TArgs), this.GetType());
	}
#endif

	#endregion

	#region Methods

	/// <summary>
	/// Builds a resource log event using the pre-defined log level and configuration.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, null, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, Exception exception, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, exception, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, TLogLevel actualLogLevel, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, null, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, TLogLevel actualLogLevel, Exception exception, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, exception, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	#endregion
}

/// <summary>
/// Represents a reusable template for creating <typeparamref name="TLogResourceEvent"/> instances from predefined event information obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
/// <remarks>
/// Use this template if the log message uses <b>different</b> placeholders then the output message.
/// </remarks>
/// <typeparam name="TLogResourceEvent"> The type of the resource event that this template creates. </typeparam>
/// <typeparam name="TLogLevel"> The type of the log level. </typeparam>
/// <typeparam name="TEventId"> The type of the event id. </typeparam>
/// <typeparam name="TArgs"> The type of the arguments tuple to be used with the log event. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the log message. </typeparam>
/// <typeparam name="TOutputArgs"> The type of the arguments tuple to be used to create the output message. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the output message. </typeparam>
public abstract class LogResourceEventTemplate<TLogResourceEvent, TLogLevel, TEventId, TArgs, TOutputArgs> : LogResourceEventTemplateBase<TLogResourceEvent, TLogLevel, TEventId>
	where TLogResourceEvent : ILogResourceEvent<TLogLevel, TEventId>
	where TLogLevel : struct, Enum
	where TEventId : struct
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
	where TArgs : System.Runtime.CompilerServices.ITuple
	where TOutputArgs : System.Runtime.CompilerServices.ITuple
#else
	where TArgs : struct
	where TOutputArgs : struct
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
	/// This constructor enforces that the generic type parameters <typeparamref name="TArgs"/> and <typeparamref name="TOutputArgs"/> must be a <see cref="ValueTuple"/>.
	/// Attempting to instantiate this class with a non-ValueTuple type for <typeparamref name="TArgs"/> or <typeparamref name="TOutputArgs"/>  will result in an exception.
	/// </remarks>
	/// <exception cref="InvalidOperationException"> Thrown if the generic parameter <typeparamref name="TArgs"/> or <typeparamref name="TOutputArgs"/>  is not a <see cref="ValueTuple"/>. </exception>
	protected LogResourceEventTemplate()
	{
		// Check if the generic parameter is a value tuple.
		LogEventTemplateHelper.ThrowIfNotValueTupleType(typeof(TArgs), this.GetType());
		LogEventTemplateHelper.ThrowIfNotValueTupleType(typeof(TOutputArgs), this.GetType());
	}
#endif

	#endregion

	#region Methods

	/// <summary>
	/// Builds a resource log event using the pre-defined log level and configuration.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, TOutputArgs outputArgs, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, null, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogEventTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	/// <summary>
	/// Builds a resource log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, TOutputArgs outputArgs, Exception exception, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, exception, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogEventTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, TOutputArgs outputArgs, TLogLevel actualLogLevel, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, null, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogEventTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEventSettings.LogCulture"/> will be used instead. </param>
	/// <returns> The created <typeparamref name="TLogResourceEvent"/>. </returns>
	public TLogResourceEvent Build(TArgs args, TOutputArgs outputArgs, TLogLevel actualLogLevel, Exception exception, IPayload? payload = null, CultureInfo? actualLogCulture = null)
		=> this.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, exception, payload, LogEventTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogEventTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	#endregion
}