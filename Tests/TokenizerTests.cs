namespace Tests;

using uax29;
using NUnit.Framework.Internal;
using System.Linq;
using System.Text;

[TestFixture]
public class TestTokenizer
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Reset()
    {
        var example = "Hello, how are you?";
        var bytes = Encoding.UTF8.GetBytes(example);

        var tokens = new Tokenizer(bytes);

        var first = new List<string>();
        while (tokens.MoveNext())
        {
            var s = Encoding.UTF8.GetString(tokens.Current);
            first.Add(s);
        }

        Assert.That(first.Count > 1);   // just make sure it did the thing

        tokens.Reset();

        var second = new List<string>();
        while (tokens.MoveNext())
        {
            var s = Encoding.UTF8.GetString(tokens.Current);
            second.Add(s);
        }

        Assert.That(first.SequenceEqual(second));
    }

}

