namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

/// <summary>
/// RangeEnumerator splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct RangeEnumerator<T> where T : struct
{
	SplitEnumerator<T> tokenizer;
	bool begun = false;

	/// <summary>
	/// RangeEnumerator splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	internal RangeEnumerator(SplitEnumerator<T> tokenizer)
	{
		this.tokenizer = tokenizer;
	}

	/// <summary>
	/// Move to the next token. Use <see cref="Current"/> to retrieve the Range.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		begun = true;
		return tokenizer.MoveNext();
	}

	/// <summary>
	/// Range of the current token in the original input string
	/// </summary>
	public readonly Range Current
	{
		get
		{
			return new Range(tokenizer.start, tokenizer.end);
		}
	}

	public readonly RangeEnumerator<T> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Iterate over all ranges and collect them into a list
	/// </summary>
	/// <returns>List[Range]</returns>
	public readonly List<Range> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun. You may wish to call Reset() on the enumerator.");
		}

		var result = new List<Range>();
		foreach (var range in this)
		{
			result.Add(range);
		}

		return result;
	}

	/// <summary>
	/// Iterate over all ranges and collect them into an array.
	/// </summary>
	/// <returns>Range[]</returns>
	public readonly Range[] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun. You may wish to call Reset() on the tokenizer.");
		}

		return this.ToList().ToArray();
	}
}
