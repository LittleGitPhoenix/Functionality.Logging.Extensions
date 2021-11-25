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

		internal static readonly Dictionary<IGroupIdentifier, HashSet<ILogger>> Cache;

		#endregion

		#region Properties

		#endregion

		#region (De)Constructors

		static LoggerGroupManager()
		{
			// Save parameters.

			// Initialize fields.
			Cache = new Dictionary<IGroupIdentifier, HashSet<ILogger>>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the <paramref name="logger"/> to the internal cache using the <paramref name="groupIdentifier"/>.
		/// </summary>
		/// <param name="logger"> The <see cref="ILogger"/> to add. </param>
		/// <param name="groupIdentifier"> The group identifier used when adding. </param>
		/// <returns> The same <see cref="ILogger"/> instance for chaining. </returns>
		internal static ILogger AddLoggerToGroup<TIdentifier>(ILogger logger, TIdentifier groupIdentifier)
			where TIdentifier : notnull
		{
			var identifier = (GroupIdentifier<TIdentifier>) groupIdentifier;
			if (LoggerGroupManager.Cache.TryGetValue(identifier, out var loggerCollection))
			{
				loggerCollection.Add(logger);
			}
			else
			{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
				LoggerGroupManager.Cache.Add(identifier, new HashSet<ILogger>() {logger});
#else
				LoggerGroupManager.Cache.TryAdd(identifier, new HashSet<ILogger>() {logger});
#endif
			}

			return logger;
		}

		/// <summary>
		/// Returns all cached <see cref="ILogger"/>s for the given <paramref name="groupIdentifier"/>.
		/// </summary>
		/// <param name="groupIdentifier"> The group identifier of the cached loggers. </param>
		/// <returns> A collection of <see cref="ILogger"/>s sharing the same group. </returns>
		public static IReadOnlyCollection<ILogger> GetAllLoggers<TIdentifier>(TIdentifier groupIdentifier)
			where TIdentifier : notnull
		{
			var identifier = (GroupIdentifier<TIdentifier>) groupIdentifier;
			var loggerCollection = LoggerGroupManager
				.GetAllGroups(group => identifier.Equals(group.GroupIdentifier))
				.FirstOrDefault()
				.LoggerCollection
				;

			return loggerCollection?.ToArray() ?? Array.Empty<ILogger>();
		}

		/// <summary>
		/// Return all groups.
		/// </summary>
		/// <returns> A collection of all cached groups. </returns>
		public static IReadOnlyCollection<(object GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetAllGroups()
		{
			return LoggerGroupManager
				.GetAllGroups(_ => true)
				.Select(tuple => (tuple.GroupIdentifier.Value, tuple.LoggerCollection))
				.ToArray()
				;
		}

		/// <summary>
		/// Return all groups that the <paramref name="logger"/> is a part of.
		/// </summary>
		/// <param name="logger"> The <see cref="ILogger"/> whose groups to get.. </param>
		/// <returns> A collection of groups, where the <paramref name="logger"/> is a part of. </returns>
		public static IReadOnlyCollection<(object GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetAllGroups(ILogger logger)
		{
			return LoggerGroupManager
				.GetAllGroups(tuple => tuple.LoggerCollection.Contains(logger))
				.Select(tuple => (tuple.GroupIdentifier.Value, tuple.LoggerCollection))
				.ToArray()
				;
		}

		/// <summary>
		/// Return all groups matching the <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate"> The <see cref="Predicate{T}"/> to match. </param>
		/// <returns> A collection of groups, that match the <paramref name="predicate"/>. </returns>
		private static IEnumerable<(IGroupIdentifier GroupIdentifier, IReadOnlyCollection<ILogger> LoggerCollection)> GetAllGroups(Predicate<(IGroupIdentifier GroupIdentifier, HashSet<ILogger> LoggerCollection)> predicate)
		{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
			foreach (var group in LoggerGroupManager.Cache)
			{
				var tuple = (group.Key, group.Value);
#else
			foreach (var (groupIdentifier, loggerCollection) in LoggerGroupManager.Cache)
			{
				var tuple = (groupIdentifierHash: groupIdentifier, loggerCollection);
#endif
				if (predicate.Invoke(tuple)) yield return tuple;
			}
		}

		#endregion

		#region Nested Types

		internal interface IGroupIdentifier
		{
			public object Value { get; }
		}
		
		internal readonly struct GroupIdentifier<TIdentifier> : IGroupIdentifier, IEquatable<IGroupIdentifier>
			where TIdentifier : notnull
		{
			#region Delegates / Events
			#endregion

			#region Constants
			#endregion

			#region Fields

			private readonly int _hashCode;

			#endregion

			#region Properties

			object IGroupIdentifier.Value => this.Value;

			public TIdentifier Value { get; }
			
			#endregion

			#region (De)Constructors

			public GroupIdentifier(TIdentifier value)
			{
				// Save parameters.
				this.Value = value;

				// Initialize fields.
				_hashCode = EqualityComparer<TIdentifier>.Default.GetHashCode(this.Value);
			}

			public static explicit operator GroupIdentifier<TIdentifier>(TIdentifier value)
			{
				return new GroupIdentifier<TIdentifier>(value);
			}

			#endregion

			#region Methods

			#region IEquatable

			/// <summary> The default hash method. </summary>
			/// <returns> A hash value for the current object. </returns>
			public override int GetHashCode()
			{
				return _hashCode;
			}

			/// <summary>
			/// Compares this instance to another one.
			/// </summary>
			/// <param name="other"> The other instance to compare to. </param>
			/// <returns> <c>True</c> if this instance equals the <paramref name="other"/> instance, otherwise <c>False</c>. </returns>
			public override bool Equals(object? other)
			{
				if (other is IGroupIdentifier equatable) return this.Equals(equatable);
				return false;
			}

			/// <summary>
			/// Compares this instance to another one.
			/// </summary>
			/// <param name="other"> The other instance to compare to. </param>
			/// <returns> <c>True</c> if this instance equals the <paramref name="other"/> instance, otherwise <c>False</c>. </returns>
			public bool Equals(IGroupIdentifier? other)
			{
				return other?.GetHashCode() == this.GetHashCode();
			}

			/// <summary>
			/// Compares the two instance <paramref name="x"/> and <paramref name="y"/> for equality.
			/// </summary>
			/// <param name="x"> The first instance to compare. </param>
			/// <param name="y"> The second instance to compare. </param>
			/// <returns> <c>True</c> if <paramref name="x"/> equals <paramref name="y"/>, otherwise <c>False</c>. </returns>		
			public static bool operator ==(GroupIdentifier<TIdentifier> x, IGroupIdentifier y)
			{
				return x.GetHashCode() == y?.GetHashCode();
			}

			/// <summary>
			/// Compares the two instance <paramref name="x"/> and <paramref name="y"/> for in-equality.
			/// </summary>
			/// <param name="x"> The first instance to compare. </param>
			/// <param name="y"> The second instance to compare. </param>
			/// <returns> <c>True</c> if <paramref name="x"/> doesn't equal <paramref name="y"/>, otherwise <c>False</c>. </returns>
			public static bool operator !=(GroupIdentifier<TIdentifier> x, IGroupIdentifier y)
			{
				return !(x == y);
			}

			#endregion

			#region Overrides of ValueType

			/// <inheritdoc />
			public override string ToString() => $"{nameof(IGroupIdentifier)}: {this.Value} ({_hashCode})";

			#endregion

			#endregion
		}

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