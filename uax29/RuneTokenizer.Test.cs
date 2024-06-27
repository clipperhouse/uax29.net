namespace Tests;

using UAX29;
using System.Linq;
using System.Text;

[TestFixture]
public class TestRuneTokenizer
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Next()
	{
		var example = "Hello, how are you, 😀 👨‍❤️‍👩";
		var expected = example.EnumerateRunes().Select(r => r.Value);

		{
			var runes = RuneTokenizer.Create(example).ToArray();
			Assert.That(runes.SequenceEqual(expected));
		}

		{
			var bytes = Encoding.UTF8.GetBytes(example);
			var runes = RuneTokenizer.Create(bytes).ToArray();
			Assert.That(runes.SequenceEqual(expected));
		}
	}

	[Test]
	public void Previous()
	{
		var example = "Hello, how are you, 😀 👨‍❤️‍👩";
		var expected = example.EnumerateRunes().Reverse().Select(r => r.Value).ToArray();
		var runes = RuneTokenizer.Create(example);

		// move to the end	
		while (runes.MoveNext()) { }

		var i = 0;
		while (runes.MovePrevious())
		{
			Assert.That(runes.Current, Is.EqualTo(expected[i]));
			i++;
		}
	}

	[Test]
	public void Reset()
	{
		var example = "Hello, how are you?";
		var bytes = Encoding.UTF8.GetBytes(example);

		var tokens = Tokenizer.GetWords(example);

		var first = new List<string>();
		foreach (var token in tokens)
		{
			first.Add(token.ToString());
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		tokens.Reset();

		var second = new List<string>();
		foreach (var token in tokens)
		{
			second.Add(token.ToString());
		}

		Assert.That(first.SequenceEqual(second));
	}
}
