#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging;

static class IdentifierBuilder
{
    #region Delegates / Events
    #endregion

    #region Constants
    #endregion

    #region Fields

	internal static Dictionary<int, char> AlphaNumericCharSet;

    #endregion

    #region Properties
    #endregion

    #region (De)Constructors

    static IdentifierBuilder()
    {
        // Save parameters.

        // Initialize fields.
        AlphaNumericCharSet = GetAlphaNumericChars();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates both the numeric and the alphanumeric identifiers for the <paramref name="values"/>.
    /// </summary>
    /// <param name="values"> The values from which to create the numeric hash code. </param>
    /// <returns> A <see cref="ValueTuple"/> containing the numeric and the alphanumeric identifiers. </returns>
    /// <remarks> The order of the values is not relevant for building the identifier. </remarks>
    public static (int NumericIdentifier, string AlphanumericIdentifier) BuildNumericAndAlphanumericIdentifier(params string[] values)
	{
		var numericIdentifier = BuildNumericIdentifier(values);
		var alphaNumericIdentifier = BuildAlphanumericIdentifier(numericIdentifier);
		return (numericIdentifier, alphaNumericIdentifier);
	}

    /// <summary>
    /// Creates a numeric identifier for the <paramref name="values"/>.
    /// </summary>
    /// <param name="values"> The values from which to create the numeric hash code. </param>
    /// <returns> A numeric identifier. </returns>
    /// <remarks> The order of the values is not relevant for building the identifier. </remarks>
    public static int BuildNumericIdentifier(params string[] values)
    {
        if (!values.Any()) return 0;

        var allValues = String.Join(String.Empty, values.OrderBy(value => value));
        var valuesData = System.Text.Encoding.UTF8.GetBytes(allValues);
        using var hashGenerator = System.Security.Cryptography.SHA256.Create();
        var hashData = hashGenerator.ComputeHash(valuesData);
        var identifier = BitConverter.ToInt32(hashData, 0);
		return identifier;
	}

    /// <summary>
    /// Creates an identifier comprised of 20 alphanumeric characters.
    /// </summary>
    /// <param name="values"> The values from which to create the hash code. </param>
    /// <returns> A 20 alphanumeric characters long identifier. </returns>
    /// <remarks> The order of the values is not relevant for building the identifier. </remarks>
    public static string BuildAlphanumericIdentifier(params string[] values)
		=> BuildAlphanumericIdentifier(values.Any() ? BuildNumericIdentifier(values) : 317);

    private static string BuildAlphanumericIdentifier(int seed)
    {
        //var seed = values.Any() ? BuildNumericIdentifier(values) : 317;
        var random = new Random(seed);
        var chars = new char[20];
        var amountOfAvailableChars = AlphaNumericCharSet.Count;
        for (var index = 0; index < chars.Length; index++)
        {
            chars[index] = AlphaNumericCharSet[random.Next(0, amountOfAvailableChars)];
        }

        var identifier = new String(chars);
        return identifier;
    }

    private static Dictionary<int, char> GetAlphaNumericChars()
    {
        var availableChars = new List<int>()
            .Concat(Enumerable.Range(48, 10)) // 0-9
            .Concat(Enumerable.Range(65, 26)) // A-Z
            .Concat(Enumerable.Range(97, 26)) // a-z
            .Select((value, index) => (Index: index, Char: System.Text.Encoding.ASCII.GetString(new[] {(byte) value})[0]))
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