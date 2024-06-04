namespace uax29;

using System.ComponentModel;
using System.Globalization;

public enum TokenType
{
	Words, Graphemes, Sentences
}

public delegate int Split<T>(ReadOnlySpan<T> input, bool atEOF = true);

public static class Tokenizer
{
	/// <summary>
	/// Create a tokenizer for a string, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="input">The string to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <returns>
	/// A tokenizer to iterate over, using <see cref="Tokenizer{T}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{T}.Current"/>.
	/// <see cref="Tokenizer{T}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="char"/>.
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
	/// A tokenizer to iterate over, using <see cref="Tokenizer{T}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{T}.Current"/>.
	/// <see cref="Tokenizer{T}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="byte"/>.
	/// </returns>
	public static Tokenizer<byte> Create(ReadOnlySpan<byte> input, TokenType tokenType = TokenType.Words)
	{
		return new Tokenizer<byte>(input, tokenType);
	}

	/// <summary>
	/// Create a tokenizer for a <see cref="ReadOnlySpan"/> of <see cref="char"/>, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="input">The string to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <returns>
	/// A tokenizer to iterate over, using <see cref="Tokenizer{T}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{T}.Current"/>.
	/// <see cref="Tokenizer{T}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="char"/>.
	/// </returns>
	public static Tokenizer<char> Create(ReadOnlySpan<char> input, TokenType tokenType = TokenType.Words)
	{
		return new Tokenizer<char>(input, tokenType);
	}
}

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// Use MoveNext to iterate, and Current to retrive the current token (i.e. word, grapheme or sentence).
/// </summary>
public ref struct Tokenizer<T> where T : struct
{
	readonly ReadOnlySpan<T> input;
	readonly Split<T> Split;
	public readonly TokenType TokenType;

	int start = 0;
	int end = 0;

	/// <summary>
	/// Tokenizer splits strings (bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A UTF-8 byte string</param>
	/// <param name="tokenType">Choose to split words, graphemes or sentences. Default is words.</param>
	internal Tokenizer(ReadOnlySpan<T> input, TokenType tokenType = TokenType.Words)
	{
		this.input = input;
		this.TokenType = tokenType;

		Type type = typeof(T);
		if (type == typeof(byte))
		{
			this.Split = tokenType switch
			{
				TokenType.Words => Words.SplitUtf8Bytes as Split<T>,
				TokenType.Graphemes => Graphemes.SplitUtf8Bytes as Split<T>,
				TokenType.Sentences => Sentences.SplitUtf8Bytes as Split<T>,
				// I wish T were inferrable simply by choice of the above generic delegates!
				_ => throw new InvalidEnumArgumentException(nameof(tokenType), (int)tokenType, typeof(TokenType))
			} ?? throw new NotImplementedException();
		}
		else if (type == typeof(char))
		{
			this.Split = tokenType switch
			{
				TokenType.Words => Words.SplitChars as Split<T>,
				TokenType.Graphemes => Graphemes.SplitChars as Split<T>,
				TokenType.Sentences => Sentences.SplitChars as Split<T>,
				_ => throw new InvalidEnumArgumentException(nameof(tokenType), (int)tokenType, typeof(TokenType))
			} ?? throw new NotImplementedException();
		}
		else
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Move to the next token. Returns false when no more tokens (typically EOF). Use Current to retrieve the token.
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
	/// The current token (word, grapheme or sentence) as UTF-8 bytes. Use Encoding.UTF8 to get a string.
	/// </summary>
	public readonly ReadOnlySpan<T> Current
	{
		get
		{
			return input[start..end];
		}
	}

	/// <summary>
	/// Resets the tokenizer to the first rune.
	/// </summary>
	public void Reset()
	{
		this.start = 0;
		this.end = 0;
	}
}
