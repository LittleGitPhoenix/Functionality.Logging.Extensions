#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Represents a type that has only a single value, typically used to indicate the absence of a meaningful result.
/// </summary>
/// <remarks>
/// The <see cref="Unit"/> type is commonly used in scenarios where a method or operation conceptually returns no value, such as in functional programming patterns.
/// It can be used as a placeholder for generic type parameters when no result is required.
/// </remarks>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Marker value type without executable behavior.")]
#else
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
public struct Unit
{
	/// <summary>
	/// Represents the single, default value of the <see cref="Unit"/> type.
	/// </summary>
	/// <remarks> Use this value when a method or operation requires a <see cref="Unit"/> instance, typically to indicate the absence of a meaningful result. </remarks>
	public static readonly Unit Value = new();
}