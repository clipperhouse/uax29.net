using System.Diagnostics;

namespace UAX29;

/// <summary>
/// Splits an input string (UTF-8 or UTF-16) and provides an enumerator over the splits.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct SplitEnumerator<T> where T : struct
{
	ReadOnlySpan<T> input;

	readonly Split<T> split;

	internal int start = 0;

	/// <summary>
	/// The byte or char position of the current token in the original string.
	/// </summary>
	public readonly int Position => start;

	internal int end = 0;

	readonly Options options;

	bool begun = false;

	/// <summary>
	/// Splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	/// <param name="split">A func/method meeting the Split delegate signature.</param>
	/// <param name="options">Options for handling the input text.</param>
	internal SplitEnumerator(ReadOnlySpan<T> input, Split<T> split, Options options = Options.None)
	{
		this.input = input;
		this.split = split;
		this.options = options;
	}

	/// <summary>
	/// Move to the next token. Use <see cref="Current"/> to retrieve the token.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		begun = true;

		while (end < input.Length)
		{
			var advance = this.split(input[end..], out var whitespace);
			Debug.Assert(advance > 0);

			start = end;
			end += advance;

			// This option is only supported for words; prevent other uses at the static API level
			if (whitespace && options.Includes(Options.OmitWhitespace))
			{
				continue;
			}

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

	public readonly SplitEnumerator<T> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Resets the enumerator back to the first token.
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
	/// Iterate over all tokens and collects them into a list, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input</returns>
	public List<T[]> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		var result = new List<T[]>();
		foreach (var token in this)
		{
			result.Add(token.ToArray());
		}

		this.Reset();
		return result;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into an array, allocating a new array for each token.
	/// </summary>
	/// <returns>byte[][] or char[][], depending on the input</returns>
	public T[][] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		return this.ToList().ToArray();
	}

	/// <summary>
	/// Get the ranges (boundaries) of the tokens.
	/// </summary>
	/// <returns>
	/// An enumerator of Range. Use foreach to iterate over the ranges. Apply them to your original input
	/// using [range] or .AsSpan(range) to get the tokens.
	/// </returns>
	public readonly RangeEnumerator<T> Ranges
	{
		get
		{
			return new RangeEnumerator<T>(this);
		}
	}
}
