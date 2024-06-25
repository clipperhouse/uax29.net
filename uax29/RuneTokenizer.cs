using System.Buffers;
using System.Text;

namespace UAX29;

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct RuneTokenizer<T> where T : struct
{
	ReadOnlySpan<T> input;

	readonly Decoder<T> DecodeFirstRune;
	readonly Decoder<T> DecodeLastRune;

	internal int pos = 0;
	internal Rune rune;

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
	/// Move to the next token. Use <see cref="Current"/> to retrieve the rune.
	/// </summary>
	/// <returns>Whether there are any more runes. False typically means EOF.</returns>
	public bool MoveNext()
	{
		begun = true;

		if (pos >= input.Length)
		{
			return false;
		}

		var status = DecodeFirstRune(input[pos..], out this.rune, out int consumed);
		if (status != OperationStatus.Done)
		{
			// Garbage in, garbage out
			throw new InvalidOperationException("Rune could not be decoded");
		}

		pos += consumed;
		return true;
	}

	/// <summary>
	/// Move to the next token. Use <see cref="Current"/> to retrieve the rune.
	/// </summary>
	/// <returns>Whether there are any more runes. False typically means EOF.</returns>
	public bool MovePrevious()
	{
		if (pos == 0)
		{
			begun = false;
			return false;
		}

		var status = DecodeLastRune(input[..pos], out this.rune, out int consumed);
		if (status != OperationStatus.Done)
		{
			// Garbage in, garbage out
			throw new InvalidOperationException("Rune could not be decoded");
		}

		pos -= consumed;
		return true;
	}

	/// <summary>
	/// The current rune
	/// </summary>
	public readonly Rune Current
	{
		get
		{
			return this.rune;
		}
	}

	public readonly RuneTokenizer<T> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Resets the tokenizer back to the first rune.
	/// </summary>
	public void Reset()
	{
		this.pos = 0;
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
		foreach (var token in this)
		{
			result.Add(token);
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
