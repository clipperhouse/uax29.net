namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

internal abstract class SplitterBase2<TSpan> where TSpan : struct
{
    readonly internal Dict Dict;

    public SplitterBase2(Dict Dict, Property Ignore)
    {
        this.Dict = Dict;
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
}
