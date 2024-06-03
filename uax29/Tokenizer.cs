using System.ComponentModel;

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
	readonly Span<byte> data;
	readonly SplitFunc split;

	int start = 0;
	int end = 0;

	/// <summary>
	/// Tokenizer splits strings (bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="data">A UTF-8 byte string</param>
	/// <param name="typ">Choose to split words, graphemes or sentences. Default is words.</param>
	public Tokenizer(Span<byte> data, TokenType typ = TokenType.Words)
	{
		this.data = data;
		this.split = typ switch
		{
			TokenType.Words => Words.SplitFunc,
			TokenType.Graphemes => Graphemes.SplitFunc,
			TokenType.Sentences => Sentences.SplitFunc,
			_ => throw new InvalidEnumArgumentException(nameof(typ), (int)typ, typeof(TokenType))
		};
	}

	/// <summary>
	/// Move to the next token. Returns false when no more tokens (typically EOF). Use Current to retrieve the token.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		while (end < data.Length)
		{
			var advance = split(data[end..]);
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
	public readonly Span<byte> Current
	{
		get
		{
			return data[start..end];
		}
	}
}
