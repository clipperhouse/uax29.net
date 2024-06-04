namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Words
{
    internal static readonly Split Split = new Splitter(Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8).Split;

    internal class Splitter : SplitterBase
    {
        internal Splitter(Decoder decodeFirstRune, Decoder decodeLastRune) :
            base(dict, Ignore, decodeFirstRune, decodeLastRune)
        { }

        const Property AHLetter = ALetter | Hebrew_Letter;
        const Property MidNumLetQ = MidNumLet | Single_Quote;
        new const Property Ignore = Extend | Format | ZWJ;

        public override int Split(ReadOnlySpan<byte> input, bool atEOF = true)
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
                if (current.Iss(LF) && last.Iss(CR))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#WB3a
                // https://unicode.org/reports/tr29/#WB3b
                if ((last | current).Iss(Newline | CR | LF))
                {
                    break;
                }

                // https://unicode.org/reports/tr29/#WB3c
                if (current.Iss(Extended_Pictographic) && last.Iss(ZWJ))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#WB3d
                if ((current & last).Iss(WSegSpace))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#WB4
                if (current.Iss(Extend | Format | ZWJ))
                {
                    pos += w;
                    continue;
                }

                // WB4 applies to subsequent rules; there is an implied "ignoring Extend & Format & ZWJ"
                // https://unicode.org/reports/tr29/#Grapheme_Cluster_and_Format_Rules
                // The previous/subsequent methods are shorthand for "seek a property but skip over Extend|Format|ZWJ on the way"

                // https://unicode.org/reports/tr29/#WB5
                if (current.Iss(AHLetter) && last.Iss(AHLetter | Ignore))
                {
                    // Optimization: maybe a run without ignored characters
                    if (last.Iss(AHLetter))
                    {
                        pos += w;
                        while (pos < input.Length)
                        {
                            status = DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
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
                            if (!lookup.Iss(AHLetter))
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
                    if (Previous(AHLetter, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB6 can possibly apply
                var maybeWB6 = current.Iss(MidLetter | MidNumLetQ) && last.Iss(AHLetter | Ignore);

                // https://unicode.org/reports/tr29/#WB6
                if (maybeWB6)
                {
                    if (Subsequent(AHLetter, input[(pos + w)..]) && Previous(AHLetter, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7 can possibly apply
                var maybeWB7 = current.Iss(AHLetter) && last.Iss(MidLetter | MidNumLetQ | Ignore);

                // https://unicode.org/reports/tr29/#WB7
                if (maybeWB7)
                {
                    var i = PreviousIndex(MidLetter | MidNumLetQ, input[..pos]);
                    if (i > 0 && Previous(AHLetter, input[..i]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7a can possibly apply
                var maybeWB7a = current.Iss(Single_Quote) && last.Iss(Hebrew_Letter | Ignore);

                // https://unicode.org/reports/tr29/#WB7a
                if (maybeWB7a)
                {
                    if (Previous(Hebrew_Letter, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7b can possibly apply
                var maybeWB7b = current.Iss(Double_Quote) && last.Iss(Hebrew_Letter | Ignore);

                // https://unicode.org/reports/tr29/#WB7b
                if (maybeWB7b)
                {
                    if (Subsequent(Hebrew_Letter, input[(pos + w)..]) && Previous(Hebrew_Letter, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB7c can possibly apply
                var maybeWB7c = current.Iss(Hebrew_Letter) && last.Iss(Double_Quote | Ignore);

                // https://unicode.org/reports/tr29/#WB7c
                if (maybeWB7c)
                {
                    var i = PreviousIndex(Double_Quote, input[..pos]);
                    if (i > 0 && Previous(Hebrew_Letter, input[..i]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // https://unicode.org/reports/tr29/#WB8
                // https://unicode.org/reports/tr29/#WB9
                // https://unicode.org/reports/tr29/#WB10
                if (current.Iss(Numeric | AHLetter) && last.Iss(Numeric | AHLetter | Ignore))
                {
                    // Note: this logic de facto expresses WB5 as well, but harmless since WB5
                    // was already tested above

                    // Optimization: maybe a run without ignored characters
                    if (last.Iss(Numeric | AHLetter))
                    {
                        pos += w;
                        while (pos < input.Length)
                        {
                            status = DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
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

                            if (!lookup.Iss(Numeric | AHLetter))
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
                    if (Previous(Numeric | AHLetter, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB11 can possibly apply
                var maybeWB11 = current.Iss(Numeric) && last.Iss(MidNum | MidNumLetQ | Ignore);

                // https://unicode.org/reports/tr29/#WB11
                if (maybeWB11)
                {
                    var i = PreviousIndex(MidNum | MidNumLetQ, input[..pos]);
                    if (i > 0 && Previous(Numeric, input[..i]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB12 can possibly apply
                var maybeWB12 = current.Iss(MidNum | MidNumLetQ) && last.Iss(Numeric | Ignore);

                // https://unicode.org/reports/tr29/#WB12
                if (maybeWB12)
                {
                    if (Subsequent(Numeric, input[(pos + w)..]) && Previous(Numeric, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // https://unicode.org/reports/tr29/#WB13
                if (current.Iss(Katakana) && last.Iss(Katakana | Ignore))
                {
                    // Optimization: maybe a run without ignored characters
                    if (last.Iss(Katakana))
                    {
                        pos += w;
                        while (pos < input.Length)
                        {
                            status = DecodeFirstRune(input[pos..], out Rune rune2, out int w2);
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
                            if (!lookup.Iss(Katakana))
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
                    if (Previous(Katakana, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB13a can possibly apply
                var maybeWB13a = current.Iss(ExtendNumLet) && last.Iss(AHLetter | Numeric | Katakana | ExtendNumLet | Ignore);

                // https://unicode.org/reports/tr29/#WB13a
                if (maybeWB13a)
                {
                    if (Previous(AHLetter | Numeric | Katakana | ExtendNumLet, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB13b can possibly apply
                var maybeWB13b = current.Iss(AHLetter | Numeric | Katakana) && last.Iss(ExtendNumLet | Ignore);

                // https://unicode.org/reports/tr29/#WB13b
                if (maybeWB13b)
                {
                    if (Previous(ExtendNumLet, input[..pos]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // Optimization: determine if WB15 or WB16 can possibly apply
                var maybeWB1516 = current.Iss(Regional_Indicator) && last.Iss(Regional_Indicator | Ignore);

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
                        if (status != OperationStatus.Done)
                        {
                            // Garbage in, garbage out
                            break;
                        }

                        if (lookup.Iss(Ignore))
                        {
                            continue;
                        }

                        if (!lookup.Iss(Regional_Indicator))
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
