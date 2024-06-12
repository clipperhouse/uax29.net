namespace uax29;

public enum TokenType
{
	Words, Graphemes, Sentences
}

internal delegate int Split<TSpan>(ReadOnlySpan<TSpan> input, bool atEOF = true);

public static partial class Tokenizer
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

	static readonly Dictionary<TokenType, Split<byte>> byteSplits = new()
	{
		{TokenType.Words, Words.SplitUtf8Bytes},
		{TokenType.Graphemes, Graphemes.SplitUtf8Bytes},
		{TokenType.Sentences, Sentences.SplitUtf8Bytes},
	};

	static readonly Dictionary<TokenType, Split<char>> charSplits = new()
	{
		{TokenType.Words, Words.SplitChars},
		{TokenType.Graphemes, Graphemes.SplitChars},
		{TokenType.Sentences, Sentences.SplitChars},
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
		if (end < input.Length)
		{
			var advance = this.split(input[end..]);
			// Interpret as EOF
			if (advance == 0)
			{
				return false;
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
