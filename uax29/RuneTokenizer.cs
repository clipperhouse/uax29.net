using System.Buffers;
using System.Text;

namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

internal static class RuneTokenizer
{
	internal static RuneTokenizer<char> Create(ReadOnlySpan<char> input)
	{
		return new RuneTokenizer<char>(input, Decoders.Char);
	}

	internal static RuneTokenizer<byte> Create(ReadOnlySpan<byte> input)
	{
		return new RuneTokenizer<byte>(input, Decoders.Utf8);
	}
}

internal static class Decoders
{
	internal readonly static Decoders<char> Char = new(Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);
	internal readonly static Decoders<byte> Utf8 = new(Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8);
}

internal class Decoders<T>
{
	internal Decoder<T> FirstRune;
	internal Decoder<T> LastRune;

	internal Decoders(Decoder<T> firstRune, Decoder<T> lastRune)
	{
		this.FirstRune = firstRune;
		this.LastRune = lastRune;
	}
}


/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
/// <typeparam name="T">byte or char, indicating the type of the input, and by implication, the output.</typeparam>
public ref struct RuneTokenizer<T> where T : struct
{
	internal readonly ReadOnlySpan<T> input;

	readonly Decoders<T> Decode;

	int start = 0;
	int end = 0;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	/// <param name="tokenType">Choose to split words, graphemes or sentences. Default is words.</param>
	internal RuneTokenizer(ReadOnlySpan<T> input, Decoders<T> decoders)
	{
		this.input = input;
		this.Decode = decoders;
	}

	/// <summary>
	/// Move to the next rune. Use <see cref="Current"/> to retrieve the rune.
	/// </summary>
	/// <returns>Whether there are any more runes. False typically means EOF.</returns>
	public bool MoveNext()
	{
		if (!Any())
		{
			start = end;
			return false;
		}

		var status = Decode.FirstRune(input[end..], out Rune rune, out int consumed);
		if (status != OperationStatus.Done)
		{
			// Garbage in, garbage out
			throw new InvalidOperationException("Rune could not be decoded");
		}

		this.rune = rune.Value;
		this.width = consumed;

		start = end;
		end += consumed;
		return true;
	}

	internal readonly bool Any()
	{
		return end < input.Length;
	}

	/// <summary>
	/// Move to the previous rune. Use <see cref="Current"/> to retrieve the rune.
	/// </summary>
	/// <returns>Whether there are any more runes. False typically means the beginning of the string.</returns>
	public bool MovePrevious()
	{
		if (start == 0)
		{
			end = start;
			return false;
		}

		var status = Decode.LastRune(input[..start], out Rune rune, out int consumed);
		if (status != OperationStatus.Done)
		{
			// Garbage in, garbage out
			throw new InvalidOperationException("Rune could not be decoded");
		}

		this.rune = rune.Value;
		this.width = consumed;

		end = start;
		start -= consumed;

		return true;
	}

	int rune;

	/// <summary>
	/// The current rune
	/// </summary>
	public readonly int Current
	{
		get
		{
			return this.rune;
		}
	}

	int width;

	public readonly int CurrentWidth
	{
		get
		{
			return this.width;
		}
	}

	internal void Consume(int consumed)
	{
		start += consumed;
		end = start;
		this.rune = 0;
		this.width = 0;
	}

	// public readonly RuneTokenizer<T> GetEnumerator()
	// {
	// 	return this;
	// }

	/// <summary>
	/// Resets the tokenizer back to the first rune.
	/// </summary>
	public void Reset()
	{
		this.end = 0;
	}

	/// <summary>
	/// Iterates over all runes and collects them into a list, allocating a new array for each token.
	/// </summary>
	/// <returns>List<byte[]> or List<char[]>, depending on the input</returns>
	public List<int> ToList()
	{
		var result = new List<int>();
		while (MoveNext())
		{
			result.Add(Current);
		}

		this.Reset();
		return result;
	}

	/// <summary>
	/// Iterates over all runes and collects them into an array, allocating a new array for each token.
	/// </summary>
	/// <returns>byte[][] or char[][], depending on the input</returns>
	public int[] ToArray()
	{
		return this.ToList().ToArray();
	}
}

internal static class RuneExtensions
{
	/// <summary>
	/// Seek backward until it hits a rune which matches the seek parameter.
	/// </summary>
	/// <param name="seek">Property to attempt to find</param>
	/// <param name="runes">Data in which to seek</param>
	/// <param name="intermediate">Optional, Property which needs to be found before finding seek</param>
	/// <returns>True if found, otherwise false</returns>
	internal static bool Subsequent<TSpan>(this RuneTokenizer<TSpan> runes, Property seek, Dict Dict, Property Ignore) where TSpan : struct
	{
		// N.B: runes is passed by value, i.e. is a copy, so navigating here does not affect the caller
		while (runes.MoveNext())
		{
			var lookup = Dict.Lookup(runes.Current);

			if (lookup.Is(Ignore))
			{
				continue;
			}

			if (lookup.Is(seek))
			{
				return true;
			}

			break;
		}

		return false;
	}

}