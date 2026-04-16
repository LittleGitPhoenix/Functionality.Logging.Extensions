#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Globalization;
using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Represents a reusable template for creating <see cref="ILogResourceEvent"/> instances from predefined event information obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
public class LogResourceEventTemplate
{
	/// <inheritdoc cref="ILogEvent.EventId"/>"
	public EventId EventId { get; init; }

	/// <inheritdoc cref="ILogEvent.LogLevel"/>"
	public LogLevel LogLevel { get; init; }

	/// <summary> The resource manager from where log and output message is obtained. </summary>
	public required System.Resources.ResourceManager ResourceManager { get; init; }

	/// <summary> The name of the resource in the <see cref="ResourceManager"/>. </summary>
	public required string ResourceName { get; init; }

	/// <summary>
	/// Builds a resource log event using the pre-defined log level and configuration.
	/// </summary>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, null, payload, null, null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(Exception exception, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, exception, payload, null, null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(LogLevel actualLogLevel, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, null, payload, null, null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(LogLevel actualLogLevel, Exception exception, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, exception, payload, null, null, actualLogCulture);

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static ILogResourceEvent CreateLogEvent(EventId eventId, System.Resources.ResourceManager resourceManager, string resourceName, LogLevel actualLogLevel, Exception? exception, ILogScope? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
	{
		return exception is null
			? new LogResourceEvent(eventId, actualLogLevel, resourceManager, resourceName, argsArray, outputArgs, actualLogCulture) { PayLoad = payload }
			: new LogResourceEvent(eventId, exception, actualLogLevel, resourceManager, resourceName, argsArray, outputArgs, actualLogCulture) { PayLoad = payload }
			;
	}
}

/// <summary>
/// Represents a reusable template for creating <see cref="ILogResourceEvent"/> instances from predefined event information obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
/// <remarks>
/// This template should be used if the output message either has no format parameters or if it uses the same parameters as the log message.
/// </remarks>
/// <typeparam name="TArgs"> The type of the arguments tuple to be used with the log event. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the log message. </typeparam>
public class LogResourceEventTemplate<TArgs>
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

	/// <summary> The resource manager from where log and output message is obtained. </summary>
	public required System.Resources.ResourceManager ResourceManager { get; init; }

	/// <summary> The name of the resource in the <see cref="ResourceManager"/>. </summary>
	public required string ResourceName { get; init; }

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
	public LogResourceEventTemplate()
	{
		// Check if the generic parameter is a value tuple.
		LogTemplateHelper.ThrowIfNotValueTupleType(typeof(TArgs), this.GetType());
	}
#endif

	#endregion

	#region Methods

	/// <summary>
	/// Builds a resource log event using the pre-defined log level and configuration.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, null, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, Exception exception, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, exception, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, LogLevel actualLogLevel, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, null, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, LogLevel actualLogLevel, Exception exception, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, exception, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), null, actualLogCulture);

	#endregion
}

/// <summary>
/// Represents a reusable template for creating <see cref="ILogResourceEvent"/> instances from predefined event information obtained from a <see cref="System.Resources.ResourceManager"/>.
/// </summary>
/// <remarks>
/// This template should be used if the output message has different format parameters then the log message.
/// </remarks>
/// <typeparam name="TArgs"> The type of the arguments tuple to be used with the log event. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the log message. </typeparam>
/// <typeparam name="TOutputArgs"> The type of the arguments tuple to be used to create the output message. Must be a <see cref="ValueTuple"/> or a <b>System.Runtime.CompilerServices.ITuple</b>, representing the parameters to be formatted into the output message. </typeparam>
public class LogResourceEventTemplate<TArgs, TOutputArgs>
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

	/// <inheritdoc cref="ILogEvent.EventId"/>"
	public EventId EventId { get; init; }

	/// <inheritdoc cref="ILogEvent.LogLevel"/>"
	public LogLevel LogLevel { get; init; }

	/// <summary> The resource manager from where log and output message is obtained. </summary>
	public required System.Resources.ResourceManager ResourceManager { get; init; }

	/// <summary> The name of the resource in the <see cref="ResourceManager"/>. </summary>
	public required string ResourceName { get; init; }

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
	public LogResourceEventTemplate()
	{
		// Check if the generic parameter is a value tuple.
		LogTemplateHelper.ThrowIfNotValueTupleType(typeof(TArgs), this.GetType());
		LogTemplateHelper.ThrowIfNotValueTupleType(typeof(TOutputArgs), this.GetType());
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
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, TOutputArgs outputArgs, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, null, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	/// <summary>
	/// Builds a resource log event containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, TOutputArgs outputArgs, Exception exception, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, this.LogLevel, exception, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, TOutputArgs outputArgs, LogLevel actualLogLevel, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, null, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	/// <summary>
	/// Builds a resource log event using the specified <paramref name="actualLogLevel"/> containing the specified <paramref name="exception"/>.
	/// </summary>
	/// <param name="args"> The arguments used to construct the log event. </param>
	/// <param name="outputArgs"> The arguments used to construct the output event. </param>
	/// <param name="actualLogLevel"> The log level to associate with the created log event. This overrides the pre-defined <see cref="LogLevel"/>. </param>
	/// <param name="exception"> The exception to include in the log event. Cannot be <see langword="null"/>. </param>
	/// <param name="payload"> Optional payload that will be added as scope to the log event. </param>
	/// <param name="actualLogCulture"> Optional culture of the log message. If this is <see langword="null"/> <see cref="LogResourceEvent.LogCulture"/> will be used instead. </param>
	/// <returns> The created <see cref="ILogResourceEvent"/>. </returns>
	public ILogResourceEvent Build(TArgs args, TOutputArgs outputArgs, LogLevel actualLogLevel, Exception exception, ILogScope? payload = null, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent(this.EventId, this.ResourceManager, this.ResourceName, actualLogLevel, exception, payload, LogTemplateHelper<TArgs>.ConvertTupleToObjectArray(args), LogTemplateHelper<TOutputArgs>.ConvertTupleToObjectArray(outputArgs), actualLogCulture);

	#endregion
}