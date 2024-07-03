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
        return new Tokenizer2<byte>(runes, Sentences.Split);
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
        return new Tokenizer2<byte>(runes, Sentences.Split);
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
        return new Tokenizer2<byte>(runes, Sentences.Split);
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
        return new Tokenizer2<char>(runes, Sentences.Split);
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
        return new Tokenizer2<char>(runes, Sentences.Split);
    }


    /// <summary>
    /// Split the sentences in the given <see cref="Stream"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
    /// <param name="minBufferBytes">
    /// Optional, the minimum bytes to buffer from the Stream. This determines the maximum sentence token size. Tokens that exceed the bytes in the buffer
    /// will simply be cut off at this length, no error will occur.
    /// 
    /// Default is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a sentence beyond a couple of dozen bytes.
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
    public static StreamTokenizer2<byte> GetSentences(Stream stream, int minBufferBytes = 1024, byte[]? bufferStorage = null)
    {
        bufferStorage ??= new byte[minBufferBytes * 2];
        var buffer = new Buffer<byte>(stream.Read, minBufferBytes, bufferStorage);
        return new StreamTokenizer2<byte>(buffer, RuneTokenizer.Create, Sentences.Split);
    }

    /// <summary>
    /// Split the sentences in the given <see cref="TextReader"/> / <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream/text reader of char to tokenize.</param>
    /// <param name="minBufferChars">
    /// Optional, the minimum chars to buffer from the reader. This determines the maximum sentence token size. Tokens that exceed the chars in the buffer
    /// will simply be cut off at this length, no error will occur.
    /// 
    /// Default is 1024 chars. The tokenizer is intended for natural language, so we don't expect you'll find text with a sentence beyond a few dozen chars.
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
    public static StreamTokenizer2<char> GetSentences(TextReader stream, int minBufferChars = 1024, char[]? bufferStorage = null)
    {
        bufferStorage ??= new char[minBufferChars * 2];
        var buffer = new Buffer<char>(stream.Read, minBufferChars, bufferStorage);
        return new StreamTokenizer2<char>(buffer, RuneTokenizer.Create, Sentences.Split);
    }
}
