namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal delegate OperationStatus Decoder(ReadOnlySpan<byte> input, out Rune result, out int consumed);

internal abstract class SplitterBase
{
    readonly internal Dict Dict;
    readonly internal Property Ignore;
    readonly internal Decoder DecodeFirstRune;
    readonly internal Decoder DecodeLastRune;

    public SplitterBase(Dict dict, Property ignore, Decoder decodeFirstRune, Decoder decodeLastRune)
    {
        this.Dict = dict;
        this.Ignore = ignore;
        this.DecodeFirstRune = decodeFirstRune;
        this.DecodeLastRune = decodeLastRune;
    }

    public abstract int Split(ReadOnlySpan<byte> input, bool atEOF);

    /// <summary>
    /// Seeks backward until it hits a rune which matches property.
    /// </summary>
    /// <param name="property">Property to attempt to find</param>
    /// <param name="input">Data in which to seek</param>
    /// <returns>The index if found, or -1 if not</returns>
    internal int PreviousIndex(Property property, ReadOnlySpan<byte> input)
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

            if (lookup.Iss(Ignore))
            {
                continue;
            }

            if (lookup.Iss(property))
            {
                return i;
            }

            // If we get this far, it's not there
            break;
        }

        return -1;
    }

    /// <summary>
    /// Seeks backward until it hits a rune which matches property.
    /// </summary>
    /// <param name="property">Property to attempt to find</param>
    /// <param name="input">Data in which to seek</param>
    /// <returns>True if found, otherwise false</returns>
    internal bool Previous(Property property, ReadOnlySpan<byte> input)
    {
        return PreviousIndex(property, input) != -1;
    }

    internal bool Subsequent(Property property, ReadOnlySpan<byte> data)
    {
        var i = 0;
        while (i < data.Length)
        {
            var status = DecodeFirstRune(data[i..], out Rune rune, out int w);
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

            if (lookup.Iss(Ignore))
            {
                i += w;
                continue;
            }

            if (lookup.Iss(property))
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
    internal static bool Iss(this Property lookup, Property properties)
    {
        return (lookup & properties) != 0;
    }
}
