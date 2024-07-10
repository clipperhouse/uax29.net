namespace UAX29;

using System.Diagnostics;

/// A bitmap of Unicode categories
using Property = uint;

/// <summary>
/// The function that splits a string or UTF-8 byte array into tokens.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
/// <param name="input">The string to split/tokenize.</param>
/// <returns>How many bytes/chars were consumed from the input.</returns>
internal delegate int Split<T>(ReadOnlySpan<T> input, out Property seen);

internal static class Extensions
{
	/// <summary>
	/// Determines whether two properties (bitstrings) match, i.e. intersect, i.e. share at least one bit.
	/// </summary>
	/// <param name="lookup">One property to test against...</param>
	/// <param name="properties">...the other</param>
	/// <returns>True if the two properties share a bit, i.e. Unicode category.</returns>
	internal static bool Is(this Property lookup, Property properties)
	{
		return (lookup & properties) != 0;
	}

	/// <summary>
	/// Determines if property consists entirely of compare, i.e. no other values (flags) besides the ones in compare
	/// </summary>
	/// <param name="property">The property to test</param>
	/// <param name="compare">THe property to test against</param>
	/// <returns>True if property consists entirely of compare</returns>
	internal static bool IsExclusively(this Property property, Property compare)
	{
		Debug.Assert(compare > 0);
		return
			(property & compare) != 0 &&    // compare appears in property
			(property & ~compare) == 0      // but no others do
		;
	}
}
