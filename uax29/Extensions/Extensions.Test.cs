namespace Tests;

using UAX29.Extensions;
using System.Text;

[TestFixture]
public class TestExtensions
{
    [SetUp]
    public void Setup()
    {
    }

    static int ExpectedOverloads()
    {
        var expected = 0;

        expected++;     // string
        expected++;     // char[]
        expected++;     // Span<char>
        expected++;     // ReadOnlySpan<char>
        expected++;     // Memory<char>
        expected++;     // ReadOnlyMemory<char>

        expected++;     // byte[]
        expected++;     // Span<byte>
        expected++;     // ReadOnlySpan<byte>
        expected++;     // Memory<byte>
        expected++;     // ReadOnlyMemory<byte>

        expected++;     // Stream
        expected++;     // TextReader

        expected *= 3;  // Words, Graphemes, Sentences

        return expected;
    }


    [Test]
    public void Overloads()
    {
        // no assertions, just needs to compile

        int expected = ExpectedOverloads();
        int got = 0;

        var input = "Hello, how are you?";
        var bytes = Encoding.UTF8.GetBytes(input);
        using var stream = new MemoryStream(bytes);
        using var reader = new StreamReader(stream);

        {
            // string
            input.SplitWords(); got++;

            // char[]
            input.ToCharArray().SplitWords(); got++;

            // ReadOnlySpan<char>
            input.AsSpan().SplitWords(); got++;

            // Span<char>
            var span = new Span<char>(input.ToCharArray());
            span.SplitWords(); got++;

            // Memory<char>
            var mem = new Memory<char>(input.ToCharArray());
            mem.SplitWords(); got++;

            // ReadOnlyMemoryMemory<char>
            ReadOnlyMemory<char> rmem = input.AsMemory();
            rmem.SplitWords(); got++;

            reader.SplitWords(); got++;
        }

        {
            // chars

            input.SplitGraphemes(); got++;

            var array = input.ToCharArray();
            array.SplitGraphemes(); got++;

            var span = new Span<char>(array);
            span.SplitGraphemes(); got++;

            ReadOnlySpan<char> rspan = input.AsSpan();
            rspan.SplitGraphemes(); got++;

            var mem = new Memory<char>(array);
            mem.SplitGraphemes(); got++;

            ReadOnlyMemory<char> rmem = input.AsMemory();
            rmem.SplitGraphemes(); got++;

            reader.SplitGraphemes(); got++;
        }


        {
            // chars

            input.SplitSentences(); got++;

            var array = input.ToCharArray();
            array.SplitSentences(); got++;

            var span = new Span<char>(array);
            span.SplitSentences(); got++;

            ReadOnlySpan<char> rspan = input.AsSpan();
            rspan.SplitSentences(); got++;

            var mem = new Memory<char>(array);
            mem.SplitSentences(); got++;

            ReadOnlyMemory<char> rmem = input.AsMemory();
            rmem.SplitSentences(); got++;

            reader.SplitSentences(); got++;
        }

        {
            // bytes

            bytes.SplitWords(); got++;

            Span<byte> span = bytes.AsSpan();
            span.SplitWords(); got++;

            ReadOnlySpan<byte> rspan = bytes.AsSpan();
            rspan.SplitWords(); got++;

            Memory<byte> mem = bytes.AsMemory();
            mem.SplitWords(); got++;

            ReadOnlyMemory<byte> rmem = bytes.AsMemory();
            rmem.SplitWords(); got++;

            stream.SplitWords(); got++;
        }


        {
            // bytes

            bytes.SplitGraphemes(); got++;

            Span<byte> span = bytes.AsSpan();
            span.SplitGraphemes(); got++;

            ReadOnlySpan<byte> rspan = bytes.AsSpan();
            rspan.SplitGraphemes(); got++;

            Memory<byte> mem = bytes.AsMemory();
            mem.SplitGraphemes(); got++;

            ReadOnlyMemory<byte> rmem = bytes.AsMemory();
            rmem.SplitGraphemes(); got++;

            stream.SplitGraphemes(); got++;
        }


        {
            // bytes

            bytes.SplitSentences(); got++;

            Span<byte> span = bytes.AsSpan();
            span.SplitSentences(); got++;

            ReadOnlySpan<byte> rspan = bytes.AsSpan();
            rspan.SplitSentences(); got++;

            Memory<byte> mem = bytes.AsMemory();
            mem.SplitSentences(); got++;

            ReadOnlyMemory<byte> rmem = bytes.AsMemory();
            rmem.SplitSentences(); got++;

            stream.SplitSentences(); got++;
        }

        Assert.That(got, Is.EqualTo(expected));
    }
}
