
namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Graphemes
{
    internal static readonly Split Split = new Splitter<byte>(Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8).Split;

    internal class Splitter<TSpan> : SplitterBase<TSpan>
    {
        internal Splitter(Decoder<TSpan> decodeFirstRune, Decoder<TSpan> decodeLastRune) :
            base(Graphemes.Dict, Ignore, decodeFirstRune, decodeLastRune)
        { }

        new const Property Ignore = Extend;

        public override int Split(ReadOnlySpan<TSpan> input, bool atEOF = true)
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

                var status = DecodeFirstRune(input[pos..], out Rune rune, out w);
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
                if (current.Iss(LF) && last.Iss(CR))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#GB4
                // https://unicode.org/reports/tr29/#GB5
                if ((current | last).Iss(Control | CR | LF))
                {
                    break;
                }

                // https://unicode.org/reports/tr29/#GB6
                if (current.Iss(L | V | LV | LVT) && last.Iss(L))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#GB7
                if (current.Iss(V | T) && last.Iss(LV | V))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#GB8
                if (current.Iss(T) && last.Iss(LVT | T))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#GB9
                if (current.Iss(Extend | ZWJ))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#GB9a
                if (current.Iss(SpacingMark))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#GB9b
                if (last.Iss(Prepend))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#GB11
                if (current.Iss(Extended_Pictographic) && last.Iss(ZWJ) && Previous(Extended_Pictographic, input[..(pos - lastWidth)]))
                {
                    pos += w;
                    continue;
                }


                // https://unicode.org/reports/tr29/#GB12 and
                // https://unicode.org/reports/tr29/#GB13
                if ((current & last).Iss(Regional_Indicator))
                {
                    var i = pos;
                    var count = 0;

                    while (i > 0)
                    {
                        status = DecodeLastRune(input[..i], out Rune rune2, out int w2);
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

                        if (!lookup.Iss(Regional_Indicator))
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
