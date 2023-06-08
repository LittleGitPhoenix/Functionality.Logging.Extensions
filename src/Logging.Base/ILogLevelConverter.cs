namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Interface for log level converters.
/// </summary>
/// <typeparam name="TSourceLogLevel"> The type of the source log level. </typeparam>
/// <typeparam name="TTargetLogLevel"> The type of the target log level. </typeparam>
public interface ILogLevelConverter<TSourceLogLevel, TTargetLogLevel>
	where TSourceLogLevel : Enum
	where TTargetLogLevel : Enum
{
	/// <summary>
	/// Converts the <paramref name="sourceLogLevel"/> into the <typeparamref name="TTargetLogLevel"/>.
	/// </summary>
	TTargetLogLevel ConvertSourceToTarget(TSourceLogLevel sourceLogLevel);

	/// <summary>
	/// Converts the <paramref name="targetLogLevel"/> into the <typeparamref name="TSourceLogLevel"/>.
	/// </summary>
	TSourceLogLevel ConvertTargetToSource(TTargetLogLevel targetLogLevel);
}

/// <summary>
/// Special <see cref="ILogLevelConverter{TSourceLogLevel,TTargetLogLevel}"/> whose source and target log level is identical.
/// </summary>
/// <typeparam name="TLogLevel"> The type of the single log level. </typeparam>
public interface INoLogLevelConverter<TLogLevel> : ILogLevelConverter<TLogLevel, TLogLevel>
	where TLogLevel : Enum { }