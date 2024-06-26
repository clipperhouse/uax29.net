namespace UAX29;

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct RangeTokenizer<T> where T : struct
{
	ReadOnlySpan<T> input;

	readonly Split<T> split;

	internal int start = 0;
	internal int end = 0;

	bool begun = false;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	/// <param name="tokenType">Choose to split words, graphemes or sentences. Default is words.</param>
	internal RangeTokenizer(ReadOnlySpan<T> input, Split<T> split)
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
		begun = true;

		if (end < input.Length)
		{
			var advance = this.split(input[end..], true);
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
	public readonly Range Current
	{
		get
		{
			return new Range(start, end);
		}
	}

	public readonly RangeTokenizer<T> GetEnumerator()
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
	/// Iterates over all tokens and collects them into a list, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input</returns>
	public List<Range> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		var result = new List<Range>();
		foreach (var token in this)
		{
			result.Add(token);
		}

		this.Reset();
		return result;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into an array, allocating a new array for each token.
	/// </summary>
	/// <returns>byte[][] or char[][], depending on the input</returns>
	public Range[] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		return this.ToList().ToArray();
	}
}
