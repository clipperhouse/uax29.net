using Buffer;

namespace uax29;

using Buffer;

/// <summary>
/// StreamTokenizer is a small data structure for splitting strings from Streams or TextReaders. It implements GetEnumerator.
/// </summary>
public ref struct StreamTokenizer2
{
	readonly Split<byte> split;

	internal Stream stream;
	internal int consumed;
	internal Span<byte> buf = new byte[1024];

	internal int end;

	bool begun = false;

	/// <summary>
	/// StreamTokenizer is a small data structure for splitting strings.
	/// </summary>
	/// <param name="buffer">For backing storage, typically created from a Stream or TextReader.</param>
	/// <param name="split">A delegate that does the tokenizing. See Split<T> for details.</param>
	internal StreamTokenizer2(Stream stream, Split<byte> split)
	{
		this.stream = stream;
		this.split = split;
	}

	public bool MoveNext()
	{
		begun = true;

		stream.Seek(consumed, SeekOrigin.Begin);

		int read = stream.Read(buf);
		// Interpret as EOF
		if (read == 0)
		{
			return false;
		}

		var advance = this.split(buf[..read], read == 0);
		// Interpret as EOF
		if (advance == 0)
		{
			return false;
		}

		consumed += advance;
		end = advance;

		return true;
	}

	public ReadOnlySpan<byte> Current
	{
		get
		{
			return buf[..end];
		}
	}

	public StreamTokenizer2 GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Iterates over all tokens and collects them into a List, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input.</returns>
	public List<byte[]> ToList()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToList must not be called after iteration has begun.");
		}

		var result = new List<byte[]>();
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
	public byte[][] ToArray()
	{
		if (begun)
		{
			throw new InvalidOperationException("ToArray must not be called after iteration has begun.");
		}

		return this.ToList().ToArray();
	}
}
