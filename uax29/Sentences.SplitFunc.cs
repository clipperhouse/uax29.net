namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

static partial class Sentences
{
	static bool Is(this Property lookup, Property properties)
	{
		return (lookup & properties) != 0;
	}

	const Property SATerm = STerm | ATerm;
	const Property ParaSep = Sep | CR | LF;
	const Property Ignore = Extend | Format;

	internal static readonly SplitFunc SplitFunc = static (Span<byte> data, bool atEOF) =>
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

				// https://unicode.org/reports/tr29/#SB2
				break;
			}


			/*
				We've switched the evaluation order of SB1↓ and SB2↑. It's ok:
				because we've checked for len(data) at the top of this function,
				sot and eot are mutually exclusive, order doesn't matter.
			*/

			// Rules are usually of the form Cat1 × Cat2; "current" refers to the first property
			// to the right of the ×, from which we look back or forward

			var last = current;

			OperationStatus status;
			current = dict.Lookup(data[pos..], out w, out status);
			if (status != OperationStatus.Done)
			{
				// Garbage in, garbage out
				pos += w;
				break;
			}

			if (w == 0)
			{
				if (atEOF)
				{
					// Just return the bytes, we can't do anything with them
					pos = data.Length;
					break;
				}
				// Rune extends past current data, request more
				return 0;
			}

			// https://unicode.org/reports/tr29/#SB1
			if (sot)
			{
				pos += w;
				continue;
			}

			// Optimization: no rule can possibly apply
			if ((current | last) == 0)  // i.e. both are zero
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#SB3
			if (current.Is(LF) && last.Is(CR))
			{
				pos += w;
				continue;
			}

			// https://unicode.org/reports/tr29/#SB4
			if (last.Is(ParaSep))
			{
				break;
			}

			// https://unicode.org/reports/tr29/#SB5
			if (current.Is(Extend | Format))
			{
				pos += w;
				continue;
			}

			// SB5 applies to subsequent rules; there is an implied "ignoring Extend & Format"
			// https://unicode.org/reports/tr29/#Grapheme_Cluster_and_Format_Rules
			// The previous/subsequent methods are shorthand for "seek a property but skip over Extend & Format on the way"

			// Optimization: determine if SB6 can possibly apply
			var maybeSB6 = (current.Is(Numeric) && last.Is(ATerm | Ignore));

			// https://unicode.org/reports/tr29/#SB6
			if (maybeSB6)
			{
				if (Previous(ATerm, data[..pos]))
				{
					pos += w;
					continue;
				}
			}

			// Optimization: determine if SB7 can possibly apply
			var maybeSB7 = current.Is(Upper) && last.Is(ATerm | Ignore);

			// https://unicode.org/reports/tr29/#SB7
			if (maybeSB7)
			{
				var pi = PreviousIndex(ATerm, data[..pos]);
				if (pi >= 0 && Previous(Upper | Lower, data[..pi]))
				{
					pos += w;
					continue;
				}
			}

			// Optimization: determine if SB8 can possibly apply
			var maybeSB8 = last.Is(ATerm | Close | Sp | Ignore);

			// https://unicode.org/reports/tr29/#SB8
			if (maybeSB8)
			{
				var p = pos;

				// ( ¬(OLetter | Upper | Lower | ParaSep | SATerm) )*
				// Zero or more of not-the-above properties
				while (p < data.Length)
				{
					var lookup = dict.Lookup(data[p..], out int w2, out status);
					if (status != OperationStatus.Done)
					{
						// Garbage in, garbage out
						break;
					}

					if (w2 == 0)
					{
						if (atEOF)
						{
							// Just return the bytes, we can't do anything with them
							pos = data.Length;
							goto getout;    // i'd prefer a labeled break, I guess that's not thing? 
						}
						// Rune extends past current data, request more
						return 0; // TODO
					}

					if (lookup.Is(OLetter | Upper | Lower | ParaSep | SATerm))
					{
						break;
					}

					p += w2;
				}

				if (Subsequent(Lower, data[p..]))
				{
					var p2 = pos;

					// Zero or more Sp
					var sp = pos;
					while (true)
					{
						sp = PreviousIndex(Sp, data[..sp]);
						if (sp < 0)
						{
							break;
						}
						p2 = sp;
					}

					// Zero or more Close
					var close = p2;
					while (true)
					{
						close = PreviousIndex(Close, data[..close]);
						if (close < 0)
						{
							break;
						}
						p2 = close;
					}

					// Having looked back past Sp's, Close's, and intervening Extend|Format,
					// is there an ATerm?
					if (Previous(ATerm, data[..p2]))
					{
						pos += w;
						continue;
					}
				}
			}

			// Optimization: determine if SB8a can possibly apply
			var maybeSB8a = current.Is(SContinue | SATerm) && last.Is(SATerm | Close | Sp | Ignore);

			// https://unicode.org/reports/tr29/#SB8a
			if (maybeSB8a)
			{
				var p = pos;

				// Zero or more Sp
				var sp = p;
				while (true)
				{
					sp = PreviousIndex(Sp, data[..sp]);
					if (sp < 0)
					{
						break;
					}
					p = sp;
				}

				// Zero or more Close
				var close = p;
				while (true)
				{
					close = PreviousIndex(Close, data[..close]);
					if (close < 0)
					{
						break;
					}
					p = close;
				}

				// Having looked back past Sp, Close, and intervening Extend|Format,
				// is there an SATerm?
				if (Previous(SATerm, data[..p]))
				{
					pos += w;
					continue;
				}
			}

			// Optimization: determine if SB9 can possibly apply
			var maybeSB9 = current.Is(Close | Sp | ParaSep) && last.Is(SATerm | Close | Ignore);

			// https://unicode.org/reports/tr29/#SB9
			if (maybeSB9)
			{
				var p = pos;

				// Zero or more Close's
				var close = p;
				while (true)
				{
					close = PreviousIndex(Close, data[..close]);
					if (close < 0)
					{
						break;
					}
					p = close;
				}

				// Having looked back past Close's and intervening Extend|Format,
				// is there an SATerm?
				if (Previous(SATerm, data[..p]))
				{
					pos += w;
					continue;
				}
			}

			// Optimization: determine if SB10 can possibly apply
			var maybeSB10 = current.Is(Sp | ParaSep) && last.Is(SATerm | Close | Sp | Ignore);

			// https://unicode.org/reports/tr29/#SB10
			if (maybeSB10)
			{
				var p = pos;

				// Zero or more Sp's
				var sp = p;
				while (true)
				{
					sp = PreviousIndex(Sp, data[..sp]);
					if (sp < 0)
					{
						break;
					}
					p = sp;
				}

				// Zero or more Close's
				var close = p;
				while (true)
				{
					close = PreviousIndex(Close, data[..close]);
					if (close < 0)
					{
						break;
					}
					p = close;
				}

				// Having looked back past Sp's, Close's, and intervening Extend|Format,
				// is there an SATerm?
				if (Previous(SATerm, data[..p]))
				{
					pos += w;
					continue;
				}
			}

			// Optimization: determine if SB11 can possibly apply
			var maybeSB11 = last.Is(SATerm | Close | Sp | ParaSep | Ignore);

			// https://unicode.org/reports/tr29/#SB11
			if (maybeSB11)
			{
				var p = pos;

				// Zero or one ParaSep
				var ps = PreviousIndex(ParaSep, data[..p]);
				if (ps >= 0)
				{
					p = ps;
				}

				// Zero or more Sp's
				var sp = p;
				while (true)
				{
					sp = PreviousIndex(Sp, data[..sp]);
					if (sp < 0)
					{
						break;
					}
					p = sp;
				}

				// Zero or more Close's
				var close = p;
				while (true)
				{
					close = PreviousIndex(Close, data[..close]);
					if (close < 0)
					{
						break;
					}
					p = close;
				}

				// Having looked back past ParaSep, Sp's, Close's, and intervening Extend|Format,
				// is there an SATerm?
				if (Previous(SATerm, data[..p]))
				{
					break;
				}
			}

			// https://unicode.org/reports/tr29/#SB998
			pos += w;
		}

	getout:
		return pos;
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
			var lookup = dict.Lookup(data[i..], out int _, out var status);
			if (status != OperationStatus.Done)
			{
				// Garbage in, garbage out
				break;
			}

			// I think it's OK to elide width here; will fall through to break

			if (lookup.Is(Ignore))
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
			var lookup = dict.Lookup(data[i..], out int w, out var status);
			if (status != OperationStatus.Done)
			{
				// Garbage in, garbage out
				break;
			}

			if (w == 0)
			{
				break;
			}

			if (lookup.Is(Ignore))
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
