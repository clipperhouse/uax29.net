namespace UAX29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal abstract class SplitterBase2<TSpan> where TSpan : struct
{
    readonly internal Dict Dict;
    readonly internal Property Ignore;

    public SplitterBase2(Dict Dict, Property Ignore)
    {
        this.Dict = Dict;
        this.Ignore = Ignore;
    }

    /// <summary>
    /// Reads input until a token break
    /// </summary>
    /// <param name="runes">String to split</param>
    /// <param name="atEOF">
    /// Indicates whether the current input is all that is coming.
    /// (Always true in the current implementation, we may implement streaming in the future.)
    /// </param>
    /// <returns></returns>
    internal abstract int Split(RuneTokenizer<TSpan> runes, bool atEOF);

    /// <summary>
    /// Seek backward until it hits a rune which matches property.
    /// </summary>
    /// <param name="property">Property to attempt to find</param>
    /// <param name="runes">Data in which to seek</param>
    /// <returns>True if found, otherwise false</returns>
    internal bool Previous(Property property, RuneTokenizer<TSpan> runes, Property intermediate = 0)
    {
        if (intermediate != 0)
        {
            while (runes.MovePrevious())
            {
                var lookup = Dict.Lookup(runes.Current.Value);

                if (lookup.Is(Ignore))
                {
                    continue;
                }

                if (!lookup.Is(intermediate))
                {
                    return false;
                }

                break;
            }
        }

        while (runes.MovePrevious())
        {
            var lookup = Dict.Lookup(runes.Current.Value);

            if (lookup.Is(Ignore))
            {
                continue;
            }

            if (lookup.Is(property))
            {
                return true;
            }

            break;
        }

        return false;
    }


    /// <summary>
    /// Seek forward until it hits a rune which matches property.
    /// </summary>
    /// <param name="property">Property to attempt to find</param>
    /// <param name="runes">Data in which to seek</param>
    /// <returns>True if found, otherwise false</returns>
    internal bool Subsequent(Property property, RuneTokenizer<TSpan> runes, Property intermediate = 0)
    {
        if (intermediate != 0)
        {
            while (runes.MoveNext())
            {
                var lookup = Dict.Lookup(runes.Current.Value);

                if (lookup.Is(Ignore))
                {
                    continue;
                }

                if (!lookup.Is(intermediate))
                {
                    return false;
                }

                break;
            }
        }

        while (runes.MoveNext())
        {
            var lookup = Dict.Lookup(runes.Current.Value);

            if (lookup.Is(Ignore))
            {
                continue;
            }

            if (lookup.Is(property))
            {
                return true;
            }

            break;
        }

        return false;
    }
}
