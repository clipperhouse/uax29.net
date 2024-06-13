namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static class SplitterBase
{
	/// <summary>
	/// Seek backward until it hits a rune which matches property.
	/// </summary>
	/// <param name="property">Property to attempt to find</param>
	/// <param name="input">Data in which to seek</param>
	/// <returns>The index if found, or -1 if not</returns>
	internal static int PreviousIndex<TSpan, TDecoder>(Property property, ReadOnlySpan<TSpan> input)
		where TDecoder : struct, IDecoder<TSpan> // force non-reference so gets de-virtualized
	{
		// Start at the end of the buffer and move backwards
		var i = input.Length;
		while (i > 0)
		{
			var status = TDecoder.DecodeLastRune(input[..i], out Rune rune, out int w);
			if (status != OperationStatus.Done)
			{
				// Garbage in, garbage out
				break;
			}
			if (w == 0)
			{
				break;
			}

			i -= w;
			var lookup = TDecoder.Dict.Lookup(rune.Value);

			if (lookup.Is(TDecoder.Ignore))
			{
				continue;
			}

			if (lookup.Is(property))
			{
				return i;
			}

			// If we get this far, it's not there
			break;
		}

		return -1;
	}

	/// <summary>
	/// Seek backward until it hits a rune which matches property.
	/// </summary>
	/// <param name="property">Property to attempt to find</param>
	/// <param name="input">Data in which to seek</param>
	/// <returns>True if found, otherwise false</returns>
	internal static bool Previous<TSpan, TDecoder>(Property property, ReadOnlySpan<TSpan> input)
		where TDecoder : struct, IDecoder<TSpan> // force non-reference so gets de-virtualized
	{
		return PreviousIndex<TSpan, TDecoder>(property, input) != -1;
	}

	/// <summary>
	/// Seek forward until it hits a rune which matches property.
	/// </summary>
	/// <param name="property">Property to attempt to find</param>
	/// <param name="input">Data in which to seek</param>
	/// <returns>True if found, otherwise false</returns>
	internal static bool Subsequent<TSpan, TDecoder>(Property property, ReadOnlySpan<TSpan> input)
		where TDecoder : struct, IDecoder<TSpan> // force non-reference so gets de-virtualized
	{
		var i = 0;
		while (i < input.Length)
		{
			var status = TDecoder.DecodeFirstRune(input[i..], out Rune rune, out int w);
			if (status != OperationStatus.Done)
			{
				// Garbage in, garbage out
				break;
			}
			if (w == 0)
			{
				break;
			}

			var lookup = TDecoder.Dict.Lookup(rune.Value);

			if (lookup.Is(TDecoder.Ignore))
			{
				i += w;
				continue;
			}

			if (lookup.Is(property))
			{
				return true;
			}

			// If we get this far, it's not there
			break;
		}

		return false;
	}
}

internal static class Extensions
{
	/// <summary>
	/// Determines whether two properties (bitstrings) match, i.e. intersect, i.e. share at least one bit.
	/// </summary>
	/// <param name="lookup">One property to test against...</param>
	/// <param name="properties">...the other</param>
	/// <returns>True if the two properties share a bit, i.e. Unicode category.</returns>
	internal static bool Is(this Property lookup, Property properties)
	{
		return (lookup & properties) != 0;
	}
}
