namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// Specifies how an <see cref="ILogScope"/> is stored and which log events it is applied to.
/// </summary>
/// <remarks>
/// <para>
/// Choose a value based on the <em>lifetime and visibility</em> of the scope data, not on how classes call each other:
/// </para>
/// <list type="bullet">
/// <item><description>
/// Scope that belongs to <b>this execution</b> (a request, an operation, a unit of work) →
/// <see cref="ExecutionContextAware"/>.
/// </description></item>
/// <item><description>
/// Scope that belongs to <b>this logger forever</b> (service name, version, environment) →
/// <see cref="Independent"/>.
/// </description></item>
/// </list>
/// <para>
/// The four canonical usage scenarios are:
/// </para>
/// <list type="number">
/// <item><description>
/// <b>Single-logger + <see cref="ExecutionContextAware"/></b> — the common case for concurrent workloads
/// (e.g. a singleton handling many HTTP requests in parallel). Each request has its own execution context, so
/// each sees only its own scope data (e.g. a per-request trace id).
/// </description></item>
/// <item><description>
/// <b>Single-logger + <see cref="Independent"/></b> — scope set once at startup that must appear on every log
/// event regardless of which request or thread is executing (e.g. service name, version, environment).
/// </description></item>
/// <item><description>
/// <b>Logger group + <see cref="Independent"/></b> — the same static metadata pinned across all loggers of a
/// group at startup (e.g. subsystem version shared by multiple collaborating classes).
/// </description></item>
/// <item><description>
/// <b>Logger group + <see cref="ExecutionContextAware"/></b> — a per-operation scope (e.g. a trace id) applied
/// to every logger in the group at the start of an operation. Concurrent operations are isolated from each other
/// while all loggers within a single operation carry the same scope.
/// </description></item>
/// </list>
/// </remarks>
public enum LogScopeType
{
	/// <summary>
	/// The scope is stored in a single shared collection visible to <b>all</b> execution contexts and threads.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this for scope data that should appear on every log event emitted through a logger regardless of which
	/// execution context is active — typically static, lifetime-long metadata such as service name, version, or
	/// environment.
	/// </para>
	/// <para>
	/// Because the storage is shared and globally mutated, scope additions from any context are immediately
	/// visible everywhere. Bleed-back between concurrent operations is intentional for this type.
	/// </para>
	/// </remarks>
	Independent,

	/// <summary>
	/// The scope is stored using copy-on-write <see cref="System.Threading.AsyncLocal{T}"/> semantics and is only
	/// visible within the execution context it was added from (and any child contexts spawned afterwards).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this for scope data that belongs to a specific operation and must not appear in log events emitted by
	/// concurrent operations — typically per-request or per-operation data such as a trace or correlation id.
	/// </para>
	/// <para>
	/// Each <c>AddScope</c> call creates a new dictionary snapshot that is assigned only to the current
	/// execution-context slot. Disposing the returned <c>IDisposable</c> restores the previous snapshot, making
	/// scope management behave like a stack.
	/// </para>
	/// <para>
	/// <b>Known limitation:</b> isolation only takes effect once a new execution context is actually created by the
	/// .NET runtime (e.g. via <c>Task.Run</c> or a truly yielding <c>await</c>). Scope added inside a method before
	/// its first suspension point runs in the caller's execution context and modifies the caller's slot directly.
	/// The recommended mitigation for class-hierarchy isolation is to use separate <c>ILogger</c> instances per class
	/// grouped via <c>ILoggerGroup</c>.
	/// </para>
	/// </remarks>
	ExecutionContextAware,
}
