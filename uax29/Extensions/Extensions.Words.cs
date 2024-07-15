namespace UAX29.Extensions;
using UAX29;

public static partial class Extensions
{
    /// <summary>
    /// Split the words in the given <see cref="Span"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> SplitWords(this Span<byte> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> SplitWords(this ReadOnlySpan<byte> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="Memory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> SplitWords(this Memory<byte> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlyMemory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> SplitWords(this ReadOnlyMemory<byte> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given array of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> SplitWords(this byte[] input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> SplitWords(this string input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> SplitWords(this char[] input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="Span"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    ///
    public static Tokenizer<char> SplitWords(this Span<char> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlySpan"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> SplitWords(this ReadOnlySpan<char> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="Memory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> SplitWords(this Memory<char> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="ReadOnlyMemory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> SplitWords(this ReadOnlyMemory<char> input, Options options = Options.None) => Tokenizer.GetWords(input, options);

    /// <summary>
    /// Split the words in the given <see cref="Stream"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
    /// <param name="minBufferBytes">
    /// Optional, the minimum bytes to buffer from the Stream. This determines the maximum word token size. Tokens that exceed the bytes in the buffer
    /// will simply be cut off at this length, no error will occur.
    ///
    /// Default is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a word beyond a couple of dozen bytes.
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
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static StreamTokenizer<byte> SplitWords(this Stream stream, Options options = Options.None, int minBufferBytes = 1024, byte[]? bufferStorage = null)
        => Tokenizer.GetWords(stream, options, minBufferBytes, bufferStorage);

    /// <summary>
    /// Split the words in the given <see cref="TextReader"/> / <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream/text reader of char to tokenize.</param>
    /// <param name="minBufferChars">
    /// Optional, the minimum chars to buffer from the reader. This determines the maximum word token size. Tokens that exceed the chars in the buffer
    /// will simply be cut off at this length, no error will occur.
    ///
    /// Default is 1024 chars. The tokenizer is intended for natural language, so we don't expect you'll find text with a word beyond a few dozen chars.
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
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static StreamTokenizer<char> SplitWords(this TextReader stream, Options options = Options.None, int minBufferChars = 1024, char[]? bufferStorage = null)
      => Tokenizer.GetWords(stream, options, minBufferChars, bufferStorage);
}
