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
        var example = "abc defg \"hijkl\" mnopq r 你好.";
        var words = Tokenizer.GetWords(example).ToArray();

        var runes = new RuneTokenizer<char>(example, Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);

        var splitter = new Words.Splitter2<char>();
        var start = 0;
        var end = 0;

        var i = 0;
        while (true)
        {
            var advance = splitter.Split(runes);
            if (advance == 0)
            {
                break;
            }

            start = end;
            end += advance;
            var val = example[start..end];

            Assert.That(val, Is.EqualTo(words[i]));
            runes.Consume(advance);

            i++;
        }
    }
}