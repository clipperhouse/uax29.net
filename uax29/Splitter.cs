namespace UAX29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal delegate OperationStatus Decoder<TSpan>(ReadOnlySpan<TSpan> input, out Rune result, out int consumed);

internal abstract class SplitterBase<TSpan>
{
    readonly internal Dict Dict;
    readonly internal Property Ignore;
    readonly internal Decoder<TSpan> DecodeFirstRune;
    readonly internal Decoder<TSpan> DecodeLastRune;

    public SplitterBase(Dict dict, Property ignore, Decoder<TSpan> decodeFirstRune, Decoder<TSpan> decodeLastRune)
    {
        this.Dict = dict;
        this.Ignore = ignore;
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

    /// <summary>
    /// Seek backward until it hits a rune which matches property.
    /// </summary>
    /// <param name="property">Property to attempt to find</param>
    /// <param name="input">Data in which to seek</param>
    /// <returns>The index if found, or -1 if not</returns>
    internal int PreviousIndex(Property property, ReadOnlySpan<TSpan> input)
    {
        // Start at the end of the buffer and move backwards
        var i = input.Length;
        while (i > 0)
        {
            var status = DecodeLastRune(input[..i], out Rune rune, out int w);
            if (status != OperationStatus.Done)
            {
                // Garbage in, garbage out
                break;
            }
            if (w == 0)
            {
                break;
            }

            i -= w;
            var lookup = Dict.Lookup(rune.Value);

            if (lookup.Is(Ignore))
            {
                continue;
            }

            if (lookup.Is(property))
            {
                return i;
            }

            // If we get this far, it's not there
            break;
        }

        return -1;
    }

    /// <summary>
    /// Seek backward until it hits a rune which matches property.
    /// </summary>
    /// <param name="property">Property to attempt to find</param>
    /// <param name="input">Data in which to seek</param>
    /// <returns>True if found, otherwise false</returns>
    internal bool Previous(Property property, ReadOnlySpan<TSpan> input)
    {
        return PreviousIndex(property, input) != -1;
    }

    /// <summary>
    /// Seek forward until it hits a rune which matches property.
    /// </summary>
    /// <param name="property">Property to attempt to find</param>
    /// <param name="input">Data in which to seek</param>
    /// <returns>True if found, otherwise false</returns>
    internal bool Subsequent(Property property, ReadOnlySpan<TSpan> input)
    {
        var i = 0;
        while (i < input.Length)
        {
            var status = DecodeFirstRune(input[i..], out Rune rune, out int w);
            if (status != OperationStatus.Done)
            {
                // Garbage in, garbage out
                break;
            }
            if (w == 0)
            {
                break;
            }

            var lookup = Dict.Lookup(rune.Value);

            if (lookup.Is(Ignore))
            {
                i += w;
                continue;
            }

            if (lookup.Is(property))
            {
                return true;
            }

            // If we get this far, it's not there
            break;
        }

        return false;
    }
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
