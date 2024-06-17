﻿namespace uax29;

public static partial class Tokenizer
{
    /// <summary>
    /// Create an enumerator of words in the given a <see cref="Span"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> GetWords(this Span<byte> input) => new(input, Words.SplitUtf8Bytes);

    /// <summary>
    /// Create an enumerator of words in the given a <see cref="ReadOnlySpan"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> GetWords(this ReadOnlySpan<byte> input) => new(input, Words.SplitUtf8Bytes);

    /// <summary>
    /// Create an enumerator of words in the given a <see cref="Memory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>

    public static Tokenizer<byte> GetWords(this Memory<byte> input) => new(input.Span, Words.SplitUtf8Bytes);

    /// <summary>
    /// Create an enumerator of words in the given a <see cref="ReadOnlyMemory"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> GetWords(this ReadOnlyMemory<byte> input) => new(input.Span, Words.SplitUtf8Bytes);

    /// <summary>
    /// Create an enumerator of words in the given an array of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="input">The UTF-8 bytes to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<byte> GetWords(this byte[] input) => new(input.AsSpan(), Words.SplitUtf8Bytes);

    /// <summary>
    /// Create an enumerator of words in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> GetWords(this string input) => new(input.AsSpan(), Words.SplitChars);

    /// <summary>
    /// Create an enumerator of words in the given string.
    /// </summary>
    /// <param name="input">The string to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> GetWords(this char[] input) => new(input.AsSpan(), Words.SplitChars);

    /// <summary>
    /// Create an enumerator of words in the given <see cref="Span"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    /// 
    public static Tokenizer<char> GetWords(this Span<char> input) => new(input, Words.SplitChars);

    /// <summary>
    /// Create an enumerator of words in the given <see cref="ReadOnlySpan"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> GetWords(this ReadOnlySpan<char> input) => new(input, Words.SplitChars);

    /// <summary>
    /// Create an enumerator of words in the given <see cref="Memory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> GetWords(this Memory<char> input) => new(input.Span, Words.SplitChars);

    /// <summary>
    /// Create an enumerator of words in the given <see cref="ReadOnlyMemory"/> of <see cref="char"/>.
    /// </summary>
    /// <param name="input">The chars to tokenize.</param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static Tokenizer<char> GetWords(this ReadOnlyMemory<char> input) => new(input.Span, Words.SplitChars);

    /// <summary>
    /// Create an enumerator of words in the given <see cref="Stream"/> of UTF-8 encoded bytes.
    /// </summary>
    /// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
    /// <param name="maxTokenBytes">
    /// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
    /// Default is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
    /// Behind the scenes, a buffer of 2 * maxTokenSize will be created. If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
    /// </param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static StreamTokenizer<byte> GetWords(this Stream stream, int maxTokenBytes = 1024)
    {
        var tok = new Tokenizer<byte>([], Words.SplitUtf8Bytes);
        var buffer = new Buffer<byte>(stream.Read, maxTokenBytes);
        return new StreamTokenizer<byte>(buffer, tok);
    }

    /// <summary>
    /// Create an enumerator of words in the given <see cref="TextReader"/> / <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="stream">The stream/text reader of char to tokenize.</param>
    /// <param name="maxTokenBytes">
    /// Optional, the maximum token size in chars. Tokens that exceed this size will simply be cut off at this length, no error will occur.
    /// Default is 1024 chars. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
    /// Behind the scenes, a buffer of 2 * maxTokenSize will be created. If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
    /// </param>
    /// <returns>
    /// An enumerator of words. Use foreach (var word in words).
    /// </returns>
    public static StreamTokenizer<char> GetWords(this TextReader stream, int maxTokenBytes = 1024)
    {
        var tok = new Tokenizer<char>([], Words.SplitChars);
        var buffer = new Buffer<char>(stream.Read, maxTokenBytes);
        return new StreamTokenizer<char>(buffer, tok);
    }
}