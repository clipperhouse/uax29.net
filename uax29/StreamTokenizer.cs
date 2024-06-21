namespace UAX29;

/// <summary>
/// StreamTokenizer is a small data structure for splitting strings from Streams or TextReaders. It implements GetEnumerator.
/// </summary>
public ref struct StreamTokenizer<T> where T : struct
{
	internal Buffer<T> buffer;
	readonly Split<T> split;

	internal const int start = 0;   // with buffer, it's always 0
	internal int end = 0;

	bool begun = false;

	/// <summary>
	/// StreamTokenizer is a small data structure for splitting strings.
	/// </summary>
	/// <param name="buffer">For backing storage, typically created from a Stream or TextReader.</param>
	/// <param name="split">A delegate that does the tokenizing. See Split<T> for details.</param>
	internal StreamTokenizer(Buffer<T> buffer, Split<T> split)
	{
		this.buffer = buffer;
		this.split = split;
	}

	public bool MoveNext()
	{
		begun = true;

		if (end < buffer.Contents.Length)
		{
			buffer.Consume(this.Current.Length);    // previous token

			var advance = this.split(buffer.Contents, buffer.EOF);
			// Interpret as EOF
			if (advance == 0)
			{
				return false;
			}

			end = advance;

			return true;
		}

		return false;
	}

	public ReadOnlySpan<T> Current
	{
		get
		{
			return buffer.Contents[start..end];
		}
	}

	public readonly StreamTokenizer<T> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into a List, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input.</returns>
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
	/// <returns>byte[][] or char[][], depending on the input.</returns>
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
		tokenizer.buffer.SetRead(stream.Read);
	}

	/// <summary>
	/// Resets an existing tokenizer with a new stream. You might choose this as an optimization, as it will re-use a buffer, avoiding allocations.
	/// </summary>
	/// <param name="stream">The new stream</param>
	public static void SetStream(ref this StreamTokenizer<char> tokenizer, TextReader stream)
	{
		tokenizer.buffer.SetRead(stream.Read);
	}
}
