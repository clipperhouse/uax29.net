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
    ///
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