
namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Graphemes
{
	private readonly struct GraphemesIgnore : IIgnore
	{
		static Property IIgnore.Ignore { get; } = Extend;
	}

	private readonly struct GraphemesDict : IDict
	{
		static Dict IDict.Dict { get; } = Graphemes.Dict;
	}

	internal static readonly Split<byte> SplitUtf8Bytes = Splitter<byte, Utf8Decoder, GraphemesDict, GraphemesIgnore>.Split;
	internal static readonly Split<char> SplitChars = Splitter<char, Utf16Decoder, GraphemesDict, GraphemesIgnore>.Split;

	internal sealed class Splitter<TSpan, TDecoder, TDict, TIgnore>
		where TDecoder : struct, IDecoder<TSpan> // force non-reference so gets de-virtualized
		where TDict : struct, IDict // force non-reference so gets de-virtualized
		where TIgnore : struct, IIgnore // force non-reference so gets de-virtualized
	{
		private static SplitterBase.Context<TSpan, TDecoder, TDict, TIgnore> ctx { get; } = default;

		internal Splitter() : base()
		{ }

		public static int Split(ReadOnlySpan<TSpan> input, bool atEOF = true)
		{
			if (input.Length == 0)
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
				var eot = pos == input.Length;   // "end of text"


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

				/*
                    We've switched the evaluation order of GB1↓ and GB2↑. It's ok:
                    because we've checked for len(data) at the top of this function,
                    sot and eot are mutually exclusive, order doesn't matter.
                */

				var last = current;
				var lastWidth = w;

				// Rules are usually of the form Cat1 × Cat2; "current" refers to the first property
				// to the right of the × or ÷, from which we look back or forward

				var status = TDecoder.DecodeFirstRune(input[pos..], out Rune rune, out w);
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
						pos = input.Length;
						break;
					}
					// Rune extends past current data, request more
					return 0;
				}

				current = Dict.Lookup(rune.Value);

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
				if (current.Is(Extended_Pictographic) && last.Is(ZWJ) && ctx.Previous(Extended_Pictographic, input[..(pos - lastWidth)]))
				{
					pos += w;
					continue;
				}


				// https://unicode.org/reports/tr29/#GB12 and
				// https://unicode.org/reports/tr29/#GB13
				if ((current & last).Is(Regional_Indicator))
				{
					var i = pos;
					var count = 0;

					while (i > 0)
					{
						status = TDecoder.DecodeLastRune(input[..i], out Rune rune2, out int w2);
						if (status != OperationStatus.Done)
						{
							// Garbage in, garbage out
							break;
						}
						if (w2 == 0)
						{
							break;
						}

						i -= w2;

						var lookup = Dict.Lookup(rune2.Value);

						if (!lookup.Is(Regional_Indicator))
						{
							// It's GB13
							break;
						}

						count++;
					}

					// If i == 0, we fell through and hit sot (start of text), so GB12 applies
					// If i > 0, we hit a non-RI, so GB13 applies
					var odd = count % 2 == 1;
					if (odd)
					{
						pos += w;
						continue;
					}
				}

				// If we fall through all the above rules, it's a grapheme cluster break
				break;
			}

			return pos;
		}
	}
}
