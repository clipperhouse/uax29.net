namespace Tests;

using UAX29;
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

		var tokens = Split.Words(example);

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

		var tokens = Split.Words(example);

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

	static int ExpectedOverloads()
	{
		var expected = 0;

		expected++;     // string
		expected++;     // char[]
		expected++;     // Span<char>
		expected++;     // ReadOnlySpan<char>
		expected++;     // Memory<char>
		expected++;     // ReadOnlyMemory<char>

		expected++;     // byte[]
		expected++;     // Span<byte>
		expected++;     // ReadOnlySpan<byte>
		expected++;     // Memory<byte>
		expected++;     // ReadOnlyMemory<byte>

		expected++;     // Stream
		expected++;     // TextReader

		expected *= 3;  // Words, Graphemes, Sentences

		return expected;
	}


	[Test]
	public void Overloads()
	{
		// no assertions, just needs to compile

		int expected = ExpectedOverloads();
		int got = 0;

		var input = "Hello, how are you?";
		var bytes = Encoding.UTF8.GetBytes(input);
		using var stream = new MemoryStream(bytes);
		using var reader = new StreamReader(stream);

		{
			// chars

			Split.Words(input); got++;

			var array = input.ToCharArray();
			Split.Words(array); got++;

			var span = new Span<char>(array);
			Split.Words(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Split.Words(rspan); got++;

			var mem = new Memory<char>(array);
			Split.Words(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Split.Words(rmem); got++;

			Split.Words(reader); got++;
		}


		{
			// chars

			Split.Graphemes(input); got++;

			var array = input.ToCharArray();
			Split.Graphemes(array); got++;

			var span = new Span<char>(array);
			Split.Graphemes(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Split.Graphemes(rspan); got++;

			var mem = new Memory<char>(array);
			Split.Graphemes(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Split.Graphemes(rmem); got++;

			Split.Graphemes(reader); got++;
		}


		{
			// chars

			Split.Sentences(input); got++;

			var array = input.ToCharArray();
			Split.Sentences(array); got++;

			var span = new Span<char>(array);
			Split.Sentences(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Split.Sentences(rspan); got++;

			var mem = new Memory<char>(array);
			Split.Sentences(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Split.Sentences(rmem); got++;

			Split.Sentences(reader); got++;
		}

		{
			// bytes

			Split.Words(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Split.Words(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Split.Words(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Split.Words(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Split.Words(rmem); got++;

			Split.Words(stream); got++;
		}


		{
			// bytes

			Split.Graphemes(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Split.Graphemes(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Split.Graphemes(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Split.Graphemes(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Split.Graphemes(rmem); got++;

			Split.Graphemes(stream); got++;
		}


		{
			// bytes

			Split.Sentences(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Split.Sentences(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Split.Sentences(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Split.Sentences(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Split.Sentences(rmem); got++;

			Split.Sentences(stream); got++;
		}

		Assert.That(got, Is.EqualTo(expected));
	}

	[Test]
	public void Enumerator()
	{
		var input = "Hello, how are you?";
		var mem = input.AsMemory();
		var bytes = Encoding.UTF8.GetBytes(input);
		Split.Words(mem);

		var tokens = Split.Words(input);
		var first = new List<string>();
		while (tokens.MoveNext())
		{
			var s = tokens.Current.ToString();
			first.Add(s);
		}
		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		var tokens2 = Split.Words(input);
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
		var tokens = Split.Words(example);
		var list = tokens.ToList();

		var i = 0;
		foreach (var token in tokens)
		{
			Assert.That(token.SequenceEqual(list[i]));
			i++;
		}

		Assert.That(list, Has.Count.EqualTo(i), "ToList should return the same number of tokens as iteration");

		// Enumerator should reset back to the beginning
		Assert.That(tokens.start, Is.EqualTo(0));
		Assert.That(tokens.end, Is.EqualTo(0));

		var threw = false;
		tokens.MoveNext();
		try
		{
			tokens.ToList();
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
		var tokens = Split.Words(example);
		var array = tokens.ToArray();

		var i = 0;
		foreach (var token in tokens)
		{
			Assert.That(token.SequenceEqual(array[i]));
			i++;
		}

		Assert.That(array, Has.Length.EqualTo(i), "ToArray should return the same number of tokens as iteration");

		// Tokenizer should reset back to the beginning
		Assert.That(tokens.start, Is.EqualTo(0));
		Assert.That(tokens.end, Is.EqualTo(0));

		var threw = false;
		tokens.MoveNext();
		try
		{
			tokens.ToArray();
		}
		catch (InvalidOperationException)
		{
			threw = true;
		}
		Assert.That(threw, Is.True, "Calling ToArray after iteration has begun should throw");
	}

	[Test]
	public void Position()
	{
		var example = "Hello, how are you?";

		{
			var tokens = Split.Words(example);
			tokens.MoveNext();
			Assert.That(tokens.Position, Is.EqualTo(0));
			tokens.MoveNext();
			Assert.That(tokens.Position, Is.EqualTo(5));
			tokens.MoveNext();
			Assert.That(tokens.Position, Is.EqualTo(6));

			tokens.Reset();
			var ranges = tokens.Ranges;
			foreach (var range in ranges)
			{
				tokens.MoveNext();
				Assert.That(tokens.Position, Is.EqualTo(range.Start.Value));
			}
		}

		var bytes = Encoding.UTF8.GetBytes(example);
		{
			var tokens = Split.Words(bytes);
			tokens.MoveNext();
			Assert.That(tokens.Position, Is.EqualTo(0));
			tokens.MoveNext();
			Assert.That(tokens.Position, Is.EqualTo(5));
			tokens.MoveNext();
			Assert.That(tokens.Position, Is.EqualTo(6));

			tokens.Reset();
			var ranges = tokens.Ranges;
			foreach (var range in ranges)
			{
				tokens.MoveNext();
				Assert.That(tokens.Position, Is.EqualTo(range.Start.Value));
			}
		}
	}

	[Test]
	public void OmitWhitespace()
	{
		// This is not exhaustive, but covers the basics

		var example = "Hello, \nhow\r are\tyou?\n";

		{
			// Options.None should be lossless
			var expected = example;
			var got = string.Concat(
				Split.Words(example, Options.None)
				.ToList()
				.SelectMany(c => c)
			);

			Assert.That(got, Is.EqualTo(expected));
		}

		{
			// Options.OmitWhitespace should have no whitespace
			var expected = new string(example.Where(c => !char.IsWhiteSpace(c)).ToArray());
			var got = string.Concat(
				Split.Words(example, Options.OmitWhitespace)
				.ToList()
				.SelectMany(c => c)
			);

			Assert.That(got, Is.EqualTo(expected));
		}
	}
}
