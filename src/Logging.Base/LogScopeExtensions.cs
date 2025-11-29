#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Provides extension methods for <see cref="LogScope"/>s.
/// </summary>
public static class LogScopeExtensions
{
	/// <summary>
	/// Converts the given <paramref name="scope"/> into an <see cref="Phoenix.Functionality.Logging.Base.IExecutionContextAwareLogScope"/>.
	/// </summary>
	/// <param name="scope"> The scope to convert. </param>
	/// <returns> The converted scope. </returns>
	public static Phoenix.Functionality.Logging.Base.IExecutionContextAwareLogScope ToExecutionContextAwareLogScope(this Phoenix.Functionality.Logging.Base.ILogScope scope)
	{
		if (scope is Phoenix.Functionality.Logging.Base.IExecutionContextAwareLogScope executionContextAwareLogScope) return executionContextAwareLogScope;
		return new ExecutionContextAwareLogScope(scope);
	}

	/// <summary>
	/// Converts the given <paramref name="scope"/> into an <see cref="Phoenix.Functionality.Logging.Base.IExecutionContextAwareLogScope"/>.
	/// </summary>
	/// <param name="scope"> The scope to convert. </param>
	/// <returns> The converted scope. </returns>
	public static Phoenix.Functionality.Logging.Base.IExecutionContextAwareLogScope ToExecutionContextAwareLogScope<TIdentifier>(this LogScope<TIdentifier> scope) where TIdentifier : notnull => new ExecutionContextAwareLogScope(scope);
}