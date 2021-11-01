using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Microsoft
{
	/// <summary>
	/// Collection of log scopes used by the <see cref="FrameworkLogger"/>.
	/// </summary>
	/// <remarks>
	/// <para> Based on: </para>
	/// <para> • https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerProvider.cs </para>
	/// <para> • https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/Extensions/Logging/SerilogLoggerScope.cs </para>
	/// </remarks>
	class FrameworkLoggerScopes : List<object>
	{
		#region Delegates / Events
		#endregion

		#region Constants

		private const string NoName = "None";

		#endregion

		#region Fields
		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public FrameworkLoggerScopes()
		{
			// Save parameters.

			// Initialize fields.
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the <paramref name="scope"/> to the internal collection.
		/// </summary>
		/// <typeparam name="TState"> The type of the scope. </typeparam>
		/// <param name="scope"> The scope to add. </param>
		/// <returns> An <see cref="IDisposable"/> that will remove the scope when it is disposed. </returns>
		internal IDisposable AddScope<TState>(TState scope)
			where TState : notnull
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (scope is null) return DisposableAction.NoDisposableAction;
			
			// Only add unique items.
			if (!this.Contains(scope)) this.Add(scope);
			return new DisposableAction(() =>
				{
					try
					{
						if (this.Contains(scope)) this.Remove(scope);
					}
					catch (Exception ex) { /* ignore */ }
				}
			);
		}

		/// <summary>
		/// Converts all internal scopes into <see cref="LogEventPropertyValue"/>s to be used by <see cref="Serilog"/>.
		/// </summary>
		/// <returns> An enumerable of <see cref="LogEventPropertyValue"/>. </returns>
		internal IEnumerable<LogEventPropertyValue> CreateLogEventPropertyValues(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			foreach (var scope in this)
			{
				var scopeItem = FrameworkLoggerScopes.CreateLogEventPropertyValue(scope, logEvent, propertyFactory);
				if (scopeItem is null) continue;
				yield return scopeItem;
			}
		}

		private static LogEventPropertyValue? CreateLogEventPropertyValue(object scope, LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			if (scope is IEnumerable<KeyValuePair<string, object>> stateProperties)
			{
				LogEventPropertyValue? scopeItem = null; // Unless it's `FormattedLogValues`, these are treated as property bags rather than scope items.

				foreach (var stateProperty in stateProperties)
				{
					if (stateProperty.Key == FrameworkLogger.OriginalFormatPropertyName && stateProperty.Value is string)
					{
						scopeItem = new ScalarValue(scope.ToString());
						continue;
					}

					var key = stateProperty.Key;
					var destructureObjects = false;
					var value = stateProperty.Value;

					if (key.StartsWith("@"))
					{
						key = key.Substring(1);
						destructureObjects = true;
					}

					if (key.StartsWith("$"))
					{
						key = key.Substring(1);
						value = value?.ToString();
					}

					var property = propertyFactory.CreateProperty(key, value, destructureObjects);
					logEvent.AddPropertyIfAbsent(property);
				}

				return scopeItem;
			}
			else
			{
				return propertyFactory.CreateProperty(NoName, scope).Value;
			}
		}

		#endregion

		#region Nested Types

		sealed class DisposableAction : IDisposable
		{
			#region Delegates / Events
			#endregion

			#region Constants
			#endregion

			#region Fields

			private readonly Action _dispose;

			#endregion

			#region Properties

			public static IDisposable NoDisposableAction { get; } = new DisposableAction(() => { });

			#endregion

			#region (De)Constructors

			public DisposableAction(Action dispose)
			{
				_dispose = dispose;
			}

			#endregion

			#region Methods

			/// <inheritdoc />
			public void Dispose()
			{
				try
				{
					_dispose.Invoke();
				}
				catch (Exception ex)
				{
					/* ignore */
				}
			}

			#endregion
		}

		#endregion
	}
}