namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

static partial class Graphemes
{
	static bool Matches(this Property lookup, Property properties)
	{
		return (lookup & properties) != 0;
	}

	const Property Ignore = Extend;

	internal static readonly SplitFunc SplitFunc = (Span<byte> data, bool atEOF) =>
	{
		if (data.Length == 0)
		{
			return 0;
		}

		// These vars are stateful across loop iterations
		var pos = 0;
		var w = 0;
		Property current = 0;

		while (true)
		{
			var sot = pos == 0;             // "start of text"
			var eot = pos == data.Length;   // "end of text"


			if (eot)
			{
				if (!atEOF)
				{
					// Token extends past current data, request more
					return 0; // TODO
				}

				// https://unicode.org/reports/tr29/#GB2
				break;
			}

			var last = current;
			var lastw = w;

			/*
				We've switched the evaluation order of GB1↓ and GB2↑. It's ok:
				because we've checked for len(data) at the top of this function,
				sot and eot are mutually exclusive, order doesn't matter.
			*/

			// Rules are usually of the form Cat1 × Cat2; "current" refers to the first property
			// to the right of the ×, from which we look back or forward

			current = dict.Lookup(data[pos..], out w, out _);   // TODO do something with the status

			if (w == 0)
			{
				if (atEOF)
				{
					// Just return the bytes, we can't do anything with them
					pos = data.Length;
					break;
				}

				// Rune extends past current data, request more
				return 0; // TODO
			}

			// https://unicode.org/reports/tr29/#GB1
			if (sot)
			{
				pos += w;
				continue;
			}

			// Optimization: no rule can possibly apply
			if ((current | last) == 0)  // i.e. both are zero
			{
				break;
			}

			// https://unicode.org/reports/tr29/#GB3
			if (current.Matches(LF) && last.Matches(CR))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#GB4
			// https://unicode.org/reports/tr29/#GB5
			if ((current | last).Matches(Control | CR | LF))
			{
				break;
			}

			// https://unicode.org/reports/tr29/#GB6
			if (current.Matches(L | V | LV | LVT) && last.Matches(L))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#GB7
			if (current.Matches(V | T) && last.Matches(LV | V))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#GB8
			if (current.Matches(T) && last.Matches(LVT | T))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#GB9
			if (current.Matches(Extend | ZWJ))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#GB9a
			if (current.Matches(SpacingMark))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#GB9b
			if (last.Matches(Prepend))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#GB11
			if (current.Matches(Extended_Pictographic) && last.Matches(ZWJ) && Previous(Extended_Pictographic, data[..(pos - lastw)]))
			{
				pos += w;
				continue;
			}


			// https://unicode.org/reports/tr29/#GB12 and
			// https://unicode.org/reports/tr29/#GB13
			if ((current & last).Matches(Regional_Indicator))
			{
				var i = pos;
				var count = 0;

				while (i > 0)
				{
					Rune.DecodeLastFromUtf8(data[..i], out Rune r, out int w2);
					if (w2 == 0)
					{
						break;
					}
					i -= w2;


					var lookup = dict.Lookup(data[i..], out int _, out OperationStatus _);


					if (!lookup.Matches(Regional_Indicator))
					{
						// It's GB13
						break;
					}

					count++;
				}

				// If i == 0, we fell through and hit sot (start of text), so GB12 applies
				// If i > 0, we hit a non-RI, so GB13 applies
				var oddRI = count % 2 == 1;
				if (oddRI)
				{
					pos += w;
					continue;
				}
			}

			// If we fall through all the above rules, it's a grapheme cluster break
			break;
		}

		return pos;
	};


	// previous works backward in the buffer until it hits a rune in properties,
	// ignoring runes with the Ignore property.
	static bool Previous(Property property, Span<byte> data)
	{
		// Start at the end of the buffer and move backwards
		var i = data.Length;
		while (i > 0)
		{
			var _ = Rune.DecodeLastFromUtf8(data[..i], out Rune _, out int w);  // TODO handle status

			i -= w;
			var lookup = dict.Lookup(data[i..], out int _, out OperationStatus _);
			// I think it's OK to elide width here; will fall through to break

			if (lookup.Matches(Ignore))
			{
				continue;
			}

			if (lookup.Matches(property))
			{
				return true;
			}

			// If we get this far, it's not there
			break;
		}

		return false;
	}

}
