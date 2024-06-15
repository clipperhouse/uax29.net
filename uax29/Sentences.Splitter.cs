namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

/// Make SplitterBase helpers available
using static SplitterBase;

internal static partial class Sentences
{
	private readonly struct SentencesIgnore : IIgnore
	{
		static Property IIgnore.Ignore { get; } = Extend | Format;
	}

	private readonly struct SentencesDict : IDict
	{
		static Dict IDict.Dict { get; } = Sentences.Dict;
	}

	internal static readonly Split<byte> SplitUtf8Bytes = Splitter<byte, Utf8Decoder, SentencesDict, SentencesIgnore>.Split;
	internal static readonly Split<char> SplitChars = Splitter<char, Utf16Decoder, SentencesDict, SentencesIgnore>.Split;

	internal sealed class Splitter<TSpan, TDecoder, TDict, TIgnore>
		where TDecoder : struct, IDecoder<TSpan> // force non-reference so gets de-virtualized
		where TDict : struct, IDict // force non-reference so gets de-virtualized
		where TIgnore : struct, IIgnore // force non-reference so gets de-virtualized
	{
		internal Splitter() : base()
		{ }

		const Property SATerm = STerm | ATerm;
		const Property ParaSep = Sep | CR | LF;

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
				var maybeSB6 = (current.Is(Numeric) && last.Is(ATerm | TIgnore.Ignore));

				// https://unicode.org/reports/tr29/#SB6
				if (maybeSB6)
				{
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(ATerm, input[..pos]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if SB7 can possibly apply
				var maybeSB7 = current.Is(Upper) && last.Is(ATerm | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#SB7
				if (maybeSB7)
				{
					var pi = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(ATerm, input[..pos]);
					if (pi >= 0 && Previous<TSpan, TDecoder, TDict, TIgnore>(Upper | Lower, input[..pi]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if SB8 can possibly apply
				var maybeSB8 = last.Is(ATerm | Close | Sp | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#SB8
				if (maybeSB8)
				{
					var p = pos;

					// ( ¬(OLetter | Upper | Lower | ParaSep | SATerm) )*
					// Zero or more of not-the-above properties
					while (p < input.Length)
					{
						status = TDecoder.DecodeFirstRune(input[p..], out Rune rune2, out int w2);
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
								pos = input.Length;
								goto getout;    // i'd prefer a labeled break, I guess that's not thing? 
							}
							// Rune extends past current data, request more
							return 0; // TODO
						}

						var lookup = Dict.Lookup(rune2.Value);

						if (lookup.Is(OLetter | Upper | Lower | ParaSep | SATerm))
						{
							break;
						}

						p += w2;
					}

					if (Subsequent<TSpan, TDecoder, TDict, TIgnore>(Lower, input[p..]))
					{
						var p2 = pos;

						// Zero or more Sp
						var sp = pos;
						while (true)
						{
							sp = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Sp, input[..sp]);
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
							close = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Close, input[..close]);
							if (close < 0)
							{
								break;
							}
							p2 = close;
						}

						// Having looked back past Sp's, Close's, and intervening Extend|Format,
						// is there an ATerm?
						if (Previous<TSpan, TDecoder, TDict, TIgnore>(ATerm, input[..p2]))
						{
							pos += w;
							continue;
						}
					}
				}

				// Optimization: determine if SB8a can possibly apply
				var maybeSB8a = current.Is(SContinue | SATerm) && last.Is(SATerm | Close | Sp | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#SB8a
				if (maybeSB8a)
				{
					var p = pos;

					// Zero or more Sp
					var sp = p;
					while (true)
					{
						sp = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Sp, input[..sp]);
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
						close = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Close, input[..close]);
						if (close < 0)
						{
							break;
						}
						p = close;
					}

					// Having looked back past Sp, Close, and intervening Extend|Format,
					// is there an SATerm?
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(SATerm, input[..p]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if SB9 can possibly apply
				var maybeSB9 = current.Is(Close | Sp | ParaSep) && last.Is(SATerm | Close | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#SB9
				if (maybeSB9)
				{
					var p = pos;

					// Zero or more Close's
					var close = p;
					while (true)
					{
						close = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Close, input[..close]);
						if (close < 0)
						{
							break;
						}
						p = close;
					}

					// Having looked back past Close's and intervening Extend|Format,
					// is there an SATerm?
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(SATerm, input[..p]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if SB10 can possibly apply
				var maybeSB10 = current.Is(Sp | ParaSep) && last.Is(SATerm | Close | Sp | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#SB10
				if (maybeSB10)
				{
					var p = pos;

					// Zero or more Sp's
					var sp = p;
					while (true)
					{
						sp = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Sp, input[..sp]);
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
						close = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Close, input[..close]);
						if (close < 0)
						{
							break;
						}
						p = close;
					}

					// Having looked back past Sp's, Close's, and intervening Extend|Format,
					// is there an SATerm?
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(SATerm, input[..p]))
					{
						pos += w;
						continue;
					}
				}

				// Optimization: determine if SB11 can possibly apply
				var maybeSB11 = last.Is(SATerm | Close | Sp | ParaSep | TIgnore.Ignore);

				// https://unicode.org/reports/tr29/#SB11
				if (maybeSB11)
				{
					var p = pos;

					// Zero or one ParaSep
					var ps = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(ParaSep, input[..p]);
					if (ps >= 0)
					{
						p = ps;
					}

					// Zero or more Sp's
					var sp = p;
					while (true)
					{
						sp = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Sp, input[..sp]);
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
						close = PreviousIndex<TSpan, TDecoder, TDict, TIgnore>(Close, input[..close]);
						if (close < 0)
						{
							break;
						}
						p = close;
					}

					// Having looked back past ParaSep, Sp's, Close's, and intervening Extend|Format,
					// is there an SATerm?
					if (Previous<TSpan, TDecoder, TDict, TIgnore>(SATerm, input[..p]))
					{
						break;
					}
				}

				// https://unicode.org/reports/tr29/#SB998
				pos += w;
			}

		getout:
			return pos;
		}
	}
}
