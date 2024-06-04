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

        var tokens = Tokenizer.Create(example);

        var first = new List<string>();
        while (tokens.MoveNext())
        {
            // do something with tokens.Current
            first.Add(tokens.Current.ToString());
        }

        Assert.That(first.Count > 1);   // just make sure it did the thing

        tokens.Reset();

        var second = new List<string>();
        while (tokens.MoveNext())
        {
            // do something with tokens.Current
            second.Add(tokens.Current.ToString());
        }

        Assert.That(first.SequenceEqual(second));
    }

}

