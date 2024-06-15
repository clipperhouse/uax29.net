namespace uax29;

public static partial class Tokenizer
{
	/// <summary>
	/// Create a tokenizer for a stream of UTF-8 encoded bytes, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <param name="maxTokenBytes">
	/// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
	/// Default is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
	/// Behind the scenes, a buffer of 2 * maxTokenSize will be created. If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
	/// </param>
	/// <returns>
	/// An enumerator of tokens. Use foreach (var token in tokens).
	/// </returns>
	public static StreamTokenizer<byte> Create(Stream stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		var tok = Create(ReadOnlySpan<byte>.Empty, tokenType);
		var buffer = new Buffer<byte>(stream.Read, maxTokenBytes);
		return new StreamTokenizer<byte>(buffer, tok);
	}

	/// <summary>
	/// Create a tokenizer for a stream reader of char, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="stream">The stream/text reader of char to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <param name="maxTokenBytes">
	/// Optional, the maximum token size in chars. Tokens that exceed this size will simply be cut off at this length, no error will occur.
	/// Default is 1024 chars. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
	/// Behind the scenes, a buffer of 2 * maxTokenSize will be created. If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
	/// </param>
	/// <returns>
	/// An enumerator of tokens. Use foreach (var token in tokens).
	/// </returns>
	public static StreamTokenizer<char> Create(TextReader stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		var tok = Create(ReadOnlySpan<char>.Empty, tokenType);
		var buffer = new Buffer<char>(stream.Read, maxTokenBytes);
		return new StreamTokenizer<char>(buffer, tok);
	}
}

/// <summary>
/// Tokenizer splits a stream of UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
public ref struct StreamTokenizer<T> where T : struct
{
	internal Tokenizer<T> tok;

	internal Buffer<T> buffer;

	bool begun = false;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="stream">A stream of UTF-8 encoded bytes.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	internal StreamTokenizer(Buffer<T> buffer, Tokenizer<T> tok)
	{
		this.tok = tok;
		this.buffer = buffer;
	}

	public bool MoveNext()
	{
		begun = true;

		buffer.Consume(tok.Current.Length); // the previous token
		var input = buffer.Contents;
		tok.SetText(input);
		return tok.MoveNext();
	}

	public readonly ReadOnlySpan<T> Current => tok.Current;

	public readonly StreamTokenizer<T> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into a List, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input</returns>
	public readonly List<T[]> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun.");
		}

		var result = new List<T[]>();
		foreach (var token in this)
		{
			result.Add(token.ToArray());
		}

		return result;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into an Array, allocating a new array for each token.
	/// </summary>
	/// <returns>byte[][] or char[][], depending on the input</returns>
	public readonly T[][] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun.");
		}

		return this.ToList().ToArray();
	}
}

public static class StreamExtensions
{
	/// <summary>
	/// Resets an existing tokenizer with a new stream. You might choose this as an optimization, as it will re-use a buffer, avoiding allocations.
	/// </summary>
	/// <param name="stream">The new stream</param>
	public static void SetStream(ref this StreamTokenizer<byte> tokenizer, Stream stream)
	{
		tokenizer.tok.SetText([]);
		tokenizer.buffer.SetRead(stream.Read);
	}

	/// <summary>
	/// Resets an existing tokenizer with a new stream. You might choose this as an optimization, as it will re-use a buffer, avoiding allocations.
	/// </summary>
	/// <param name="stream">The new stream</param>
	public static void SetStream(ref this StreamTokenizer<char> tokenizer, TextReader stream)
	{
		tokenizer.tok.SetText([]);
		tokenizer.buffer.SetRead(stream.Read);
	}
}
