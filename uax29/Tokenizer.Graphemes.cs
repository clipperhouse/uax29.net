namespace uax29;

public static partial class Tokenizer
{
    /// <summary>
    /// Split the graphemes in the given <see cref="Span"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> GetGraphemes(this Span<byte> input) => new(input, Graphemes.SplitUtf8Bytes);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> GetGraphemes(this ReadOnlySpan<byte> input) => new(input, Graphemes.SplitUtf8Bytes);

    /// <summary>
    /// Split the graphemes in the given <see cref="Memory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> GetGraphemes(this Memory<byte> input) => new(input.Span, Graphemes.SplitUtf8Bytes);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlyMemory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> GetGraphemes(this ReadOnlyMemory<byte> input) => new(input.Span, Graphemes.SplitUtf8Bytes);

    /// <summary>
    /// Split the graphemes in the given array of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> GetGraphemes(this byte[] input) => new(input.AsSpan(), Graphemes.SplitUtf8Bytes);

    /// <summary>
    /// Split the graphemes in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> GetGraphemes(this string input) => new(input.AsSpan(), Graphemes.SplitChars);

    /// <summary>
    /// Split the graphemes in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> GetGraphemes(this char[] input) => new(input.AsSpan(), Graphemes.SplitChars);

    /// <summary>
    /// Split the graphemes in the given <see cref="Span"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    /// 
    public static Tokenizer<char> GetGraphemes(this Span<char> input) => new(input, Graphemes.SplitChars);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlySpan"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> GetGraphemes(this ReadOnlySpan<char> input) => new(input, Graphemes.SplitChars);

    /// <summary>
    /// Split the graphemes in the given <see cref="Memory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> GetGraphemes(this Memory<char> input) => new(input.Span, Graphemes.SplitChars);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlyMemory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> GetGraphemes(this ReadOnlyMemory<char> input) => new(input.Span, Graphemes.SplitChars);

    /// <summary>
    /// Split the graphemes in the given <see cref="Stream"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
    /// <param name="maxTokenBytes">
    /// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
    /// Default is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
    /// Behind the scenes, a buffer of 2 * maxTokenSize will be created. If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
    /// </param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static StreamTokenizer<byte> GetGraphemes(this Stream stream, int maxTokenBytes = 1024)
    {
        var tok = new Tokenizer<byte>([], Graphemes.SplitUtf8Bytes);
        var buffer = new Buffer<byte>(stream.Read, maxTokenBytes);
        return new StreamTokenizer<byte>(buffer, tok);
    }

    /// <summary>
    /// Split the graphemes in the given <see cref="TextReader"/> / <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream/text reader of char to tokenize.</param>
    /// <param name="maxTokenBytes">
    /// Optional, the maximum token size in chars. Tokens that exceed this size will simply be cut off at this length, no error will occur.
    /// Default is 1024 chars. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
    /// Behind the scenes, a buffer of 2 * maxTokenSize will be created. If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
    /// </param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static StreamTokenizer<char> GetGraphemes(this TextReader stream, int maxTokenBytes = 1024)
    {
        var tok = new Tokenizer<char>([], Graphemes.SplitChars);
        var buffer = new Buffer<char>(stream.Read, maxTokenBytes);
        return new StreamTokenizer<char>(buffer, tok);
    }
}
