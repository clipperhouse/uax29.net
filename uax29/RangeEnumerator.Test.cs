namespace Tests;

using UAX29;
using System.Linq;
using System.Text;

[TestFixture]
public class TestRangeEnumerator
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

		var words = Split.Words(example);
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
	public void MatchesSplit()
	{
		var example = "abcdefghijk lmnopq r \tstu vwxyz; ABC DEFG \r\nHIJKL MNOP Q RSTUV WXYZ! 你好，世界.\r";

		foreach (var option in options)
		{
			var tokens = Split.Words(example, option);
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

		var words = Split.Words(input);
		var first = new List<string>();
		foreach (var word in words)
		{
			first.Add(word.ToString());
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		var ranges = Split.Words(input).Ranges;
		var second = new List<string>();
		foreach (var range in ranges)
		{
			second.Add(input[range]);
		}
		Assert.That(first.SequenceEqual(second));
	}

	[Test]
	public void ToList()
	{
		var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
		var words = Split.Words(example);
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
		var words = Split.Words(example);
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
