#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Autofac;
using Autofac.Builder;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft.Autofac;

/// <summary>
/// Contains extension methods for <see cref="ContainerBuilder"/>.
/// </summary>
public static class RegistrationBuilderExtensions
{
	#region WithLogger
	
	/// <summary>
	/// Configures the usage of a named <see cref="ILogger"/> parameter.
	/// </summary>
	/// <typeparam name="TLimit"> Registration limit type. </typeparam>
	/// <typeparam name="TReflectionActivatorData"> Activator data type. </typeparam>
	/// <typeparam name="TStyle"> Registration style. </typeparam>
	/// <param name="registration"> Registration to set parameter on. </param>
	/// <param name="loggerName"> The name of the previously registered <see cref="ILogger"/> service. </param>
	/// <returns> A registration builder allowing further configuration of the component. </returns>
	public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithLogger<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, string loggerName)
		where TReflectionActivatorData : ReflectionActivatorData
	{
		return registration.WithParameter
		(
			new global::Autofac.Core.ResolvedParameter
			(
				(pi, context) => pi.ParameterType == typeof(ILogger),
				(pi, context) =>
				{
					var logger = context.ResolveNamed<ILogger>(loggerName);
					return logger;
				}
			)
		);
	}

	/// <summary>
	/// Configures the usage of a configurable <see cref="ILogger"/> parameter.
	/// </summary>
	/// <typeparam name="TLimit"> Registration limit type. </typeparam>
	/// <typeparam name="TReflectionActivatorData"> Activator data type. </typeparam>
	/// <typeparam name="TStyle"> Registration style. </typeparam>
	/// <param name="registration"> Registration to set parameter on. </param>
	/// <param name="loggerModificationCallback"> Callback to use for modifying the resolved <see cref="ILogger"/> instance. </param>
	/// <returns> A registration builder allowing further configuration of the component. </returns>
	public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithLogger<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, Action<ILogger> loggerModificationCallback)
		where TReflectionActivatorData : ReflectionActivatorData
	{
		return registration.WithParameter
		(
			new global::Autofac.Core.ResolvedParameter
			(
				(pi, context) => pi.ParameterType == typeof(ILogger),
				(pi, context) =>
				{
					var logger = context.Resolve<ILogger>();
					loggerModificationCallback.Invoke(logger);
					return logger;
				}
			)
		);
	}

	/// <summary>
	/// Configures the usage of a configurable and swappable <see cref="ILogger"/> parameter.
	/// </summary>
	/// <typeparam name="TLimit"> Registration limit type. </typeparam>
	/// <typeparam name="TReflectionActivatorData"> Activator data type. </typeparam>
	/// <typeparam name="TStyle"> Registration style. </typeparam>
	/// <param name="registration"> Registration to set parameter on. </param>
	/// <param name="loggerModificationCallback"> Callback to use for modifying or completely changing the resolved <see cref="ILogger"/> instance. </param>
	/// <returns> A registration builder allowing further configuration of the component. </returns>
	public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithLogger<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration, Func<ILogger, ILogger> loggerModificationCallback)
		where TReflectionActivatorData : ReflectionActivatorData
	{
		return registration.WithParameter
		(
			new global::Autofac.Core.ResolvedParameter
			(
				(pi, context) => pi.ParameterType == typeof(ILogger),
				(pi, context) =>
				{
					var logger = context.Resolve<ILogger>();
					return loggerModificationCallback.Invoke(logger);
				}
			)
		);
	}

	#endregion
}