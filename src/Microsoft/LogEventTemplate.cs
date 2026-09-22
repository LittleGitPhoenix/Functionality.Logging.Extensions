#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Microsoft.Extensions.Logging;
using Phoenix.Functionality.Logging.Base;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Represents a reusable template for creating <see cref="ILogEvent"/> instances from predefined event information.
/// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Pure pass-through class.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class LogEventTemplate : LogEventTemplate<ILogEvent, LogLevel, EventId>
{
	/// <inheritdoc />
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	protected override ILogEvent CreateLogEvent(EventId eventId, string logMessage, LogLevel actualLogLevel, Exception? exception, IPayload? payload, params object?[] argsArray)
		=> CreateLogEvent_Internal(eventId, logMessage, actualLogLevel, exception, payload, argsArray);
	
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	internal static ILogEvent CreateLogEvent_Internal(EventId eventId, string logMessage, LogLevel actualLogLevel, Exception? exception, IPayload? payload, params object?[] argsArray)
	{
		return exception is null
			? new LogEvent(eventId, actualLogLevel, logMessage, argsArray) { Payload =  payload }
			: new LogEvent(eventId, exception, actualLogLevel, logMessage, argsArray) { Payload = payload }
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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Pure pass-through class.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public class LogEventTemplate<TArgs> : LogEventTemplate<ILogEvent, LogLevel, EventId, TArgs>
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
	protected override ILogEvent CreateLogEvent(EventId eventId, string logMessage, LogLevel actualLogLevel, Exception? exception, IPayload? payload, params object?[] argsArray)
		=> LogEventTemplate.CreateLogEvent_Internal(eventId, logMessage, actualLogLevel, exception, payload, argsArray);

	#endregion
}