namespace UAX29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal abstract class SplitterBase<TSpan>
{
    readonly internal Decoder<TSpan> DecodeFirstRune;
    readonly internal Decoder<TSpan> DecodeLastRune;

    public SplitterBase(Decoder<TSpan> decodeFirstRune, Decoder<TSpan> decodeLastRune)
    {
        this.DecodeFirstRune = decodeFirstRune;
        this.DecodeLastRune = decodeLastRune;
    }

    /// <summary>
    /// Reads input until a token break
    /// </summary>
    /// <param name="input">Data to split</param>
    /// <param name="atEOF">
    /// Indicates whether the current input is all that is coming.
    /// (Always true in the current implementation, we may implement streaming in the future.)
    /// </param>
    /// <returns></returns>
    internal abstract int Split(ReadOnlySpan<TSpan> input, bool atEOF);
}

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
