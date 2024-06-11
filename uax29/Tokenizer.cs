namespace uax29;

using System.ComponentModel;

public enum TokenType
{
	Words, Graphemes, Sentences
}

public delegate int Split<TSpan>(ReadOnlySpan<TSpan> input, bool atEOF = true);

public static class Tokenizer
{
	/// <summary>
	/// Create a tokenizer for a string, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="input">The string to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <returns>
	/// An enumerator of tokens. Use foreach (var token in tokens).
	/// </returns>
	public static Tokenizer<char> Create(string input, TokenType tokenType = TokenType.Words)
	{
		var split = charSplits[tokenType];
		return new Tokenizer<char>(input.AsSpan(), split);
	}

	/// <summary>
	/// Create a tokenizer for a <see cref="ReadOnlySpan"/> (or array) of UTF-8 encoded bytes, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="input">The UTF-8 bytes to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <returns>
	/// An enumerator of tokens. Use foreach (var token in tokens).
	/// </returns>
	public static Tokenizer<byte> Create(ReadOnlySpan<byte> input, TokenType tokenType = TokenType.Words)
	{
		var split = byteSplits[tokenType];
		return new Tokenizer<byte>(input, split);
	}

	/// <summary>
	/// Create a tokenizer for a <see cref="ReadOnlySpan"/> of <see cref="char"/>, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="input">The string to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <returns>
	/// An enumerator of tokens. Use foreach (var token in tokens).
	/// </returns>
	public static Tokenizer<char> Create(ReadOnlySpan<char> input, TokenType tokenType = TokenType.Words)
	{
		var split = charSplits[tokenType];
		return new Tokenizer<char>(input, split);
	}

	/// <summary>
	/// Create a tokenizer for a stream of UTF-8 encoded bytes, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <param name="maxTokenBytes">
	/// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
	/// Default is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
	/// If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
	/// </param>
	/// <returns>
	/// An enumerator of tokens. Use foreach (var token in tokens).
	/// </returns>
	/// 
	public static StreamTokenizer<byte> Create(Stream stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		var tok = Create(ReadOnlySpan<byte>.Empty, tokenType);
		var buffer = new Buffer<byte>(stream.Read, maxTokenBytes);
		return new StreamTokenizer<byte>(buffer, tok, maxTokenBytes);
	}

	/// <summary>
	/// Create a tokenizer for a stream reader of char, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="stream">The stream reader of char to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <param name="maxTokenBytes">
	/// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
	/// Default is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
	/// If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
	/// </param>
	/// <returns>
	/// An enumerator of tokens. Use foreach (var token in tokens).
	/// </returns>
	public static StreamTokenizer<char> Create(TextReader stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		var tok = Create(ReadOnlySpan<char>.Empty, tokenType);
		var buffer = new Buffer<char>(stream.Read, maxTokenBytes);
		return new StreamTokenizer<char>(buffer, tok, maxTokenBytes);
	}

	static readonly Dictionary<TokenType, Split<byte>> byteSplits = new()
	{
		{TokenType.Words, Words.SplitUtf8Bytes},
		{TokenType.Graphemes , Graphemes.SplitUtf8Bytes},
		{TokenType.Sentences , Sentences.SplitUtf8Bytes},
	};

	static readonly Dictionary<TokenType, Split<char>> charSplits = new()
	{
		{TokenType.Words, Words.SplitChars},
		{TokenType.Graphemes , Graphemes.SplitChars},
		{TokenType.Sentences , Sentences.SplitChars},
	};
}

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
public ref struct Tokenizer<TSpan> where TSpan : struct
{
	ReadOnlySpan<TSpan> input;

	readonly Split<TSpan> split;

	int start = 0;
	int end = 0;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	/// <param name="tokenType">Choose to split words, graphemes or sentences. Default is words.</param>
	internal Tokenizer(ReadOnlySpan<TSpan> input, Split<TSpan> split)
	{
		this.input = input;
		this.split = split;
	}

	/// <summary>
	/// Move to the next token. Use <see cref="Current"/> to retrieve the token.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		while (end < input.Length)
		{
			var advance = this.split(input[end..]);
			// Interpret as EOF
			if (advance == 0)
			{
				break;
			}

			start = end;
			end = start + advance;

			return true;
		}
		return false;
	}

	/// <summary>
	/// The current token (word, grapheme or sentence).
	/// If the input was a string, <see cref="Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="char"/>.
	/// If the input was UTF-8 bytes, <see cref="Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="byte"/>.
	/// </summary>
	public readonly ReadOnlySpan<TSpan> Current
	{
		get
		{
			return input[start..end];
		}
	}

	public readonly Tokenizer<TSpan> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Resets the tokenizer back to the first token.
	/// </summary>
	public void Reset()
	{
		this.start = 0;
		this.end = 0;
	}

	/// <summary>
	/// (Re)sets the text to be tokenized, and resets the iterator back to the the start.
	/// </summary>
	public void SetText(ReadOnlySpan<TSpan> input)
	{
		Reset();
		this.input = input;
	}
}
