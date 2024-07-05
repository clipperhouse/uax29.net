namespace UAX29;

using System.Diagnostics;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Words
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

		const Property AHLetter = ALetter | Hebrew_Letter;
		const Property MidNumLetQ = MidNumLet | Single_Quote;
		const Property Ignore = Extend | Format | ZWJ;

		internal int Split(ReadOnlySpan<TSpan> input)
		{
			Debug.Assert(input.Length > 0);

			// These vars are stateful across loop iterations
			int pos = 0;
			int w;
			Property current = 0;
			Property lastExIgnore = 0;      // "last excluding ignored categories"
			Property lastLastExIgnore = 0;  // "the last one before that"
			int regionalIndicatorCount = 0;

			{
				// https://unicode.org/reports/tr29/#WB1
				// start of text always advances

				var _ = Decode.FirstRune(input[pos..], out Rune rune, out w);
				/*
                We are not doing anything about invalid runes. The decoders,
                if I am reading correctly, will return a width regardless,
                so we just pass over it. Garbage in, garbage out.
                */
				Debug.Assert(w > 0);

				pos += w;
				current = Dict.Lookup(rune.Value);
			}

			// https://unicode.org/reports/tr29/#WB2
			while (pos < input.Length)
			{
				var _ = Decode.FirstRune(input[pos..], out Rune rune, out w);
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
				if ((current | last) == 0)
				{ // i.e. both are zero
					break;
				}

				// https://unicode.org/reports/tr29/#WB3
				if (current.Is(LF) && last.Is(CR))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB3a
				// https://unicode.org/reports/tr29/#WB3b
				if ((last | current).Is(Newline | CR | LF))
				{
					break;
				}

				// https://unicode.org/reports/tr29/#WB3c
				if (current.Is(Extended_Pictographic) && last.Is(ZWJ))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB3d
				if ((current & last).Is(WSegSpace))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB4
				if (current.Is(Ignore))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB5
				if (current.Is(AHLetter) && lastExIgnore.Is(AHLetter))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB6
				if (current.Is(MidLetter | MidNumLetQ) && lastExIgnore.Is(AHLetter))
				{
					if (Subsequent(AHLetter, input[(pos + w)..]))
					{
						pos += w;
						continue;
					}
				}

				// https://unicode.org/reports/tr29/#WB7
				if (current.Is(AHLetter) && lastExIgnore.Is(MidLetter | MidNumLetQ) && lastLastExIgnore.Is(AHLetter))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB7a
				if (current.Is(Single_Quote) && lastExIgnore.Is(Hebrew_Letter))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB7b
				if (current.Is(Double_Quote) && lastExIgnore.Is(Hebrew_Letter))
				{
					if (Subsequent(Hebrew_Letter, input[(pos + w)..]))
					{
						pos += w;
						continue;
					}
				}

				// https://unicode.org/reports/tr29/#WB7c
				if (current.Is(Hebrew_Letter) && lastExIgnore.Is(Double_Quote) && lastLastExIgnore.Is(Hebrew_Letter))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB8
				// https://unicode.org/reports/tr29/#WB9
				// https://unicode.org/reports/tr29/#WB10
				if (current.Is(Numeric | AHLetter) && lastExIgnore.Is(Numeric | AHLetter))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB11
				if (current.Is(Numeric) && lastExIgnore.Is(MidNum | MidNumLetQ) && lastLastExIgnore.Is(Numeric))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB12
				if (current.Is(MidNum | MidNumLetQ) && lastExIgnore.Is(Numeric))
				{
					if (Subsequent(Numeric, input[(pos + w)..]))
					{
						pos += w;
						continue;
					}
				}

				// https://unicode.org/reports/tr29/#WB13
				if (current.Is(Katakana) && lastExIgnore.Is(Katakana | Ignore))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB13a
				if (current.Is(ExtendNumLet) && lastExIgnore.Is(AHLetter | Numeric | Katakana | ExtendNumLet))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB13b
				if (current.Is(AHLetter | Numeric | Katakana) && lastExIgnore.Is(ExtendNumLet))
				{
					pos += w;
					continue;
				}

				// https://unicode.org/reports/tr29/#WB15
				// https://unicode.org/reports/tr29/#WB16
				if (current.Is(Regional_Indicator) && lastExIgnore.Is(Regional_Indicator))
				{
					regionalIndicatorCount++;

					var odd = regionalIndicatorCount % 2 == 1;
					if (odd)
					{
						pos += w;
						continue;
					}
				}

				// https://unicode.org/reports/tr29/#WB999
				// If we fall through all the above rules, it's a word break
				break;
			}

			return pos;


			/// <summary>
			/// Seek forward until it hits a rune which matches property.
			/// </summary>
			/// <param name="property">Property to attempt to find</param>
			/// <param name="input">Data in which to seek</param>
			/// <returns>True if found, otherwise false</returns>
			bool Subsequent(Property property, ReadOnlySpan<TSpan> input)
			{
				var i = 0;
				while (i < input.Length)
				{
					var _ = Decode.FirstRune(input[i..], out Rune rune, out int w);
					/*
                    We are not doing anything about invalid runes. The decoders,
                    if I am reading correctly, will return a width regardless,
                    so we just pass over it. Garbage in, garbage out.
                    */
					Debug.Assert(w > 0);

					var lookup = Dict.Lookup(rune.Value);

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
	}
}
