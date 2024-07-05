using System.Buffers;
using System.Text;

namespace UAX29;

internal static class Decoders
{
    internal readonly static Decoders<char> Char = new(Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);
    internal readonly static Decoders<byte> Utf8 = new(Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8);
}

internal delegate OperationStatus Decoder<TSpan>(ReadOnlySpan<TSpan> input, out Rune result, out int consumed);

internal class Decoders<T>
{
    internal Decoder<T> FirstRune;
    internal Decoder<T> LastRune;

    internal Decoders(Decoder<T> firstRune, Decoder<T> lastRune)
    {
        this.FirstRune = firstRune;
        this.LastRune = lastRune;
    }
}

