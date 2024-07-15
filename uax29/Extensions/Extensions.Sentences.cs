namespace UAX29.Extensions;
using UAX29;

public static partial class Extensions
{
    /// <summary>
    /// Split the graphemes in the given <see cref="Span"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> SplitSentences(this Span<byte> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> SplitSentences(this ReadOnlySpan<byte> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="Memory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> SplitSentences(this Memory<byte> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlyMemory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> SplitSentences(this ReadOnlyMemory<byte> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given array of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<byte> SplitSentences(this byte[] input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> SplitSentences(this string input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> SplitSentences(this char[] input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="Span"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    ///
    public static Tokenizer<char> SplitSentences(this Span<char> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlySpan"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> SplitSentences(this ReadOnlySpan<char> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="Memory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> SplitSentences(this Memory<char> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="ReadOnlyMemory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static Tokenizer<char> SplitSentences(this ReadOnlyMemory<char> input) => Tokenizer.GetSentences(input);

    /// <summary>
    /// Split the graphemes in the given <see cref="Stream"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
    /// <param name="minBufferBytes">
    /// Optional, the minimum bytes to buffer from the Stream. This determines the maximum grapheme token size. Tokens that exceed the bytes in the buffer
    /// will simply be cut off at this length, no error will occur.
    ///
    /// Default is 256 bytes.
    /// </param>
    /// <param name="bufferStorage">
    /// Optional, a byte array for underlying buffer storage. It must be at least as large at minBufferBytes.
    ///
    /// If not provided, storage of 2 * minBufferBytes will be allocated by default.
    ///
    /// This parameter is a choice about performance and memory usage. A buffer larger than minBufferBytes allows fewer, larger reads the stream,
    /// which is more efficient, but will increase memory usage.
    ///
    /// You might also wish to use ArrayPool<byte> to reuse the storage and minimize allocations.
    /// </param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static StreamTokenizer<byte> SplitSentences(this Stream stream, int minBufferBytes = 1024, byte[]? bufferStorage = null) => Tokenizer.GetSentences(stream, minBufferBytes, bufferStorage);

    /// <summary>
    /// Split the graphemes in the given <see cref="TextReader"/> / <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream/text reader of char to tokenize.</param>
    /// <param name="minBufferChars">
    /// Optional, the minimum chars to buffer from the reader. This determines the maximum grapheme token size. Tokens that exceed the chars in the buffer
    /// will simply be cut off at this length, no error will occur.
    ///
    /// Default is 256 chars.
    /// </param>
    /// <param name="bufferStorage">
    /// Optional, a char array for underlying buffer storage. It must be at least as large at minBufferChars.
    ///
    /// If not provided, storage of 2 * minBufferChars will be allocated by default.
    ///
    /// This parameter is a choice about performance and memory usage. A buffer larger than minBufferChars allows fewer, larger reads the reader,
    /// which is more efficient, but will increase memory usage.
    ///
    /// You might also wish to use ArrayPool<char> to reuse the storage and minimize allocations.
    /// </param>
    /// <returns>
    /// An enumerator of graphemes. Use foreach (var grapheme in graphemes).
    /// </returns>
    public static StreamTokenizer<char> SplitSentences(this TextReader stream, int minBufferChars = 1024, char[]? bufferStorage = null) => Tokenizer.GetSentences(stream, minBufferChars, bufferStorage);
}
