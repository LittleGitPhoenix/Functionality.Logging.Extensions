#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Provides a singleton implementation of an empty disposable object that performs no action when disposed.
/// </summary>
/// <remarks> Use this type when an IDisposable instance is required but no resource cleanup is necessary. </remarks>
class NoDisposable : IDisposable
{
	public static NoDisposable Instance => Lazy.Value;
	private static readonly Lazy<NoDisposable> Lazy = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

	private NoDisposable() { }

	/// <inheritdoc />
	public void Dispose() { }
}