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

	/// The current token
	public readonly Span<byte> Current
	{
		get
		{
			return data[start..end];
		}
	}
}
