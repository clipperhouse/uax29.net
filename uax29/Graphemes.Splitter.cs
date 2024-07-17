namespace UAX29;

using System.Diagnostics;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Graphemes
{
	internal static readonly Split<byte> SplitBytes = new Splitter<byte>(Decoders.Utf8).Split;
	internal static readonly Split<char> SplitChars = new Splitter<char>(Decoders.Char).Split;

	sealed class Splitter<TSpan>
	{
		readonly Decoders<TSpan> Decode;
		internal Splitter(Decoders<TSpan> decoders)
		{
			this.Decode = decoders;
		}

		const Property Ignore = Extend;

		/// <summary>
		/// Splits the first grapheme in the input.
		/// </summary>
		/// <param name="input">The string in which to split graphemes.</param>
		/// <param name="seen">Ignore, only applicable to splitting words, not graphemes.</param>
		/// <returns>The number of bytes/chars that comprise the grapheme.</returns>
		internal int Split(ReadOnlySpan<TSpan> input, out bool _)   // this out param is only relevant in Words.Splitter
		{
			Debug.Assert(input.Length > 0);

			// These vars are stateful across loop iterations
			var pos = 0;
			int w;
			Property current = 0;
			Property lastExIgnore = 0;      // "last excluding ignored categories"
			Property lastLastExIgnore = 0;  // "last one before that"
			int regionalIndicatorCount = 0;

			{
				// https://unicode.org/reports/tr29/#GB1
				// start of text always advances

				Decode.FirstRune(input[pos..], out Rune rune, out w);
				/*
				We are not doing anything about invalid runes. The decoders,
				if I am reading correctly, will return a width regardless,
				so we just pass over it. Garbage in, garbage out.
				*/
				Debug.Assert(w > 0);

				pos += w;
				current = Dict.Lookup(rune.Value);
			}

			// https://unicode.org/reports/tr29/#GB2
			while (pos < input.Length)
			{
				Decode.FirstRune(input[pos..], out Rune rune, out w);
				/*
				We are not doing anything about invalid runes. The decoders,
				if I am reading correctly, will return a width regardless,
				so we just pass over it. Garbage in, garbage out.
				*/
				Debug.Assert(w > 0);

				var last = current;
				if (!last.Is(Ignore))
				{
					lastLastExIgnore = lastExIgnore;
					lastExIgnore = last;
				}

				current = Dict.Lookup(rune.Value);

				// Optimization: no rule can possibly apply
				if ((current | last) == 0)  // i.e. both are zero
				{
					break;
				}

				// https://unicode.org/reports/tr29/#GB3
				if (current.Is(LF) && last.Is(CR))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#GB4
				// https://unicode.org/reports/tr29/#GB5
				if ((current | last).Is(Control | CR | LF))
				{
					break;
				}

				// https://unicode.org/reports/tr29/#GB6
				if (current.Is(L | V | LV | LVT) && last.Is(L))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#GB7
				if (current.Is(V | T) && last.Is(LV | V))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#GB8
				if (current.Is(T) && last.Is(LVT | T))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#GB9
				if (current.Is(Extend | ZWJ))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#GB9a
				if (current.Is(SpacingMark))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#GB9b
				if (last.Is(Prepend))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#GB11
				if (current.Is(Extended_Pictographic) && last.Is(ZWJ) && lastLastExIgnore.Is(Extended_Pictographic))
				{
					pos += w;
					continue;
				}


				// https://unicode.org/reports/tr29/#GB12
				// https://unicode.org/reports/tr29/#GB13
				if ((current & last).Is(Regional_Indicator))
				{
					regionalIndicatorCount++;

					var odd = regionalIndicatorCount % 2 == 1;
					if (odd)
					{
						pos += w;
						continue;
					}
				}

				// If we fall through all the above rules, it's a grapheme cluster break
				break;
			}

			_ = false;  // see the out parameter at top
			return pos;
		}
	}
}
