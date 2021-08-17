#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft
{
	/// <summary>
	/// Delegate returning an <see cref="ILogger"/>.
	/// </summary>
	/// <returns> A new <see cref="ILogger"/> instance. </returns>
	public delegate ILogger LoggerFactory();

	/// <summary>
	/// Delegate returning an <see cref="ILogger"/> with a given <paramref name="name"/>.
	/// </summary>
	/// <param name="name"> The name to use for the new <see cref="ILogger"/>. </param>
	/// <returns> A new <see cref="ILogger"/> instance. </returns>
	public delegate ILogger NamedLoggerFactory(string name);
}