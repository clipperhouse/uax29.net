namespace UAX29;

public static partial class Tokenizer
{
    /// <summary>
    /// Split the sentences in the given <see cref="Span"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<byte> GetSentences(Span<byte> input) => new(input, Sentences.SplitUtf8Bytes);

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes, according to the Unicode UAX #29 spec. https://unicode.org/reports/tr29/
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<byte> GetSentences(ReadOnlySpan<byte> input) => new(input, Sentences.SplitUtf8Bytes);

    /// <summary>
    /// Split the sentences in the given <see cref="Memory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<byte> GetSentences(Memory<byte> input) => new(input.Span, Sentences.SplitUtf8Bytes);

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlyMemory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<byte> GetSentences(ReadOnlyMemory<byte> input) => new(input.Span, Sentences.SplitUtf8Bytes);

    /// <summary>
    /// Split the sentences in the given array of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<byte> GetSentences(byte[] input) => new(input.AsSpan(), Sentences.SplitUtf8Bytes);

    /// <summary>
    /// Split the sentences in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<char> GetSentences(string input) => new(input.AsSpan(), Sentences.SplitChars);

    /// <summary>
    /// Split the sentences in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<char> GetSentences(char[] input) => new(input.AsSpan(), Sentences.SplitChars);

    /// <summary>
    /// Split the sentences in the given <see cref="Span"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    /// 
    public static Tokenizer<char> GetSentences(Span<char> input) => new(input, Sentences.SplitChars);

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlySpan"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<char> GetSentences(ReadOnlySpan<char> input) => new(input, Sentences.SplitChars);

    /// <summary>
    /// Split the sentences in the given <see cref="Memory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<char> GetSentences(Memory<char> input) => new(input.Span, Sentences.SplitChars);

    /// <summary>
    /// Split the sentences in the given <see cref="ReadOnlyMemory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static Tokenizer<char> GetSentences(ReadOnlyMemory<char> input) => new(input.Span, Sentences.SplitChars);



    /// <summary>
    /// Split the sentences in the given <see cref="Stream"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
    /// <param name="minBufferBytes">
    /// Optional, the minimum bytes to buffer from the Stream. This determines the maximum sentence token size. Tokens that exceed the bytes in the buffer
    /// will simply be cut off at this length, no error will occur.
    /// 
    /// Default is 1024 bytes.
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
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static StreamTokenizer<byte> GetSentences(Stream stream, int minBufferBytes = 1024, byte[]? bufferStorage = null)
    {
        bufferStorage ??= new byte[minBufferBytes * 2];
        var buffer = new Buffer<byte>(stream.Read, minBufferBytes, bufferStorage);
        return new StreamTokenizer<byte>(buffer, Sentences.SplitUtf8Bytes);
    }

    /// <summary>
    /// Split the sentences in the given <see cref="TextReader"/> / <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream/text reader of char to tokenize.</param>
    /// <param name="minBufferChars">
    /// Optional, the minimum chars to buffer from the reader. This determines the maximum sentence token size. Tokens that exceed the chars in the buffer
    /// will simply be cut off at this length, no error will occur.
    /// 
    /// Default is 1024 chars.
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
    /// An enumerator of sentences. Use foreach (var sentence in sentences).
    /// </returns>
    public static StreamTokenizer<char> GetSentences(TextReader stream, int minBufferChars = 1024, char[]? bufferStorage = null)
    {
        bufferStorage ??= new char[minBufferChars * 2];
        var buffer = new Buffer<char>(stream.Read, minBufferChars, bufferStorage);
        return new StreamTokenizer<char>(buffer, Sentences.SplitChars);
    }
}
