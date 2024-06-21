namespace UAX29;

/// <summary>
/// The function that splits a string or UTF-8 byte array into tokens.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
/// <param name="input">The string to split/tokenize.</param>
/// <param name="atEOF">The split may need to know if further data to be expected, such as from a stream.</param>
/// <returns>How many bytes/chars were consumed from the input.</returns>
internal delegate int Split<T>(ReadOnlySpan<T> input, bool atEOF = true);

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct Tokenizer<T> where T : struct
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
	internal Tokenizer(ReadOnlySpan<T> input, Split<T> split)
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
	public readonly ReadOnlySpan<T> Current
	{
		get
		{
			return input[start..end];
		}
	}

	public readonly Tokenizer<T> GetEnumerator()
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
	/// Iterates over all tokens and collects them into a list. A new underlying array is allocated, and original input data is copied.
	/// </summary>
	/// <returns>List<ReadOnlyMemory<byte>> or List<ReadOnlyMemory<byte>>, depending on the input.</returns>
	public readonly List<ReadOnlyMemory<T>> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		var copy = this.input.ToArray();
		var tokenizer = new Tokenizer<T>(copy, this.split);

		var list = new List<ReadOnlyMemory<T>>();
		foreach (var token in tokenizer)
		{
			ReadOnlyMemory<T> mem = token.ToArray();
			list.Add(mem);
		}
		return list;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into an array. A new underlying array is allocated, and original input data is copied.
	/// </summary>
	/// <returns>ReadOnlyMemory<byte>[] or ReadOnlyMemory<byte>[], depending on the input.</returns>
	public readonly ReadOnlyMemory<T>[] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		return this.ToList().ToArray();
	}
}
