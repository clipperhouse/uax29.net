namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

/// <summary>
/// The function that splits a string or UTF-8 byte array into tokens.
/// </summary>
/// <typeparam name="TSpan">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
/// <param name="input">The string to split/tokenize.</param>
/// <returns>How many bytes/chars were consumed from the input.</returns>
///

/// <summary>
/// The function that splits a string or UTF-8 byte array into tokens.
/// </summary>
/// <param name="start"></param>
/// <param name="end"></param>
/// <param name="input"></param>
/// <typeparam name="TSpan"></typeparam>
/// <returns></returns>
internal delegate (int start, int end, int advance) Split<TSpan>(ReadOnlySpan<TSpan> input);

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
}
