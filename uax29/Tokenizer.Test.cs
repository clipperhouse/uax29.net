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

			Tokenizer.GetWords(input); got++;

			var array = input.ToCharArray();
			Tokenizer.GetWords(array); got++;

			var span = new Span<char>(array);
			Tokenizer.GetWords(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Tokenizer.GetWords(rspan); got++;

			var mem = new Memory<char>(array);
			Tokenizer.GetWords(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Tokenizer.GetWords(rmem); got++;

			Tokenizer.GetWords(reader); got++;
		}


		{
			// chars

			Tokenizer.GetGraphemes(input); got++;

			var array = input.ToCharArray();
			Tokenizer.GetGraphemes(array); got++;

			var span = new Span<char>(array);
			Tokenizer.GetGraphemes(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Tokenizer.GetGraphemes(rspan); got++;

			var mem = new Memory<char>(array);
			Tokenizer.GetGraphemes(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Tokenizer.GetGraphemes(rmem); got++;

			Tokenizer.GetGraphemes(reader); got++;
		}


		{
			// chars

			Tokenizer.GetSentences(input); got++;

			var array = input.ToCharArray();
			Tokenizer.GetSentences(array); got++;

			var span = new Span<char>(array);
			Tokenizer.GetSentences(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Tokenizer.GetSentences(rspan); got++;

			var mem = new Memory<char>(array);
			Tokenizer.GetSentences(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Tokenizer.GetSentences(rmem); got++;

			Tokenizer.GetSentences(reader); got++;
		}

		{
			// bytes

			Tokenizer.GetWords(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Tokenizer.GetWords(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Tokenizer.GetWords(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Tokenizer.GetWords(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Tokenizer.GetWords(rmem); got++;

			Tokenizer.GetWords(stream); got++;
		}


		{
			// bytes

			Tokenizer.GetGraphemes(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Tokenizer.GetGraphemes(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Tokenizer.GetGraphemes(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Tokenizer.GetGraphemes(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Tokenizer.GetGraphemes(rmem); got++;

			Tokenizer.GetGraphemes(stream); got++;
		}


		{
			// bytes

			Tokenizer.GetSentences(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Tokenizer.GetSentences(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Tokenizer.GetSentences(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Tokenizer.GetSentences(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Tokenizer.GetSentences(rmem); got++;

			Tokenizer.GetSentences(stream); got++;
		}

		Assert.That(got, Is.EqualTo(expected));
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
		var tokens = Tokenizer.GetWords(example);
		var list = tokens.ToList();

		var i = 0;
		foreach (var token in tokens)
		{
			Assert.That(token.SequenceEqual(list[i]));
			i++;
		}

		Assert.That(list, Has.Count.EqualTo(i), "ToList should return the same number of tokens as iteration");

		// Tokenizer should reset back to the beginning
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
		var tokens = Tokenizer.GetWords(example);
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
			var tokens = Tokenizer.GetWords(example);
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
			var tokens = Tokenizer.GetWords(bytes);
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
				Tokenizer.GetWords(example, Options.None)
				.ToList()
				.SelectMany(c => c)
			);

			Assert.That(got, Is.EqualTo(expected));
		}

		{
			// Options.OmitWhitespace should have no whitespace
			var expected = new string(example.Where(c => !char.IsWhiteSpace(c)).ToArray());
			var got = string.Concat(
				Tokenizer.GetWords(example, Options.OmitWhitespace)
				.ToList()
				.SelectMany(c => c)
			);

			Assert.That(got, Is.EqualTo(expected));
		}
	}
}
