using System.Buffers;
using System.ComponentModel;
using System.Text;
namespace uax29;

public enum TokenType
{
	Words, Graphemes, Sentences
}

/// <summary>
/// Tokenizer splits strings (bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// It accepts UTF-8 bytes, an returns an iterator.
/// </summary>
public ref struct Tokenizer
{
	readonly ReadOnlySpan<byte> input;
	readonly SplitFunc split;
	readonly Decoder FirstRune;
	readonly Decoder LastRune;
	readonly Dict dict;

	int start = 0;
	int end = 0;

	/// <summary>
	/// Tokenizer splits strings (bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A UTF-8 byte string</param>
	/// <param name="typ">Choose to split words, graphemes or sentences. Default is words.</param>
	public Tokenizer(ReadOnlySpan<byte> input, TokenType typ = TokenType.Words)
	{
		this.input = input;
		this.split = typ switch
		{
			TokenType.Words => Words.SplitFunc,
			TokenType.Graphemes => Graphemes.SplitFunc,
			TokenType.Sentences => Sentences.SplitFunc,
			_ => throw new InvalidEnumArgumentException(nameof(typ), (int)typ, typeof(TokenType))
		};
		this.dict = typ switch
		{
			TokenType.Words => Words.Dict,
			TokenType.Graphemes => Graphemes.dict,
			TokenType.Sentences => Sentences.dict,
			_ => throw new InvalidEnumArgumentException(nameof(typ), (int)typ, typeof(TokenType))
		};
		this.FirstRune = Rune.DecodeFromUtf8;
		this.LastRune = Rune.DecodeLastFromUtf8;
	}

	/// <summary>
	/// Move to the next token. Returns false when no more tokens (typically EOF). Use Current to retrieve the token.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		while (end < input.Length)
		{
			var advance = split(input[end..]);
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
	public readonly ReadOnlySpan<byte> Current
	{
		get
		{
			return input[start..end];
		}
	}
}
