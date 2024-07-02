namespace UAX29;

public static partial class Tokenizer2
{
    /// <summary>
    /// Split the words in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer2<byte> GetWords(byte[] input)
    {
        var runes = RuneTokenizer.Create(input);
        return new Tokenizer2<byte>(runes, Words.Split);
    }

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer2<byte> GetWords(ReadOnlySpan<byte> input)
    {
        var runes = RuneTokenizer.Create(input);
        return new Tokenizer2<byte>(runes, Words.Split);
    }

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlyMemory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer2<byte> GetWords(ReadOnlyMemory<byte> input)
    {
        var runes = RuneTokenizer.Create(input.Span);
        return new Tokenizer2<byte>(runes, Words.Split);
    }

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlySpan"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer2<char> GetWords(ReadOnlySpan<char> input)
    {
        var runes = RuneTokenizer.Create(input);
        return new Tokenizer2<char>(runes, Words.Split);
    }

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlyMemory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer2<char> GetWords(ReadOnlyMemory<char> input)
    {
        var runes = RuneTokenizer.Create(input.Span);
        return new Tokenizer2<char>(runes, Words.Split);
    }
}
