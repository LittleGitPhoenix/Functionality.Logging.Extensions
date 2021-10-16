#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft
{
	/// <summary>
	/// Manages logger groups. Should typically not be used directly but implicitly via some extension methods of <see cref="ILogger"/>.
	/// </summary>
	public static class LoggerGroupManager
	{
		#region Delegates / Events

		#endregion

		#region Constants

		#endregion

		#region Fields

		private static readonly Dictionary<object, HashSet<ILogger>> Cache;

		#endregion

		#region Properties

		#endregion

		#region (De)Constructors

		static LoggerGroupManager()
		{
			// Save parameters.

			// Initialize fields.
			Cache = new Dictionary<object, HashSet<ILogger>>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the <paramref name="logger"/> to the internal cache using the <paramref name="groupIdentifier"/>.
		/// </summary>
		/// <param name="logger"> The <see cref="ILogger"/> to add. </param>
		/// <param name="groupIdentifier"> The group identifier used when adding. </param>
		/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
		internal static ILogger AddLoggerToGroup(ILogger logger, object groupIdentifier)
		{
			if (LoggerGroupManager.Cache.TryGetValue(groupIdentifier, out var loggerCollection))
			{
				loggerCollection.Add(logger);
			}
			else
			{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
				LoggerGroupManager.Cache.Add(groupIdentifier, new HashSet<ILogger>() {logger});
#else
				LoggerGroupManager.Cache.TryAdd(groupIdentifier, new HashSet<ILogger>() {logger});
#endif
			}

			return logger;
		}

		/// <summary>
		/// Returns all cached <see cref="ILogger"/>s for the given <paramref name="groupIdentifier"/>.
		/// </summary>
		/// <param name="groupIdentifier"> The group identifier of the cached loggers. </param>
		/// <returns> A collection of <see cref="ILogger"/>s sharing the same group. </returns>
		public static IReadOnlyCollection<ILogger> GetAllLoggers(object groupIdentifier) =>
			LoggerGroupManager
				.GetAllGroups(group => group.GroupIdentifier == groupIdentifier)
				.FirstOrDefault()
				.LoggerCollection
				.ToArray();

		/// <summary>
		/// Return all groups.
		/// </summary>
		/// <returns> A collection of all cached groups. </returns>
		public static IReadOnlyCollection<(object GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetAllGroups() => LoggerGroupManager.GetAllGroups(_ => true).ToArray();

		/// <summary>
		/// Return all groups that the <paramref name="logger"/> is a part of.
		/// </summary>
		/// <param name="logger"> The <see cref="ILogger"/> whose groups to get.. </param>
		/// <returns> A collection of groups, where the <paramref name="logger"/> is a part of. </returns>
		public static IReadOnlyCollection<(object GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetAllGroups(ILogger logger) => LoggerGroupManager.GetAllGroups(group => group.LoggerCollection.Contains(logger)).ToArray();

		/// <summary>
		/// Return all groups matching the <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate"> The <see cref="Predicate{T}"/> to match. </param>
		/// <returns> A collection of groups, that match the <paramref name="predicate"/>. </returns>
		private static IEnumerable<(object GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetAllGroups(Predicate<(object GroupIdentifier, HashSet<ILogger> LoggerCollection)> predicate)
		{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
			foreach (var group in LoggerGroupManager.Cache)
			{
				var tuple = (group.Key, group.Value);
#else
			foreach (var (groupIdentifier, loggerCollection) in LoggerGroupManager.Cache)
			{
				var tuple = (groupIdentifier, loggerCollection);
#endif
				if (predicate.Invoke(tuple)) yield return tuple;
			}
		}

		#endregion

		#region Nested Types

		internal sealed class Disposables : IDisposable
		{
			#region Delegates / Events

			#endregion

			#region Constants

			#endregion

			#region Fields

			private readonly HashSet<IDisposable> _disposables;

			#endregion

			#region Properties

			#endregion

			#region (De)Constructors

			public Disposables(IEnumerable<IDisposable> disposables)
			{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
				_disposables = new HashSet<IDisposable>(disposables);
#else
				_disposables = disposables.ToHashSet();
#endif
			}

			#endregion

			#region Methods

			#region IDisposable

			/// <inheritdoc />
			public void Dispose()
			{
				foreach (var disposable in _disposables)
				{
					disposable.Dispose();
				}
			}

			#endregion

			#endregion
		}

		#endregion
	}
}