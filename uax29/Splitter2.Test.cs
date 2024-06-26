namespace Tests;

using System.Reflection;
using System.Text;
using UAX29;

[TestFixture]
public class TestSplitter2
{
    string example;

    [SetUp]
    public void Setup()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var names = assembly.GetManifestResourceNames();
        using var stream = assembly.GetManifestResourceStream("uax29.sample.txt");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        example = reader.ReadToEnd();
    }

    [Test]
    public void Hmm()
    {
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

            var got = example[start..end];
            var expected = words[i];

            Assert.That(got, Is.EqualTo(expected));
            runes.Consume(advance);

            i++;
        }
    }
}