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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Pure pass-through class.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class LogResourceEventTemplate : LogResourceEventTemplate<ILogResourceEvent, LogLevel, EventId>
{
	/// <inheritdoc />
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	protected override ILogResourceEvent CreateLogEvent(EventId eventId, System.Resources.ResourceManager resourceManager, string resourceName, LogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
		=> CreateLogEvent_Internal(eventId, resourceManager, resourceName, actualLogLevel, exception, payload, argsArray, outputArgs, actualLogCulture);

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static ILogResourceEvent CreateLogEvent_Internal(EventId eventId, System.Resources.ResourceManager resourceManager, string resourceName, LogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
	{
		return exception is null
			? new LogResourceEvent(eventId, actualLogLevel, resourceManager, resourceName, argsArray, outputArgs, actualLogCulture) { Payload = payload }
			: new LogResourceEvent(eventId, exception, actualLogLevel, resourceManager, resourceName, argsArray, outputArgs, actualLogCulture) { Payload = payload }
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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Pure pass-through class.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class LogResourceEventTemplate<TArgs> : LogResourceEventTemplate<ILogResourceEvent, LogLevel, EventId, TArgs>
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
	#endregion

	#region Methods

	/// <inheritdoc />
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	protected override ILogResourceEvent CreateLogEvent(EventId eventId, System.Resources.ResourceManager resourceManager, string resourceName, LogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent_Internal(eventId, resourceManager, resourceName, actualLogLevel, exception, payload, argsArray, outputArgs, actualLogCulture);

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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Pure pass-through class.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class LogResourceEventTemplate<TArgs, TOutputArgs> : LogResourceEventTemplate<ILogResourceEvent, LogLevel, EventId, TArgs, TOutputArgs>
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
	#endregion

	#region Methods

	/// <inheritdoc />
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	protected override ILogResourceEvent CreateLogEvent(EventId eventId, System.Resources.ResourceManager resourceManager, string resourceName, LogLevel actualLogLevel, Exception? exception, IPayload? payload, object?[]? argsArray, object?[]? outputArgs, CultureInfo? actualLogCulture = null)
		=> LogResourceEventTemplate.CreateLogEvent_Internal(eventId, resourceManager, resourceName, actualLogLevel, exception, payload, argsArray, outputArgs, actualLogCulture);

	#endregion
}