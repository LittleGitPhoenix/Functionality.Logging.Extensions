//#region LICENSE NOTICE
////! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
//#endregion

//namespace Phoenix.Functionality.Logging.Base;

///// <summary>
///// Provides extension methods for <see cref="LogScope"/>s.
///// </summary>
//public static class LogScopeExtensions
//{
//	/// <summary>
//	/// Converts the given <paramref name="scope"/> into an <see cref="IExecutionContextAwareLogScope"/>.
//	/// </summary>
//	/// <param name="scope"> The scope to convert. </param>
//	/// <returns> The converted scope. </returns>
//	public static IExecutionContextAwareLogScope ToExecutionContextAwareLogScope(this ILogScope scope)
//	{
//		if (scope is IExecutionContextAwareLogScope executionContextAwareLogScope) return executionContextAwareLogScope;
//		return new ExecutionContextAwareLogScope(scope);
//	}
//}