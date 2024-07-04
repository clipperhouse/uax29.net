namespace UAX29;

using System.Buffers;
using System.Diagnostics;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Sentences
{
    internal static readonly Split<byte> SplitBytes = new Splitter<byte>(Decoders.Utf8).Split;
    internal static readonly Split<char> SplitChars = new Splitter<char>(Decoders.Char).Split;

    internal sealed class Splitter<TSpan>
    {
        internal readonly Decoders<TSpan> Decode;
        internal Splitter(Decoders<TSpan> decoders)
        {
            this.Decode = decoders;
        }

        const Property SATerm = STerm | ATerm;
        const Property ParaSep = Sep | CR | LF;
        const Property Ignore = Extend | Format;

        internal int Split(ReadOnlySpan<TSpan> input)
        {
            Debug.Assert(input.Length > 0);

            // These vars are stateful across loop iterations
            var pos = 0;
            int w;
            Property current = 0;
            Property lastExIgnore = 0;      // "last excluding ignored categories"
            Property lastLastExIgnore = 0;  // "last one before that"
            Property lastExIgnoreSp = 0;
            Property lastExIgnoreClose = 0;
            Property lastExIgnoreSpClose = 0;

            {
                // https://unicode.org/reports/tr29/#SB1
                // start of text always advances
                var status = Decode.FirstRune(input[pos..], out Rune rune, out w);
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

            // https://unicode.org/reports/tr29/#SB2
            while (pos < input.Length)
            {
                // Rules are usually of the form Cat1 × Cat2; "current" refers to the first property
                // to the right of the ×, from which we look back or forward

                var last = current;

                if (!last.Is(Ignore))
                {
                    lastLastExIgnore = lastExIgnore;
                    lastExIgnore = last;
                }

                if (!lastExIgnore.Is(Sp))
                {
                    lastExIgnoreSp = lastExIgnore;
                }

                if (!lastExIgnore.Is(Close))
                {
                    lastExIgnoreClose = lastExIgnore;
                }

                if (!lastExIgnoreSp.Is(Close))
                {
                    lastExIgnoreSpClose = lastExIgnoreSp;
                }

                var status = Decode.FirstRune(input[pos..], out Rune rune, out w);
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

                // https://unicode.org/reports/tr29/#SB6
                if (current.Is(Numeric) && lastExIgnore.Is(ATerm))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#SB7
                if (current.Is(Upper) && lastExIgnore.Is(ATerm) && lastLastExIgnore.Is(Upper | Lower))
                {
                    pos += w;
                    continue;
                }

                // Optimization: determine if SB8 can possibly apply
                var maybeSB8 = lastExIgnoreSpClose.Is(ATerm);

                // https://unicode.org/reports/tr29/#SB8
                if (maybeSB8)
                {
                    var p = pos;

                    // ( ¬(OLetter | Upper | Lower | ParaSep | SATerm) )*
                    // Zero or more of not-the-above properties
                    while (p < input.Length)
                    {
                        status = Decode.FirstRune(input[p..], out Rune rune2, out int w2);
                        Debug.Assert(w2 > 0);
                        if (status != OperationStatus.Done)
                        {
                            // Garbage in, garbage out
                            break;
                        }

                        var lookup = Dict.Lookup(rune2.Value);

                        if (lookup.Is(OLetter | Upper | Lower | ParaSep | SATerm))
                        {
                            break;
                        }

                        p += w2;
                    }

                    if (Subsequent(Lower, input[p..]))
                    {
                        pos += w;
                        continue;
                    }
                }

                // https://unicode.org/reports/tr29/#SB8a
                if (current.Is(SContinue | SATerm) && lastExIgnoreSpClose.Is(SATerm))
                {
                    pos += w;
                    continue;
                }

                // Optimization: determine if SB9 can possibly apply
                var maybeSB9 = current.Is(Close | Sp | ParaSep) && last.Is(SATerm | Close | Ignore);

                // https://unicode.org/reports/tr29/#SB9
                if (current.Is(Close | Sp | ParaSep) && lastExIgnoreClose.Is(SATerm))
                {
                    pos += w;
                    continue;
                }

                // Optimization: determine if SB10 can possibly apply
                var maybeSB10 = current.Is(Sp | ParaSep) && last.Is(SATerm | Close | Sp | Ignore);

                // https://unicode.org/reports/tr29/#SB10
                if (current.Is(Sp | ParaSep) && lastExIgnoreSpClose.Is(SATerm))
                {
                    pos += w;
                    continue;
                }

                // Optimization: determine if SB11 can possibly apply
                var maybeSB11 = lastExIgnore.Is(SATerm | Close | Sp | ParaSep);

                // https://unicode.org/reports/tr29/#SB11
                if (maybeSB11)
                {
                    var p = pos;

                    // Zero or one ParaSep
                    var ps = PreviousIndex(ParaSep, input[..p]);
                    if (ps >= 0)
                    {
                        p = ps;
                    }

                    // Zero or more Sp's
                    var sp = p;
                    while (true)
                    {
                        sp = PreviousIndex(Sp, input[..sp]);
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
                        close = PreviousIndex(Close, input[..close]);
                        if (close < 0)
                        {
                            break;
                        }
                        p = close;
                    }

                    // Having looked back past ParaSep, Sp's, Close's, and intervening Extend|Format,
                    // is there an SATerm?
                    if (Previous(SATerm, input[..p]))
                    {
                        break;
                    }
                }

                // https://unicode.org/reports/tr29/#SB998
                pos += w;
            }

            return pos;


            /// <summary>
            /// Seek backward until it hits a rune which matches property.
            /// </summary>
            /// <param name="property">Property to attempt to find</param>
            /// <param name="input">Data in which to seek</param>
            /// <returns>The index if found, or -1 if not</returns>
            int PreviousIndex(Property property, ReadOnlySpan<TSpan> input)
            {
                // Start at the end of the buffer and move backwards
                var i = input.Length;
                while (i > 0)
                {
                    var status = Decode.LastRune(input[..i], out Rune rune, out int w);
                    if (status != OperationStatus.Done)
                    {
                        // Garbage in, garbage out
                        break;
                    }
                    if (w == 0)
                    {
                        break;
                    }

                    i -= w;
                    var lookup = Dict.Lookup(rune.Value);

                    if (lookup.Is(Ignore))
                    {
                        continue;
                    }

                    if (lookup.Is(property))
                    {
                        return i;
                    }

                    // If we get this far, it's not there
                    break;
                }

                return -1;
            }

            /// <summary>
            /// Seek backward until it hits a rune which matches property.
            /// </summary>
            /// <param name="property">Property to attempt to find</param>
            /// <param name="input">Data in which to seek</param>
            /// <returns>True if found, otherwise false</returns>
            bool Previous(Property property, ReadOnlySpan<TSpan> input)
            {
                return PreviousIndex(property, input) != -1;
            }
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
                    var status = Decode.FirstRune(input[i..], out Rune rune, out int w);
                    Debug.Assert(w > 0);
                    if (status != OperationStatus.Done)
                    {
                        // Garbage in, garbage out
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
