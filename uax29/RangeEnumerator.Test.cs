namespace Tests;

using UAX29;
using System.Linq;
using System.Text;

[TestFixture]
public class TestRangeTokenizer
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

		var words = Tokenizer.GetWords(example);
		var ranges = words.Ranges;

		var first = new List<Range>();
		foreach (var range in ranges)
		{
			first.Add(range);
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		words.Reset();

		var second = new List<Range>();
		foreach (var range in ranges)
		{
			second.Add(range);
		}

		Assert.That(first.SequenceEqual(second));
	}

	static readonly Options[] options = [Options.None, Options.OmitWhitespace];

	[Test]
	public void MatchesTokenizer()
	{
		var example = "abcdefghijk lmnopq r \tstu vwxyz; ABC DEFG \r\nHIJKL MNOP Q RSTUV WXYZ! 你好，世界.\r";

		foreach (var option in options)
		{
			var tokens = Tokenizer.GetWords(example, option);
			var ranges = tokens.Ranges;

			foreach (var range in ranges)
			{
				tokens.MoveNext();

				var ranged = example.AsSpan(range);
				var token = tokens.Current;
				Assert.That(token.SequenceEqual(ranged));
			}
		}
	}

	[Test]
	public void Enumerator()
	{
		var input = "Hello, how are you?";
		var mem = input.AsMemory();
		Tokenizer.GetWords(mem);

		var words = Tokenizer.GetWords(input);
		var ranges = words.Ranges;

		var first = new List<Range>();
		while (ranges.MoveNext())
		{
			first.Add(ranges.Current);
		}
		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing


		var tokens2 = Tokenizer.GetWords(input);
		var ranges2 = words.Ranges;

		var second = new List<Range>();
		foreach (var range in ranges2)
		{
			second.Add(range);
		}
		Assert.That(first.SequenceEqual(second));
	}

	[Test]
	public void ToList()
	{
		var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
		var words = Tokenizer.GetWords(example);
		var ranges = words.Ranges;
		var list = ranges.ToList();

		var i = 0;
		foreach (var range in ranges)
		{
			Assert.That(range, Is.EqualTo(list[i]));
			i++;
		}

		Assert.That(list, Has.Count.EqualTo(i), "ToList should return the same number of tokens as iteration");

		var threw = false;
		ranges.MoveNext();
		try
		{
			ranges.ToList();
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
		var words = Tokenizer.GetWords(example);
		var ranges = words.Ranges;
		var array = ranges.ToArray();

		var i = 0;
		foreach (var range in ranges)
		{
			Assert.That(range, Is.EqualTo(array[i]));
			i++;
		}

		Assert.That(array, Has.Length.EqualTo(i), "ToArray should return the same number of tokens as iteration");

		var threw = false;
		ranges.MoveNext();
		try
		{
			ranges.ToArray();
		}
		catch (InvalidOperationException)
		{
			threw = true;
		}
		Assert.That(threw, Is.True, "Calling ToArray after iteration has begun should throw");
	}
}
