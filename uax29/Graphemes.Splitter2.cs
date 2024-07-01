namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Graphemes
{
    internal static readonly Split2<byte> Split2Utf8Bytes = new Splitter2<byte>().Split;
    internal static readonly Split2<char> Split2Chars = new Splitter2<char>().Split;

    internal sealed class Splitter2<TSpan> : SplitterBase2<TSpan> where TSpan : struct
    {
        internal Splitter2() :
            base(Graphemes.Dict, Ignore)
        { }

        new const Property Ignore = Extend;

        internal override int Split(RuneTokenizer<TSpan> runes, bool atEOF = true)
        {
            // These vars are stateful across loop iterations
            int pos = 0;
            Property current = 0;
            Property lastExIgnore = 0;      // "last excluding ignored categories"
            Property lastLastExIgnore = 0;  // "last one before that"
            int regionalIndicatorCount = 0;

            // https://unicode.org/reports/tr29/#GB1
            if (runes.MoveNext())
            {
                // start of text always advances
                var rune = runes.Current;
                current = Dict.Lookup(rune);
                pos += runes.CurrentWidth;
            }

            while (runes.MoveNext())
            {
                var last = current;
                if (!last.Is(Ignore))
                {
                    lastLastExIgnore = lastExIgnore;
                    lastExIgnore = last;
                }

                var rune = runes.Current;
                var w = runes.CurrentWidth;
                current = Dict.Lookup(rune);

                // Optimization: no rule can possibly apply
                if ((current | last) == 0)
                { // i.e. both are zero
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

                // https://unicode.org/reports/tr29/#WB999
                // If we fall through all the above rules, it's a word break
                break;
            }

            return pos;
        }
    }
}
