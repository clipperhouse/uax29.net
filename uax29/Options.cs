namespace UAX29;

/// <summary>
/// Options for handling input text in UAX29.
/// </summary>
[Flags]
public enum Options : byte
{
    /// <summary>
    /// Do nothing special; default
    /// </summary>
    None = 0,

    /// <summary>
    /// Omit tokens that consist entirely of whitespace, defined as UAX #29 WSegSpace | CR | LF | Tab.
    /// <para>
    /// “Whitespace” in this implementation includes those which delimit words, but not all characters that are categorically whitespace.
    /// For example, “non-breaking space” is whitespace, but it’s not what you want when splitting words, and so
    /// it is not considered whitespace for our purposes.
    /// </para>
    /// <para>* Only supported for splitting Words; ignored for Graphemes and Sentences. *</para>
    /// </summary>
    OmitWhitespace = 1,
}

internal static class OptionsExtensions
{
    internal static bool Includes(this Options options, Options compare)
    {
        return (options & compare) != 0;
    }
}