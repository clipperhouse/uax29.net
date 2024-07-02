namespace UAX29;

/// A bitmap of Unicode categories
using Property = uint;

internal static partial class Sentences
{
    internal static readonly Split2<byte> Split2Utf8Bytes = new Splitter2<byte>().Split;
    internal static readonly Split2<char> Split2Chars = new Splitter2<char>().Split;

    internal sealed class Splitter2<TSpan> : SplitterBase2<TSpan> where TSpan : struct
    {
        internal Splitter2() : base(Sentences.Dict, Ignore) { }

        const Property SATerm = STerm | ATerm;
        const Property ParaSep = Sep | CR | LF;
        new const Property Ignore = Extend | Format;

        internal override int Split(RuneTokenizer<TSpan> runes, bool atEOF = true)
        {
            // These vars are stateful across loop iterations
            int pos = 0;
            Property current = 0;
            Property lastExIgnore = 0;      // "last excluding ignored categories"
            Property lastLastExIgnore = 0;  // "last one before that"
            Property lastExIgnoreSp = 0;
            Property lastExIgnoreClose = 0;
            Property lastExIgnoreSpClose = 0;

            // https://unicode.org/reports/tr29/#SB1
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

                var rune = runes.Current;
                var w = runes.CurrentWidth;
                current = Dict.Lookup(rune);

                // Optimization: no rule can possibly apply
                if ((current | last) == 0)  // i.e. both are zero
                {
                    pos += w;
                    break;
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
                    var runesRight = runes;

                    // ( ¬(OLetter | Upper | Lower | ParaSep | SATerm) )*
                    // Zero or more of not-the-above properties; consume them
                    while (runesRight.Any())
                    {
                        var lookup = Dict.Lookup(runesRight.Current);

                        if (lookup.Is(Ignore))
                        {
                            runesRight.MoveNext();
                            continue;
                        }

                        if (!lookup.Is(OLetter | Upper | Lower | ParaSep | SATerm))
                        {
                            runesRight.MoveNext();
                            continue;
                        }

                        break;
                    }

                    // Skip the Ignore
                    while (runesRight.Any())
                    {
                        var lookup = Dict.Lookup(runesRight.Current);
                        if (lookup.Is(Ignore))
                        {
                            runesRight.MoveNext();
                            continue;
                        }

                        break;
                    }

                    // Followed by a Lower
                    {
                        var lookup = Dict.Lookup(runesRight.Current);
                        if (lookup.Is(Lower))
                        {
                            pos += w;
                            continue;
                        }
                    }
                }

                // https://unicode.org/reports/tr29/#SB8a
                if (current.Is(SContinue | SATerm) && lastExIgnoreSpClose.Is(SATerm))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#SB9
                if (current.Is(Close | Sp | ParaSep) && lastExIgnoreClose.Is(SATerm))
                {
                    pos += w;
                    continue;
                }

                // https://unicode.org/reports/tr29/#SB8a
                if (current.Is(Sp | ParaSep) && lastExIgnoreSpClose.Is(SATerm))
                {
                    pos += w;
                    continue;
                }

                var maybeSB11 = lastExIgnore.Is(SATerm | Close | Sp | ParaSep);

                // https://unicode.org/reports/tr29/#SB11
                if (maybeSB11)
                {
                    // Start looking back
                    var runesLeft = runes;

                    // Zero or one ParaSep
                    while (runesLeft.MovePrevious())
                    {
                        var lookup = Dict.Lookup(runesLeft.Current);

                        if (lookup.Is(Ignore))
                        {
                            continue;
                        }

                        if (lookup.Is(ParaSep))
                        {
                            break;
                        }

                        // un-consume the rune
                        runesLeft.MoveNext();
                        break;
                    }

                    // Zero or more Sp
                    while (runesLeft.MovePrevious())
                    {
                        var lookup = Dict.Lookup(runesLeft.Current);

                        if (lookup.Is(Ignore))
                        {
                            continue;
                        }

                        if (lookup.Is(Sp))
                        {
                            continue;
                        }

                        // un-consume the rune
                        runesLeft.MoveNext();
                        break;
                    }

                    // Zero or more Close
                    while (runesLeft.MovePrevious())
                    {
                        var lookup = Dict.Lookup(runesLeft.Current);

                        if (lookup.Is(Ignore))
                        {
                            continue;
                        }

                        if (lookup.Is(Close))
                        {
                            continue;
                        }

                        // un-consume the rune
                        runesLeft.MoveNext();
                        break;
                    }

                    // Having looked back past Sp's, Close's, and intervening Ignore,
                    // is there an SATerm?
                    if (runesLeft.MovePrevious())
                    {
                        var lookup = Dict.Lookup(runesLeft.Current);
                        if (lookup.Is(SATerm))
                        {
                            break;
                        }
                    }
                }

                // https://unicode.org/reports/tr29/#SB998
                pos += w;
            }

            return pos;
        }
    }
}
