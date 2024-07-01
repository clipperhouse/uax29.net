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
        using var stream = assembly.GetManifestResourceStream("uax29.sample.txt") ?? throw new Exception("not found");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        example = reader.ReadToEnd();
    }

    [Test]
    public void Hmm()
    {
        var words = Tokenizer.GetWords(example).ToArray();

        var runes = RuneTokenizer.Create(example);

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

    [Test]
    public void Static()
    {
        var example = Encoding.UTF8.GetBytes(this.example);
        var words = Tokenizer.GetWords(this.example).ToArray();
        var words2 = Tokenizer2.GetWords(this.example).ToArray();

        Assert.That(words, Has.Length.EqualTo(words2.Length));

        for (var i = 0; i < words.Length; i++)
        {
            var expected = words[i].ToString();
            var got = words2[i].ToString();
            Assert.That(expected, Is.EqualTo(got));
        }
    }
}