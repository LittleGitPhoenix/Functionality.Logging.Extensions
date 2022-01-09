#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


/*!
This is required when using the C# 9 primary constructor feature in frameworks below .NET 5.
https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809
*/

#if NETSTANDARD2_0

namespace System.Runtime.CompilerServices;

internal static class IsExternalInit { }

#endif