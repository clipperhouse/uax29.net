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
    public void Static()
    {
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