﻿namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Words
{
    internal static readonly Split2<byte> Split2Utf8Bytes = new Splitter2<byte>().Split;
    internal static readonly Split2<char> Split2Chars = new Splitter2<char>().Split;

    internal sealed class Splitter2<TSpan> : SplitterBase2<TSpan> where TSpan : struct
    {
        internal Splitter2() :
            base(Words.Dict, Ignore)
        { }

        const Property AHLetter = ALetter | Hebrew_Letter;
        const Property MidNumLetQ = MidNumLet | Single_Quote;
        new const Property Ignore = Extend | Format | ZWJ;

        internal override int Split(RuneTokenizer<TSpan> runes, bool atEOF = true)
        {
            // These vars are stateful across loop iterations
            int pos = 0;
            Property current = 0;
            Property lastExIgnore = 0;      // "last excluding ignored categories"
            Property lastLastExIgnore = 0;  // "last one before that"

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

                var sot = pos == 0;             // "start of text"

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
                if (current.Is(AHLetter) && lastExIgnore.Is(AHLetter))
                {
                    pos += w;
                    continue;
                }

                // Optimization: determine if WB6 can possibly apply
                var maybeWB6 = current.Is(MidLetter | MidNumLetQ) && lastExIgnore.Is(AHLetter);

                // https://unicode.org/reports/tr29/#WB6
                if (maybeWB6)
                {
                    if (Subsequent(AHLetter, runes))
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

                // Optimization: determine if WB7b can possibly apply
                var maybeWB7b = current.Is(Double_Quote) && lastExIgnore.Is(Hebrew_Letter);

                // https://unicode.org/reports/tr29/#WB7b
                if (maybeWB7b)
                {
                    if (Subsequent(Hebrew_Letter, runes))
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

                // Optimization: determine if WB12 can possibly apply
                var maybeWB12 = current.Is(MidNum | MidNumLetQ) && lastExIgnore.Is(Numeric);

                // https://unicode.org/reports/tr29/#WB12
                if (maybeWB12)
                {
                    if (Subsequent(Numeric, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // https://unicode.org/reports/tr29/#WB13
                if (current.Is(Katakana) && lastExIgnore.Is(Katakana))
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

                // Optimization: determine if WB15 or WB16 can possibly apply
                var maybeWB1516 = current.Is(Regional_Indicator) && lastExIgnore.Is(Regional_Indicator);

                // https://unicode.org/reports/tr29/#WB15 and
                // https://unicode.org/reports/tr29/#WB16
                if (maybeWB1516)
                {
                    // WB15: Odd number of RI before hitting start of text
                    // WB16: Odd number of RI before hitting [^RI], aka "not RI"

                    var count = 0;

                    var runes2 = runes; // shallow copy
                    while (runes2.MovePrevious())
                    {
                        var rune2 = runes2.Current;
                        var lookup = Dict.Lookup(rune2);

                        if (lookup.Is(Ignore))
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
