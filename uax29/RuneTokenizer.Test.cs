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
		var expected = example.EnumerateRunes();

		{
			var runes = new RuneTokenizer<char>(example, Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16).ToArray();
			Assert.That(runes.SequenceEqual(expected));
		}

		{
			var bytes = Encoding.UTF8.GetBytes(example);
			var runes = new RuneTokenizer<byte>(bytes, Rune.DecodeFromUtf8, Rune.DecodeLastFromUtf8).ToArray();
			Assert.That(runes.SequenceEqual(expected));
		}
	}

	[Test]
	public void Previous()
	{
		var example = "Hello, how are you, 😀 👨‍❤️‍👩";
		var expected = example.EnumerateRunes().Reverse().ToArray();
		var runes = new RuneTokenizer<char>(example, Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);

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

	[Test]
	public void SetText()
	{
		var example = "Hello, how are you?";

		var tokens = Tokenizer.GetWords(example);

		var first = new List<string>();
		foreach (var token in tokens)
		{
			first.Add(token.ToString());
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		tokens.SetText(example);

		var second = new List<string>();
		foreach (var token in tokens)
		{
			second.Add(token.ToString());
		}

		Assert.That(first.SequenceEqual(second));
	}

	[Test]
	public void Enumerator()
	{
		var input = "Hello, how are you?";
		var mem = input.AsMemory();
		var bytes = Encoding.UTF8.GetBytes(input);
		Tokenizer.GetWords(mem);

		var tokens = Tokenizer.GetWords(input);
		var first = new List<string>();
		while (tokens.MoveNext())
		{
			var s = tokens.Current.ToString();
			first.Add(s);
		}
		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing		

		var tokens2 = Tokenizer.GetWords(input);
		var second = new List<string>();
		foreach (var token in tokens2)
		{
			var s = token.ToString();
			second.Add(s);
		}
		Assert.That(first.SequenceEqual(second));
	}

	[Test]
	public void ToList()
	{
		var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
		var runes = new RuneTokenizer<char>(example, Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);
		var list = runes.ToList();

		var i = 0;
		while (runes.MoveNext())
		{
			Assert.That(runes.Current, Is.EqualTo(list[i]));
			i++;
		}

		Assert.That(list, Has.Count.EqualTo(i), "ToList should return the same number of tokens as iteration");

		var threw = false;
		runes.MoveNext();
		try
		{
			runes.ToList();
		}
		catch (InvalidOperationException)
		{
			threw = true;
		}
		Assert.That(threw, Is.True, "Calling ToList after iteration has begun should throw");
	}


	[Test]
	public void ToArray()
	{
		var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
		var runes = new RuneTokenizer<char>(example, Rune.DecodeFromUtf16, Rune.DecodeLastFromUtf16);
		var array = runes.ToArray();

		var i = 0;
		while (runes.MoveNext())
		{
			Assert.That(runes.Current, Is.EqualTo(array[i]));
			i++;
		}

		Assert.That(array, Has.Length.EqualTo(i), "ToArray should return the same number of tokens as iteration");

		var threw = false;
		runes.MoveNext();
		try
		{
			runes.ToArray();
		}
		catch (InvalidOperationException)
		{
			threw = true;
		}
		Assert.That(threw, Is.True, "Calling ToArray after iteration has begun should throw");
	}
}
