using System.ComponentModel;

namespace uax29;

public enum TokenType
{
	Words, Graphemes, Sentences
}

public ref struct Tokenizer
{
	readonly Span<byte> data;
	readonly SplitFunc split;

	int pos = 0;
	int start = 0;
	int end = 0;

	public Tokenizer(Span<byte> data, TokenType typ = TokenType.Words)
	{
		this.data = data;
		this.split = typ switch
		{
			TokenType.Words => Words.SplitFunc,
			TokenType.Graphemes => Graphemes.SplitFunc,
			TokenType.Sentences => Sentences.SplitFunc,
			_ => throw new InvalidEnumArgumentException()
		};
	}

	public bool MoveNext()
	{
		while (pos < data.Length)
		{
			var advance = split(data[pos..]);
			// Interpret as EOF
			if (advance == 0)
			{
				break;
			}

			start = pos;
			end = pos + advance;
			pos += advance;

			return true;
		}
		return false;
	}

	/// The current token
	public readonly Span<byte> Current
	{
		get
		{
			return data[start..end];
		}
	}
}
