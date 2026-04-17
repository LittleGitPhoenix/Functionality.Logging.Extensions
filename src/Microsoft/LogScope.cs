#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// A logging scope applied to a whole <see cref="LoggerGroup"/> identified by <typeparamref name="TIdentifier"/>.
/// </summary>
/// <typeparam name="TIdentifier"> The type of the group identifier. </typeparam>
[Obsolete($"This class is only available so that the signatures of other obsolete methods that were using it are still valid.")]
public class LogScope<TIdentifier> where TIdentifier : notnull;