#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Special interface used to mark scopes that should flow with the execution context.
/// </summary>
public interface IExecutionContextAwareLogScope : ILogScope;

/// <summary>
/// A logging scope that is aware of the execution context.
/// </summary>
/// <remarks>
/// This class is just a marker class that indicates that the scope should flow with the execution context. If the scope is actually bound to the execution context depends on the actual logging implementation.
/// </remarks>
public class ExecutionContextAwareLogScope : LogScope, IExecutionContextAwareLogScope
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExecutionContextAwareLogScope"/> class by copying the values from an existing <see cref="ILogScope"/>.
	/// </summary>
	/// <param name="otherScope"> The <see cref="ILogScope"/> from which to copy the scope values. </param>
	public ExecutionContextAwareLogScope(ILogScope otherScope) : base(otherScope) { }
}