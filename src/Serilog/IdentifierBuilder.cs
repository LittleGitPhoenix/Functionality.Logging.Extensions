using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phoenix.Functionality.Logging.Extensions.Serilog
{
	static class IdentifierBuilder
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		internal static Dictionary<int, char> AvailableChars;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		static IdentifierBuilder()
		{
			// Save parameters.

			// Initialize fields.
			AvailableChars = IdentifierBuilder.GetAvailableCharsForIdentifier();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Creates an identifier comprised of 20 alphanumeric characters.
		/// </summary>
		/// <param name="values"> The values from which to create the hash code. </param>
		/// <returns> A 20 alphanumeric characters long identifier. </returns>
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
}