﻿namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Words
{
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

            while (runes.MoveNext())
            {
                var last = current;

                var rune = runes.Current;
                var w = rune.Utf16SequenceLength;
                current = Dict.Lookup(rune.Value);

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
                if (current.Is(AHLetter) && last.Is(AHLetter | Ignore))
                {
                    // Otherwise, do proper look back per WB4
                    if (Previous(AHLetter, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB6 can possibly apply
                var maybeWB6 = current.Is(MidLetter | MidNumLetQ) && last.Is(AHLetter | Ignore);

                // https://unicode.org/reports/tr29/#WB6
                if (maybeWB6)
                {
                    if (Subsequent(AHLetter, runes) && Previous(AHLetter, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7 can possibly apply
                var maybeWB7 = current.Is(AHLetter) && last.Is(MidLetter | MidNumLetQ | Ignore);

                // https://unicode.org/reports/tr29/#WB7
                if (maybeWB7)
                {
                    if (Previous(AHLetter, runes, MidLetter | MidNumLetQ))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7a can possibly apply
                var maybeWB7a = current.Is(Single_Quote) && last.Is(Hebrew_Letter | Ignore);

                // https://unicode.org/reports/tr29/#WB7a
                if (maybeWB7a)
                {
                    if (Previous(Hebrew_Letter, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7b can possibly apply
                var maybeWB7b = current.Is(Double_Quote) && last.Is(Hebrew_Letter | Ignore);

                // https://unicode.org/reports/tr29/#WB7b
                if (maybeWB7b)
                {
                    if (Subsequent(Hebrew_Letter, runes) && Previous(Hebrew_Letter, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7c can possibly apply
                var maybeWB7c = current.Is(Hebrew_Letter) && last.Is(Double_Quote | Ignore);

                // https://unicode.org/reports/tr29/#WB7c
                if (maybeWB7c)
                {
                    if (Previous(Hebrew_Letter, runes, Double_Quote))
                    {
                        pos += w;
                        continue;
                    }
                }

                // https://unicode.org/reports/tr29/#WB8
                // https://unicode.org/reports/tr29/#WB9
                // https://unicode.org/reports/tr29/#WB10
                if (current.Is(Numeric | AHLetter) && last.Is(Numeric | AHLetter | Ignore))
                {
                    // Note: this logic de facto expresses WB5 as well, but harmless since WB5
                    // was already tested above

                    // // Optimization: maybe a run without ignored characters
                    // if (last.Is(Numeric | AHLetter))
                    // {
                    //     pos += w;
                    //     while (pos < input.Length)
                    //     {
                    //         status = DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
                    //         if (status != OperationStatus.Done)
                    //         {
                    //             // Garbage in, garbage out
                    //             break;
                    //         }
                    //         if (w2 == 0)
                    //         {
                    //             break;
                    //         }

                    //         var lookup = Dict.Lookup(rune2.Value);

                    //         if (!lookup.Is(Numeric | AHLetter))
                    //         {
                    //             break;
                    //         }
                    //         if (w2 == 0)
                    //         {
                    //             break;
                    //         }

                    //         // Update stateful vars
                    //         current = lookup;
                    //         w = w2;

                    //         pos += w;
                    //     }
                    //     continue;
                    // }

                    // Otherwise, do proper lookback per WB4
                    if (Previous(Numeric | AHLetter, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB11 can possibly apply
                var maybeWB11 = current.Is(Numeric) && last.Is(MidNum | MidNumLetQ | Ignore);

                // https://unicode.org/reports/tr29/#WB11
                if (maybeWB11)
                {
                    if (Previous(Numeric, runes, MidNum | MidNumLetQ))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB12 can possibly apply
                var maybeWB12 = current.Is(MidNum | MidNumLetQ) && last.Is(Numeric | Ignore);

                // https://unicode.org/reports/tr29/#WB12
                if (maybeWB12)
                {
                    if (Subsequent(Numeric, runes) && Previous(Numeric, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // https://unicode.org/reports/tr29/#WB13
                if (current.Is(Katakana) && last.Is(Katakana | Ignore))
                {
                    // // Optimization: maybe a run without ignored characters
                    // if (last.Is(Katakana))
                    // {
                    //     pos += w;
                    //     while (pos < input.Length)
                    //     {
                    //         status = DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
                    //         if (status != OperationStatus.Done)
                    //         {
                    //             // Garbage in, garbage out
                    //             break;
                    //         }
                    //         if (w2 == 0)
                    //         {
                    //             break;
                    //         }

                    //         var lookup = Dict.Lookup(rune2.Value);
                    //         if (!lookup.Is(Katakana))
                    //         {
                    //             break;
                    //         }

                    //         // Update stateful vars
                    //         current = lookup;
                    //         w = w2;

                    //         pos += w;
                    //     }
                    //     continue;
                    // }

                    // Otherwise, do proper lookback per WB4
                    if (Previous(Katakana, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB13a can possibly apply
                var maybeWB13a = current.Is(ExtendNumLet) && last.Is(AHLetter | Numeric | Katakana | ExtendNumLet | Ignore);

                // https://unicode.org/reports/tr29/#WB13a
                if (maybeWB13a)
                {
                    if (Previous(AHLetter | Numeric | Katakana | ExtendNumLet, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB13b can possibly apply
                var maybeWB13b = current.Is(AHLetter | Numeric | Katakana) && last.Is(ExtendNumLet | Ignore);

                // https://unicode.org/reports/tr29/#WB13b
                if (maybeWB13b)
                {
                    if (Previous(ExtendNumLet, runes))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB15 or WB16 can possibly apply
                var maybeWB1516 = current.Is(Regional_Indicator) && last.Is(Regional_Indicator | Ignore);

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
                        var lookup = Dict.Lookup(rune2.Value);

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
