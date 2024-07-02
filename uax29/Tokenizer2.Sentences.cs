namespace UAX29;

public static partial class Tokenizer2
{
    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer2<byte> GetSentences(byte[] input)
    {
        var runes = RuneTokenizer.Create(input);
        return new Tokenizer2<byte>(runes, Sentences.Split2Utf8Bytes);
    }

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer2<byte> GetSentences(ReadOnlySpan<byte> input)
    {
        var runes = RuneTokenizer.Create(input);
        return new Tokenizer2<byte>(runes, Sentences.Split2Utf8Bytes);
    }

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlyMemory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer2<byte> GetSentences(ReadOnlyMemory<byte> input)
    {
        var runes = RuneTokenizer.Create(input.Span);
        return new Tokenizer2<byte>(runes, Sentences.Split2Utf8Bytes);
    }

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlySpan"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer2<char> GetSentences(ReadOnlySpan<char> input)
    {
        var runes = RuneTokenizer.Create(input);
        return new Tokenizer2<char>(runes, Sentences.Split2Chars);
    }

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlyMemory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer2<char> GetSentences(ReadOnlyMemory<char> input)
    {
        var runes = RuneTokenizer.Create(input.Span);
        return new Tokenizer2<char>(runes, Sentences.Split2Chars);
    }
}
