using System.Buffers;
using System.ComponentModel;
using System.Text;
namespace uax29;

public enum TokenType
{
	Words, Graphemes, Sentences
}

public delegate int Split(ReadOnlySpan<byte> input, bool atEOF = true);

/// <summary>
/// Tokenizer splits strings (bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// It accepts UTF-8 bytes, an returns an iterator.
/// </summary>
public ref struct Tokenizer
{
	readonly ReadOnlySpan<byte> input;
	readonly Split Split;

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
		this.Split = Words.Split;
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
	public readonly ReadOnlySpan<byte> Current
	{
		get
		{
			return input[start..end];
		}
	}
}
