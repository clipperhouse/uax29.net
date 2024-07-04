
namespace UAX29;

using System.Buffers;
using System.Diagnostics;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Graphemes
{
    internal static readonly Split<byte> SplitUtf8Bytes = new Splitter<byte>(Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8).Split;
    internal static readonly Split<char> SplitChars = new Splitter<char>(Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16).Split;

    internal sealed class Splitter<TSpan> : SplitterBase<TSpan>
    {
        internal Splitter(Decoder<TSpan> decodeFirstRune, Decoder<TSpan> decodeLastRune) :
            base(decodeFirstRune, decodeLastRune)
        { }

        const Property Ignore = Extend;

        internal override int Split(ReadOnlySpan<TSpan> input)
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
                var status = DecodeFirstRune(input[pos..], out Rune rune, out w);
                Debug.Assert(w > 0);
                if (status != OperationStatus.Done)
                {
                    // Garbage in, garbage out
                    pos += w;
                    return pos;
                }
                current = Dict.Lookup(rune.Value);
                pos += w;
            }

            // https://unicode.org/reports/tr29/#GB2
            while (pos < input.Length)
            {
                var last = current;
                var lastWidth = w;
                if (!last.Is(Ignore))
                {
                    lastLastExIgnore = lastExIgnore;
                    lastExIgnore = last;
                }

                // Rules are usually of the form Cat1 × Cat2; "current" refers to the first property
                // to the right of the × or ÷, from which we look back or forward

                var status = DecodeFirstRune(input[pos..], out Rune rune, out w);
                Debug.Assert(w > 0);
                if (status != OperationStatus.Done)
                {
                    // Garbage in, garbage out
                    pos += w;
                    break;
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

            return pos;
        }
    }
}
