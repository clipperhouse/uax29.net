namespace uax29;
using System.Text;
using System.Buffers;

/// A bitmap of Unicode categories
using Property = int;

public delegate (int advance, byte[] token) SplitFunc(Span<byte> data, bool atEOF);

public static partial class Words
{
	// is determines if lookup intersects one or more Properties
	static bool Matches(this Property lookup, Property properties)
	{
		return (lookup & properties) != 0;
	}

	const Property AHLetter = ALetter | Hebrew_Letter;

	const Property MidNumLetQ = MidNumLet | Single_Quote;

	const Property Ignore = Extend | Format | ZWJ;

	public static readonly SplitFunc SplitFunc = (Span<byte> data, bool atEOF) =>
	{
		if (data.Length == 0)
		{
			return (0, []);
		}

		// These vars are stateful across loop iterations
		int pos = 0;
		int w;
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
					return (0, []);
				}

				// https://unicode.org/reports/tr29/#WB2
				break;
			}

			var last = current;

			current = Lookup(data[pos..], out w, out _);   // TODO do something with the status

			if (w == 0)
			{
				if (atEOF)
				{
					// Just return the bytes, we can't do anything with them
					pos = data.Length;
					break;
				}
				// Rune extends past current data, request more
				return (0, []);
			}

			// https://unicode.org/reports/tr29/#WB1
			if (sot)
			{
				pos += w;
				continue;
			}

			// Optimization: no rule can possibly apply
			if ((current | last) == 0)
			{ // i.e. both are zero
				break;
			}

			// https://unicode.org/reports/tr29/#WB3
			if (current.Matches(LF) && last.Matches(CR))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#WB3a
			// https://unicode.org/reports/tr29/#WB3b
			if ((last | current).Matches(Newline | CR | LF))
			{
				break;
			}

			// https://unicode.org/reports/tr29/#WB3c
			if (current.Matches(Extended_Pictographic) && last.Matches(ZWJ))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#WB3d
			if ((current & last).Matches(WSegSpace))
			{
				pos += w;
				continue;
			}


			// https://unicode.org/reports/tr29/#WB4
			if (current.Matches(Extend | Format | ZWJ))
			{
				pos += w;
				continue;
			}


			// WB4 applies to subsequent rules; there is an implied "ignoring Extend & Format & ZWJ"
			// https://unicode.org/reports/tr29/#Grapheme_Cluster_and_Format_Rules
			// The previous/subsequent methods are shorthand for "seek a property but skip over Extend|Format|ZWJ on the way"

			// https://unicode.org/reports/tr29/#WB5
			if (current.Matches(AHLetter) && last.Matches(AHLetter | Ignore))
			{
				// Optimization: maybe a run without ignored characters
				if (last.Matches(AHLetter))
				{
					pos += w;
					while (pos < data.Length)
					{
						var lookup = Lookup(data[pos..], out int w2, out _);
						if (!lookup.Matches(AHLetter))
						{
							break;
						}

						if (w2 == 0)
						{
							break;
						}

						// Update stateful vars
						current = lookup;
						w = w2;

						pos += w;
					}
					continue;
				}

				// Otherwise, do proper look back per WB4
				if (Previous(AHLetter, data[..pos]))
				{
					pos += w;
					continue;
				}
			}

			// Optimization: determine if WB6 can possibly apply
			var maybeWB6 = current.Matches(MidLetter | MidNumLetQ) && last.Matches(AHLetter | Ignore);

			// https://unicode.org/reports/tr29/#WB6
			if (maybeWB6)
			{
				if (Subsequent(AHLetter, data[(pos + w)..]) && Previous(AHLetter, data[..pos]))
				{
					pos += w;
					continue;
				}
			}

			// Optimization: determine if WB7 can possibly apply
			var maybeWB7 = current.Matches(AHLetter) && last.Matches(MidLetter | MidNumLetQ | Ignore);

			// https://unicode.org/reports/tr29/#WB7
			if (maybeWB7)
			{
				var i = PreviousIndex(MidLetter | MidNumLetQ, data[..pos]);
				if (i > 0 && Previous(AHLetter, data[..i]))
				{
					pos += w;
					continue;
				}
			}

			// https://unicode.org/reports/tr29/#WB999
			// If we fall through all the above rules, it's a word break
			break;
		}

		return (pos, data[..pos].ToArray());
	};


	// previousIndex works backward until it hits a rune in properties,
	// ignoring runes with the _Ignore property (per WB4), and returns
	// the index in data. It returns -1 if such a rune is not found.
	static int PreviousIndex(Property property, Span<byte> data)
	{
		// Start at the end of the buffer and move backwards
		var i = data.Length;
		while (i > 0)
		{
			var _ = Rune.DecodeLastFromUtf8(data[..i], out Rune _, out int w);  // TODO handle status

			i -= w;
			var lookup = Lookup(data[i..], out int _, out OperationStatus _);
			// I think it's OK to elide width here; will fall through to break

			if (lookup.Matches(Ignore))
			{
				continue;
			}

			if (lookup.Matches(property))
			{
				return i;
			}

			// If we get this far, it's not there
			break;
		}

		return -1;
	}

	// previous works backward in the buffer until it hits a rune in properties,
	// ignoring runes with the _Ignore property per WB4
	static bool Previous(Property property, Span<byte> data)
	{
		return PreviousIndex(property, data) != -1;
	}

	// subsequent looks ahead in the buffer until it hits a rune in properties,
	// ignoring runes with the _Ignore property per WB4
	static bool Subsequent(Property property, Span<byte> data)
	{
		var i = 0;
		while (i < data.Length)
		{
			var lookup = Lookup(data[i..], out int w, out OperationStatus _);

			if (w == 0)
			{
				break;
			}

			if (lookup.Matches(Ignore))
			{
				i += w;
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
