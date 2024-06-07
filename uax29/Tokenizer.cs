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
	/// A tokenizer to iterate over, using <see cref="Tokenizer{TSpan}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{TSpan}.Current"/>.
	/// <see cref="Tokenizer{TSpan}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="char"/>.
	/// </returns>
	public static Tokenizer<char> Create(string input, TokenType tokenType = TokenType.Words)
	{
		return new Tokenizer<char>(input.AsSpan(), tokenType);
	}

	/// <summary>
	/// Create a tokenizer for a <see cref="ReadOnlySpan"/> (or array) of UTF-8 encoded bytes, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="input">The UTF-8 bytes to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <returns>
	/// A tokenizer to iterate over, using <see cref="Tokenizer{TSpan}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{TSpan}.Current"/>.
	/// <see cref="Tokenizer{TSpan}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="byte"/>.
	/// </returns>
	public static Tokenizer<byte> Create(ReadOnlySpan<byte> input, TokenType tokenType = TokenType.Words)
	{
		return new Tokenizer<byte>(input, tokenType);
	}

	/// <summary>
	/// Create a tokenizer for a stream of UTF-8 encoded bytes, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <param name="maxTokenBytes">
	/// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
	/// Defaults to 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
	/// If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
	/// </param>
	/// <returns>
	/// A tokenizer to iterate over, using <see cref="StreamingTokenizer{TSpan}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{TSpan}.Current"/>.
	/// <see cref="StreamingTokenizer{TSpan}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="byte"/>.
	/// </returns>
	public static StreamingTokenizer Create(Stream stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		return new StreamingTokenizer(stream, tokenType, maxTokenBytes);
	}

	/// <summary>
	/// Create a tokenizer for a <see cref="ReadOnlySpan"/> of <see cref="char"/>, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="input">The string to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <returns>
	/// A tokenizer to iterate over, using <see cref="Tokenizer{TSpan}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{TSpan}.Current"/>.
	/// <see cref="Tokenizer{TSpan}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="char"/>.
	/// </returns>
	public static Tokenizer<char> Create(ReadOnlySpan<char> input, TokenType tokenType = TokenType.Words)
	{
		return new Tokenizer<char>(input, tokenType);
	}
}

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// Use <see cref="Tokenizer{TSpan}.MoveNext"/> to iterate, and <see cref="Tokenizer{TSpan}.Current"/> to retrive the current token (i.e. the word, grapheme or sentence).
/// </summary>
public ref struct Tokenizer<TSpan> where TSpan : struct
{
	ReadOnlySpan<TSpan> input;

	readonly Split<TSpan> Split;
	public readonly TokenType TokenType;

	int start = 0;
	int end = 0;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	/// <param name="tokenType">Choose to split words, graphemes or sentences. Default is words.</param>
	internal Tokenizer(ReadOnlySpan<TSpan> input, TokenType tokenType = TokenType.Words)
	{
		this.input = input;
		this.TokenType = tokenType;
		this.Split = ChooseSplit(tokenType);
	}

	static Split<TSpan> ChooseSplit(TokenType tokenType)
	{
		// I will type inference would make this unnecessary, but couldn't find a way.
		// Just encapsualting it to clean up the constructor.
		Type type = typeof(TSpan);
		if (type == typeof(byte))
		{
			return tokenType switch
			{
				TokenType.Words => Words.SplitUtf8Bytes as Split<TSpan>,
				TokenType.Graphemes => Graphemes.SplitUtf8Bytes as Split<TSpan>,
				TokenType.Sentences => Sentences.SplitUtf8Bytes as Split<TSpan>,
				// I wish T were inferrable simply by choice of the above generic delegates!
				_ => throw new InvalidEnumArgumentException(nameof(tokenType), (int)tokenType, typeof(TokenType))
			} ?? throw new NotImplementedException();
		}
		else if (type == typeof(char))
		{
			return tokenType switch
			{
				TokenType.Words => Words.SplitChars as Split<TSpan>,
				TokenType.Graphemes => Graphemes.SplitChars as Split<TSpan>,
				TokenType.Sentences => Sentences.SplitChars as Split<TSpan>,
				_ => throw new InvalidEnumArgumentException(nameof(tokenType), (int)tokenType, typeof(TokenType))
			} ?? throw new NotImplementedException();
		}
		else
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Move to the next token. Use <see cref="Current"/> to retrieve the token.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		while (end < input.Length)
		{
			var advance = this.Split(input[end..]);
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
