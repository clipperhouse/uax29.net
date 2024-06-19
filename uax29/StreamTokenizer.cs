using Buffer;

namespace uax29;

/// <summary>
/// Tokenizer splits a stream of UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
public ref struct StreamTokenizer<T> where T : struct
{
	internal Tokenizer<T> tok;

	internal Buffer<T> buffer;

	bool begun = false;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="stream">A stream of UTF-8 encoded bytes.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	internal StreamTokenizer(Buffer<T> buffer, Tokenizer<T> tok)
	{
		this.tok = tok;
		this.buffer = buffer;
	}

	public bool MoveNext()
	{
		begun = true;

		buffer.Consume(tok.Current.Length); // the previous token
		var input = buffer.Contents;
		tok.SetText(input);
		return tok.MoveNext();
	}

	public readonly ReadOnlySpan<T> Current => tok.Current;

	public readonly StreamTokenizer<T> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into a List, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input</returns>
	public readonly List<T[]> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun.");
		}

		var result = new List<T[]>();
		foreach (var token in this)
		{
			result.Add(token.ToArray());
		}

		return result;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into an Array, allocating a new array for each token.
	/// </summary>
	/// <returns>byte[][] or char[][], depending on the input</returns>
	public readonly T[][] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun.");
		}

		return this.ToList().ToArray();
	}
}

public static class StreamExtensions
{
	/// <summary>
	/// Resets an existing tokenizer with a new stream. You might choose this as an optimization, as it will re-use a buffer, avoiding allocations.
	/// </summary>
	/// <param name="stream">The new stream</param>
	public static void SetStream(ref this StreamTokenizer<byte> tokenizer, Stream stream)
	{
		tokenizer.tok.SetText([]);
		tokenizer.buffer.SetRead(stream.Read);
	}

	/// <summary>
	/// Resets an existing tokenizer with a new stream. You might choose this as an optimization, as it will re-use a buffer, avoiding allocations.
	/// </summary>
	/// <param name="stream">The new stream</param>
	public static void SetStream(ref this StreamTokenizer<char> tokenizer, TextReader stream)
	{
		tokenizer.tok.SetText([]);
		tokenizer.buffer.SetRead(stream.Read);
	}
}
