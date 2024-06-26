using System.Collections.Immutable;

namespace UAX29;

/// <summary>
/// The function that splits a string or UTF-8 byte array into tokens.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
/// <param name="input">The string to split/tokenize.</param>
/// <param name="atEOF">The split may need to know if further data to be expected, such as from a stream.</param>
/// <returns>How many bytes/chars were consumed from the input.</returns>
internal delegate int Split2<T>(RuneTokenizer<T> input, bool atEOF = true) where T : struct;

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct Tokenizer2<T> where T : struct
{
	RuneTokenizer<T> runes;

	readonly Split2<T> split;

	internal int start = 0;
	internal int end = 0;

	bool begun = false;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="runes">A string, or UTF-8 byte array.</param>
	internal Tokenizer2(RuneTokenizer<T> runes, Split2<T> split)
	{
		this.runes = runes;
		this.split = split;
	}

	/// <summary>
	/// Move to the next token. Use <see cref="Current"/> to retrieve the token.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		if (!runes.Any())
		{
			return false;
		}

		begun = true;

		var advance = this.split(runes);
		if (advance == 0)
		{
			return false;
		}

		start = end;
		end += advance;

		runes.Consume(advance);

		return true;
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
			return runes.input[start..end];
		}
	}

	public readonly Tokenizer2<T> GetEnumerator()
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
	public void SetText(RuneTokenizer<T> runes)
	{
		Reset();
		this.runes = runes;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into a list, allocating a new array for each token.
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
	// public RangeTokenizer<T> Ranges
	// {
	// 	get
	// 	{
	// 		return new RangeTokenizer<T>(input, split);
	// 	}
	// }
}
