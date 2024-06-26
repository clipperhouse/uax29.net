using System.Buffers;
using System.Text;

namespace UAX29;

internal static class RuneTokenizer
{
	internal static RuneTokenizer<char> Create(ReadOnlySpan<char> input)
	{
		return new RuneTokenizer<char>(input, Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);
	}

	internal static RuneTokenizer<byte> Create(ReadOnlySpan<byte> input)
	{
		return new RuneTokenizer<byte>(input, Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8);
	}
}

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct RuneTokenizer<T> where T : struct
{
	internal ReadOnlySpan<T> input;

	readonly Decoder<T> DecodeFirstRune;
	readonly Decoder<T> DecodeLastRune;

	int start = 0;
	int end = 0;

	bool begun = false;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	/// <param name="tokenType">Choose to split words, graphemes or sentences. Default is words.</param>
	internal RuneTokenizer(ReadOnlySpan<T> input, Decoder<T> decodeFirstRune, Decoder<T> decodeLastRune)
	{
		this.input = input;
		this.DecodeFirstRune = decodeFirstRune;
		this.DecodeLastRune = decodeLastRune;
	}

	/// <summary>
	/// Move to the next rune. Use <see cref="Current"/> to retrieve the rune.
	/// </summary>
	/// <returns>Whether there are any more runes. False typically means EOF.</returns>
	public bool MoveNext()
	{
		begun = true;

		if (!Any())
		{
			start = end;
			return false;
		}

		var status = DecodeFirstRune(input[end..], out _, out int consumed);
		if (status != OperationStatus.Done)
		{
			// Garbage in, garbage out
			throw new InvalidOperationException("Rune could not be decoded");
		}

		start = end;
		end += consumed;
		return true;
	}

	internal bool Any()
	{
		return end < input.Length;
	}

	/// <summary>
	/// Move to the previous rune. Use <see cref="Current"/> to retrieve the rune.
	/// </summary>
	/// <returns>Whether there are any more runes. False typically means the beginning of the string.</returns>
	public bool MovePrevious()
	{
		if (start == 0)
		{
			begun = false;
			end = start;
			return false;
		}

		var status = DecodeLastRune(input[..start], out _, out int consumed);
		if (status != OperationStatus.Done)
		{
			// Garbage in, garbage out
			throw new InvalidOperationException("Rune could not be decoded");
		}

		end = start;
		start -= consumed;

		return true;
	}

	/// <summary>
	/// The current rune
	/// </summary>
	public readonly Rune Current
	{
		get
		{
			DecodeLastRune(input[start..end], out Rune rune, out int _);
			return rune;
		}
	}

	public readonly int CurrentWidth
	{
		get
		{
			return end - start;
		}
	}

	internal void Consume(int consumed)
	{
		start += consumed;
		end = start;
	}

	// public readonly RuneTokenizer<T> GetEnumerator()
	// {
	// 	return this;
	// }

	/// <summary>
	/// Resets the tokenizer back to the first rune.
	/// </summary>
	public void Reset()
	{
		this.end = 0;
		this.begun = false;
	}

	/// <summary>
	/// (Re)sets the text to be tokenized, and resets the iterator back to the the start.
	/// </summary>
	public void SetText(ReadOnlySpan<T> input)
	{
		Reset();
		this.input = input;
	}

	/// <summary>
	/// Iterates over all runes and collects them into a list, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input</returns>
	public List<Rune> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		var result = new List<Rune>();
		while (MoveNext())
		{
			result.Add(Current);
		}

		this.Reset();
		return result;
	}

	/// <summary>
	/// Iterates over all runes and collects them into an array, allocating a new array for each token.
	/// </summary>
	/// <returns>byte[][] or char[][], depending on the input</returns>
	public Rune[] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		return this.ToList().ToArray();
	}
}
