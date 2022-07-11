#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Phoenix.Functionality.Logging.Extensions.Microsoft;

/// <summary>
/// Manages logger groups.
/// </summary>
internal static class LoggerGroupManager
{
    #region Delegates / Events

    #endregion

    #region Constants

    #endregion

    #region Fields

    internal static readonly ConcurrentDictionary<IGroupIdentifier, ILoggerGroup> Cache;

    #endregion

    #region Properties

    #endregion

    #region (De)Constructors

    static LoggerGroupManager()
    {
        // Save parameters.

        // Initialize fields.
        Cache = new ();
    }

	#endregion

	#region Methods

	/// <inheritdoc cref="LoggerExtensions.AddToGroup{TIdentifier}"/>
	internal static ILogger AddLoggerToGroup<TIdentifier>(ILogger logger, TIdentifier groupIdentifier, bool applyExistingScope = true)
		where TIdentifier : notnull
	{
		var identifier = (GroupIdentifier<TIdentifier>) groupIdentifier;

		if (Cache.TryGetValue(identifier, out var loggerGroup))
		{
			loggerGroup.AddLogger(logger, applyExistingScope);
		}
		else
		{
			Cache.TryAdd(identifier, new LoggerGroup(logger));
		}

		return logger;
	}

	/// <inheritdoc cref="LoggerExtensions.RemoveFromGroup{TIdentifier}"/>
	internal static ILogger RemoveLoggerFromGroup<TIdentifier>(ILogger logger, TIdentifier groupIdentifier)
		where TIdentifier : notnull
	{
		var identifier = (GroupIdentifier<TIdentifier>) groupIdentifier;
		if (Cache.TryGetValue(identifier, out var loggerGroup))
		{
			loggerGroup.RemoveLogger(logger);
			TryRemoveLoggerGroup(loggerGroup, identifier);
		}
		return logger;
	}

	/// <inheritdoc cref="LoggerExtensions.RemoveFromAllGroups"/>
	internal static ILogger RemoveFromAllGroups(ILogger logger)
	{
		var groups = GetMatchingGroups(tuple => tuple.LoggerGroup.Contains(logger));
		foreach (var (identifier, loggerGroup) in groups)
		{
			loggerGroup.RemoveLogger(logger);
			TryRemoveLoggerGroup(loggerGroup, identifier);
		}
		return logger;
	}

	/// <inheritdoc cref="LoggerExtensions.AsGroup{TIdentifier}"/>
	internal static ILoggerGroup GetGroup<TIdentifier>(TIdentifier groupIdentifier)
		where TIdentifier : notnull
	{
		var identifier = (GroupIdentifier<TIdentifier>) groupIdentifier;
		var matchingGroups = LoggerGroupManager.GetMatchingGroups(group => identifier.Equals(group.GroupIdentifier)).Take(1).ToArray();
		return matchingGroups.Any() ? matchingGroups.FirstOrDefault().LoggerGroup : new LoggerGroup();
	}

	/// <summary>
	/// Return all groups.
	/// </summary>
	/// <returns> A collection of all cached groups. </returns>
	internal static IReadOnlyCollection<(object GroupIdentifier, ILoggerGroup LoggerGroup)> GetAllGroups()
	{
		return LoggerGroupManager
			.GetMatchingGroups(_ => true)
			.Select(tuple => (tuple.GroupIdentifier.Value, tuple.LoggerGroup))
			.ToArray()
			;
	}

	/// <inheritdoc cref="LoggerExtensions.GetGroups"/>
	internal static IReadOnlyCollection<(object GroupIdentifier, ILoggerGroup LoggerGroup)> GetGroupsOfLogger(ILogger logger)
    {
        return LoggerGroupManager
            .GetMatchingGroups(tuple => tuple.LoggerGroup.Contains(logger))
            .Select(tuple => (tuple.GroupIdentifier.Value, tuple.LoggerGroup))
            .ToArray()
            ;
    }

	/// <summary>
	/// Remove the <paramref name="loggerGroup"/> from the internal <see cref="Cache"/> if it no longer contains any loggers.
	/// </summary>
	/// <param name="loggerGroup"> The <see cref="ILoggerGroup"/> to remove. </param>
	/// <param name="identifier"> The <see cref="IGroupIdentifier"/> used for lookup in <see cref="Cache"/>. </param>
	/// <returns> <b>True</b> on success, otherwise <b>false</b>. </returns>
	private static bool TryRemoveLoggerGroup(ILoggerGroup loggerGroup, IGroupIdentifier identifier)
	{
		if (loggerGroup.Count != 0) return false;
		var success = Cache.TryRemove(identifier, out var removedLoggerGroup);
		removedLoggerGroup?.Dispose();
		return success;
	}

    /// <summary>
    /// Return all groups matching the <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate"> The <see cref="Predicate{T}"/> to match. </param>
    /// <returns> A collection of groups, that match the <paramref name="predicate"/>. </returns>
    private static IEnumerable<(IGroupIdentifier GroupIdentifier, ILoggerGroup LoggerGroup)> GetMatchingGroups(Predicate<(IGroupIdentifier GroupIdentifier, ILoggerGroup LoggerGroup)> predicate)
    {
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
		foreach (var group in Cache)
		{
			var tuple = (group.Key, group.Value);
#else
        foreach (var (groupIdentifier, loggerGroup) in Cache)
        {
            var tuple = (groupIdentifierHash: groupIdentifier, loggerGroup);
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

	#endregion
}