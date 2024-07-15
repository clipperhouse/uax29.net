namespace UAX29;

using System.Diagnostics;

/// A bitmap of Unicode categories
using Property = uint;

/// <summary>
/// StreamEnumerator is a small data structure for splitting strings from Streams or TextReaders. It implements GetEnumerator.
/// </summary>
public ref struct StreamEnumerator<T> where T : struct
{
	internal Buffer<T> buffer;
	readonly Split<T> split;
	readonly Options options;

	const int start = 0;   // with buffer, it's always 0
	int end = 0;

	/// <summary>
	/// The byte or char position of the current token in the stream.
	/// </summary>
	public readonly int Position => count;

	int count = 0;

	bool begun = false;

	/// <summary>
	/// StreamEnumerator is a small data structure for splitting strings.
	/// </summary>
	/// <param name="buffer">For backing storage, typically created from a Stream or TextReader.</param>
	/// <param name="split">A delegate that does the tokenizing. See Split<T> for details.</param>
	internal StreamEnumerator(Buffer<T> buffer, Split<T> split, Options options = Options.None)
	{
		this.buffer = buffer;
		this.split = split;
		this.options = options;
	}

	public bool MoveNext()
	{
		begun = true;

		while (end < buffer.Contents.Length)
		{
			count += end;
			buffer.Consume(this.Current.Length);    // previous token

			var advance = this.split(buffer.Contents, out Property seen);
			Debug.Assert(advance > 0);

			end = advance;

			// This option is only supported for words; prevent other uses at the static API level
			if (options.Includes(Options.OmitWhitespace) && seen.IsExclusively(Words.Whitespace))
			{
				continue;
			}

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

	public readonly StreamEnumerator<T> GetEnumerator()
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
	/// Resets an existing StreamEnumerator with a new stream. You might choose this as an optimization, as it will re-use a buffer, avoiding allocations.
	/// </summary>
	/// <param name="stream">The new stream</param>
	public static void SetStream(ref this StreamEnumerator<byte> tokenizer, Stream stream)
	{
		tokenizer.buffer.SetRead(stream.Read);
	}

	/// <summary>
	/// Resets an existing StreamEnumerator with a new stream. You might choose this as an optimization, as it will re-use a buffer, avoiding allocations.
	/// </summary>
	/// <param name="stream">The new stream</param>
	public static void SetStream(ref this StreamEnumerator<char> tokenizer, TextReader stream)
	{
		tokenizer.buffer.SetRead(stream.Read);
	}
}
