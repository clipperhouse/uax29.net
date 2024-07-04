namespace UAX29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Words
{
    internal static readonly Split<byte> SplitUtf8Bytes = new Splitter<byte>(Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8).Split;
    internal static readonly Split<char> SplitChars = new Splitter<char>(Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16).Split;

    internal sealed class Splitter<TSpan> : SplitterBase<TSpan>
    {
        internal Splitter(Decoder<TSpan> decodeFirstRune, Decoder<TSpan> decodeLastRune) :
            base(decodeFirstRune, decodeLastRune)
        { }

        const Property AHLetter = ALetter | Hebrew_Letter;
        const Property MidNumLetQ = MidNumLet | Single_Quote;
        const Property Ignore = Extend | Format | ZWJ;

        internal override int Split(ReadOnlySpan<TSpan> input, bool atEOF = true)
        {
            var len = input.Length;
            if (len == 0)
            {
                return 0;
            }

            // These vars are stateful across loop iterations
            int pos = 0;
            int w;
            Property current = 0;
            Property lastExIgnore = 0;      // "last excluding ignored categories"
            Property lastLastExIgnore = 0;  // "the last one before that"
            int regionalIndicatorCount = 0;

            {
                // start of text always advances
                var status = DecodeFirstRune(input[pos..], out Rune rune, out w);
                if (status != OperationStatus.Done)
                {
                    // Garbage in, garbage out
                    pos += w;
                    return pos;
                }
                current = Dict.Lookup(rune.Value);
                pos += w;
            }

            while (pos < len)
            {
                var last = current;
                if (!last.Is(Ignore))
                {
                    lastLastExIgnore = lastExIgnore;
                    lastExIgnore = last;
                }

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

                // Optimization: determine if WB7b can possibly apply
                var maybeWB7b = current.Is(Double_Quote) && lastExIgnore.Is(Hebrew_Letter);

                // https://unicode.org/reports/tr29/#WB7b
                if (maybeWB7b)
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

                // Optimization: determine if WB12 can possibly apply
                var maybeWB12 = current.Is(MidNum | MidNumLetQ) && lastExIgnore.Is(Numeric);

                // https://unicode.org/reports/tr29/#WB12
                if (maybeWB12)
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

                // Optimization: determine if WB15 or WB16 can possibly apply
                var maybeWB1516 = current.Is(Regional_Indicator) && lastExIgnore.Is(Regional_Indicator);

                // https://unicode.org/reports/tr29/#WB15
                // https://unicode.org/reports/tr29/#WB16
                if (maybeWB1516)
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
                    var status = DecodeFirstRune(input[i..], out Rune rune, out int w);
                    if (status != OperationStatus.Done)
                    {
                        // Garbage in, garbage out
                        break;
                    }
                    if (w == 0)
                    {
                        break;
                    }

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
