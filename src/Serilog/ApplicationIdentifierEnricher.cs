using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Phoenix.Functionality.Logging.Extensions.Serilog
{
	/// <summary>
	/// <see cref="ILogEventEnricher"/> that adds a unique application identifier to log events.
	/// </summary>
	public sealed class ApplicationIdentifierEnricher : ILogEventEnricher
	{
		#region Delegates / Events
		#endregion

		#region Constants
		
		/// <summary> The property name used when enriching events. </summary>
		public const string PropertyName = "ApplicationIdentifier";

		#endregion

		#region Fields

		internal static Dictionary<int, char> AvailableChars;

		private readonly LogEventProperty _logEventProperty;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		static ApplicationIdentifierEnricher()
		{
			AvailableChars = ApplicationIdentifierEnricher.GetAvailableCharsForIdentifier();
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="values"> A collection of strings from which a unique identifier is created via <see cref="BuildAlphanumericIdentifier"/>. </param>
		/// <remarks> The order of the values is not relevant for building the identifier. </remarks>
		public ApplicationIdentifierEnricher(params string[] values)
			: this(ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(values))
		{ }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="applicationIdentifier"> The unique application identifier. </param>
		public ApplicationIdentifierEnricher(string applicationIdentifier)
		{
			_logEventProperty = new LogEventProperty(PropertyName, new ScalarValue(applicationIdentifier));
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			logEvent.AddPropertyIfAbsent(_logEventProperty);
		}

		/// <summary>
		/// Creates an identifier comprised of 20 alphanumeric characters.
		/// </summary>
		/// <param name="values"> The values from which to create the hash code. </param>
		/// <returns> An 20 alphanumeric characters long identifier. </returns>
		/// <remarks> The order of the values is not relevant for building the identifier. </remarks>
		public static string BuildAlphanumericIdentifier(params string[] values)
		{
			var seed = 317;
			if (values.Any())
			{
				var allValues = String.Join(String.Empty, values.OrderBy(value => value));
				var valuesData = Encoding.UTF8.GetBytes(allValues);
				using var hashGenerator = System.Security.Cryptography.SHA256.Create();
				var hashData = hashGenerator.ComputeHash(valuesData);
				seed = BitConverter.ToInt32(hashData, 0);
			}

			var random = new Random(seed);
			var chars = new char[20];
			var amountOfAvailableChars = AvailableChars.Count;
			for (var index = 0; index < chars.Length; index++)
			{
				chars[index] = AvailableChars[random.Next(0, amountOfAvailableChars)];
			}

			var identifier = new String(chars);
			return identifier;
		}

		private static Dictionary<int, char> GetAvailableCharsForIdentifier()
		{
			var availableChars = new List<int>()
				.Concat(Enumerable.Range(48, 10)) // 0-9
				.Concat(Enumerable.Range(65, 26)) // A-Z
				.Concat(Enumerable.Range(97, 26)) // a-z
				.Select((value, index) => (Index: index, Char: Encoding.ASCII.GetString(new[] { (byte)value })[0]))
				.ToDictionary
				(
					anonymous => anonymous.Index,
					anonymous => anonymous.Char
				)
				;
			return availableChars;
		}
		
		#endregion
	}

	/// <summary>
	/// Provides extension methods for <see cref="LoggerEnrichmentConfiguration"/>.
	/// </summary>
	public static class LoggerEnrichmentConfigurationExtensions
	{
		/// <summary>
		/// Adds a unique <paramref name="applicationIdentifier"/> to log events.
		/// </summary>
		/// <param name="enrich"> The extended <see cref="LoggerEnrichmentConfiguration"/>. </param>
		/// <param name="applicationIdentifier"> The unique identifier. </param>
		/// <returns> The <see cref="LoggerConfiguration"/> for further chaining. </returns>
		public static LoggerConfiguration WithApplicationIdentifier(this LoggerEnrichmentConfiguration enrich, string applicationIdentifier)
		{
			if (enrich is null) throw new ArgumentNullException(nameof(enrich));
			return enrich.With(new ApplicationIdentifierEnricher(applicationIdentifier));
		}
	}
}