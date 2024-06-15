namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

/// Make SplitterBase helpers available
using static SplitterBase;

internal static partial class Words
{
	private readonly struct WordsIgnore : IIgnore
	{
		static Property IIgnore.Ignore { get; } = Extend | Format | ZWJ;
	}

	private readonly struct WordsDict : IDict
	{
		static Dict IDict.Dict { get; } = Words.Dict;
	}

	internal static readonly Split<byte> SplitUtf8Bytes = Splitter<byte, Utf8Decoder, WordsDict, WordsIgnore>.Split;
	internal static readonly Split<char> SplitChars = Splitter<char, Utf16Decoder, WordsDict, WordsIgnore>.Split;

	internal sealed class Splitter<TSpan, TDecoder, TDict, TIgnore>
		where TDecoder : struct, IDecoder<TSpan> // force non-reference so gets de-virtualized
		where TDict : struct, IDict // force non-reference so gets de-virtualized
		where TIgnore : struct, IIgnore // force non-reference so gets de-virtualized
	{
		internal Splitter() : base()
		{ }

		const Property AHLetter = ALetter | Hebrew_Letter;
		const Property MidNumLetQ = MidNumLet | Single_Quote;

		public static int Split(ReadOnlySpan<TSpan> input, bool atEOF = true)
		{
			if (input.Length == 0)
			{
				return 0;
			}

			// These vars are stateful across loop iterations
			int pos = 0;
			int w;
			Property current = 0;

			while (true)
			{
				var sot = pos == 0;             // "start of text"
				var eot = pos == input.Length;   // "end of text"

				if (eot)
				{
					if (!atEOF)
					{
						// TODO Token extends past current data, request more
						return 0;
					}

					// https://unicode.org/reports/tr29/#WB2
					break;
				}

				var last = current;

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
				if (current.Is(Extend | Format | ZWJ))
				{
					pos += w;
					continue;
				}

				// WB4 applies to subsequent rules; there is an implied "ignoring Extend & Format & ZWJ"
				// https://unicode.org/reports/tr29/#Grapheme_Cluster_and_Format_Rules
				// The previous/subsequent methods are shorthand for "seek a property but skip over Extend|Format|ZWJ on the way"

				// https://unicode.org/reports/tr29/#WB5
				if (current.Is(AHLetter) && last.Is(AHLetter | TIgnore.Ignore))
				{
					// Optimization: maybe a run without ignored characters
					if (last.Is(AHLetter))
					{
						pos += w;
						while (pos < input.Length)
						{
							status = TDecoder.DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
							if (status != OperationStatus.Done)
							{
								// Garbage in, garbage out
								break;
							}
							if (w2 == 0)
							{
								break;
							}

							var lookup = Dict.Lookup(rune2.Value);
							if (!lookup.Is(AHLetter))
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
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(AHLetter, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB6 can possibly apply
				var maybeWB6 = current.Is(MidLetter | MidNumLetQ) && last.Is(AHLetter | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB6
				if (maybeWB6)
				{
					if (Subsequent<TSpan, TDecoder, TDict, TIgnore>(AHLetter, input[(pos + w)..]) && Previous<TSpan, TDecoder, TDict, TIgnore>(AHLetter, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB7 can possibly apply
				var maybeWB7 = current.Is(AHLetter) && last.Is(MidLetter | MidNumLetQ | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB7
				if (maybeWB7)
				{
					var i = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(MidLetter | MidNumLetQ, input[..pos]);
					if (i > 0 && Previous<TSpan, TDecoder, TDict, TIgnore>(AHLetter, input[..i]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB7a can possibly apply
				var maybeWB7a = current.Is(Single_Quote) && last.Is(Hebrew_Letter | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB7a
				if (maybeWB7a)
				{
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(Hebrew_Letter, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB7b can possibly apply
				var maybeWB7b = current.Is(Double_Quote) && last.Is(Hebrew_Letter | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB7b
				if (maybeWB7b)
				{
					if (Subsequent<TSpan, TDecoder, TDict, TIgnore>(Hebrew_Letter, input[(pos + w)..]) && Previous<TSpan, TDecoder, TDict, TIgnore>(Hebrew_Letter, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB7c can possibly apply
				var maybeWB7c = current.Is(Hebrew_Letter) && last.Is(Double_Quote | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB7c
				if (maybeWB7c)
				{
					var i = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Double_Quote, input[..pos]);
					if (i > 0 && Previous<TSpan, TDecoder, TDict, TIgnore>(Hebrew_Letter, input[..i]))
					{
						pos += w;
						continue;
					}
				}

				// https://unicode.org/reports/tr29/#WB8
				// https://unicode.org/reports/tr29/#WB9
				// https://unicode.org/reports/tr29/#WB10
				if (current.Is(Numeric | AHLetter) && last.Is(Numeric | AHLetter | TIgnore.Ignore))
				{
					// Note: this logic de facto expresses WB5 as well, but harmless since WB5
					// was already tested above

					// Optimization: maybe a run without ignored characters
					if (last.Is(Numeric | AHLetter))
					{
						pos += w;
						while (pos < input.Length)
						{
							status = TDecoder.DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
							if (status != OperationStatus.Done)
							{
								// Garbage in, garbage out
								break;
							}
							if (w2 == 0)
							{
								break;
							}

							var lookup = Dict.Lookup(rune2.Value);

							if (!lookup.Is(Numeric | AHLetter))
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

					// Otherwise, do proper lookback per WB4
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(Numeric | AHLetter, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB11 can possibly apply
				var maybeWB11 = current.Is(Numeric) && last.Is(MidNum | MidNumLetQ | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB11
				if (maybeWB11)
				{
					var i = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(MidNum | MidNumLetQ, input[..pos]);
					if (i > 0 && Previous<TSpan, TDecoder, TDict, TIgnore>(Numeric, input[..i]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB12 can possibly apply
				var maybeWB12 = current.Is(MidNum | MidNumLetQ) && last.Is(Numeric | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB12
				if (maybeWB12)
				{
					if (Subsequent<TSpan, TDecoder, TDict, TIgnore>(Numeric, input[(pos + w)..]) && Previous<TSpan, TDecoder, TDict, TIgnore>(Numeric, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// https://unicode.org/reports/tr29/#WB13
				if (current.Is(Katakana) && last.Is(Katakana | TIgnore.Ignore))
				{
					// Optimization: maybe a run without ignored characters
					if (last.Is(Katakana))
					{
						pos += w;
						while (pos < input.Length)
						{
							status = TDecoder.DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
							if (status != OperationStatus.Done)
							{
								// Garbage in, garbage out
								break;
							}
							if (w2 == 0)
							{
								break;
							}

							var lookup = Dict.Lookup(rune2.Value);
							if (!lookup.Is(Katakana))
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

					// Otherwise, do proper lookback per WB4
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(Katakana, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB13a can possibly apply
				var maybeWB13a = current.Is(ExtendNumLet) && last.Is(AHLetter | Numeric | Katakana | ExtendNumLet | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB13a
				if (maybeWB13a)
				{
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(AHLetter | Numeric | Katakana | ExtendNumLet, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB13b can possibly apply
				var maybeWB13b = current.Is(AHLetter | Numeric | Katakana) && last.Is(ExtendNumLet | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB13b
				if (maybeWB13b)
				{
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(ExtendNumLet, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if WB15 or WB16 can possibly apply
				var maybeWB1516 = current.Is(Regional_Indicator) && last.Is(Regional_Indicator | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#WB15 and
				// https://unicode.org/reports/tr29/#WB16
				if (maybeWB1516)
				{
					// WB15: Odd number of RI before hitting start of text
					// WB16: Odd number of RI before hitting [^RI], aka "not RI"

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
						if (status != OperationStatus.Done)
						{
							// Garbage in, garbage out
							break;
						}

						if (lookup.Is(TIgnore.Ignore))
						{
							continue;
						}

						if (!lookup.Is(Regional_Indicator))
						{
							// It's WB16
							break;
						}

						count++;
					}

					// If i == 0, we fell through and hit sot (start of text), so WB15 applies
					// If i > 0, we hit a non-RI, so WB16 applies

					var odd = count % 2 == 1;
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
		}
	}
}
