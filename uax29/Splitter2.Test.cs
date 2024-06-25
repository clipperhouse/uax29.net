namespace Tests;

using System.IO.Compression;
using System.Text;
using UAX29;

[TestFixture]
public class TestSplitter2
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Hmm()
    {
        var example = "abcdefghijk lmnopq r 你好.";

        var runes = new RuneTokenizer<char>(example, Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);

        var splitter = new Words.Splitter2<char>();
        var pos = 0;

        while (true)
        {
            var advance = splitter.Split(runes);
            var val = example[pos..(pos + advance)];
            pos += advance;
            runes.start = pos;

            if (advance == 0)
            {
                break;
            }
        }
    }
}