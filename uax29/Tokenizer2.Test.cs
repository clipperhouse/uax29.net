namespace Tests;

using UAX29;
using System.Linq;
using System.Text;

[TestFixture]
public class TestTokenizer2
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

		var tokens = Tokenizer2.GetWords(example);

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

		var tokens = Tokenizer2.GetWords(example);

		var first = new List<string>();
		foreach (var token in tokens)
		{
			first.Add(token.ToString());
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		var runes = RuneTokenizer.Create(example);
		tokens.SetText(runes);

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

			Tokenizer2.GetWords(input); got++;

			var array = input.ToCharArray();
			Tokenizer2.GetWords(array); got++;

			var span = new Span<char>(array);
			Tokenizer2.GetWords(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Tokenizer2.GetWords(rspan); got++;

			var mem = new Memory<char>(array);
			Tokenizer2.GetWords(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Tokenizer2.GetWords(rmem); got++;

			Tokenizer2.GetWords(reader); got++;
		}


		{
			// chars

			Tokenizer2.GetGraphemes(input); got++;

			var array = input.ToCharArray();
			Tokenizer2.GetGraphemes(array); got++;

			var span = new Span<char>(array);
			Tokenizer2.GetGraphemes(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Tokenizer2.GetGraphemes(rspan); got++;

			var mem = new Memory<char>(array);
			Tokenizer2.GetGraphemes(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Tokenizer2.GetGraphemes(rmem); got++;

			Tokenizer2.GetGraphemes(reader); got++;
		}


		{
			// chars

			Tokenizer2.GetSentences(input); got++;

			var array = input.ToCharArray();
			Tokenizer2.GetSentences(array); got++;

			var span = new Span<char>(array);
			Tokenizer2.GetSentences(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			Tokenizer2.GetSentences(rspan); got++;

			var mem = new Memory<char>(array);
			Tokenizer2.GetSentences(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			Tokenizer2.GetSentences(rmem); got++;

			Tokenizer2.GetSentences(reader); got++;
		}

		{
			// bytes

			Tokenizer2.GetWords(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Tokenizer2.GetWords(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Tokenizer2.GetWords(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Tokenizer2.GetWords(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Tokenizer2.GetWords(rmem); got++;

			Tokenizer2.GetWords(stream); got++;
		}


		{
			// bytes

			Tokenizer2.GetGraphemes(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Tokenizer2.GetGraphemes(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Tokenizer2.GetGraphemes(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Tokenizer2.GetGraphemes(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Tokenizer2.GetGraphemes(rmem); got++;

			Tokenizer2.GetGraphemes(stream); got++;
		}


		{
			// bytes

			Tokenizer2.GetSentences(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			Tokenizer2.GetSentences(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			Tokenizer2.GetSentences(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			Tokenizer2.GetSentences(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			Tokenizer2.GetSentences(rmem); got++;

			Tokenizer2.GetSentences(stream); got++;
		}

		Assert.That(got, Is.EqualTo(expected));
	}

	[Test]
	public void Enumerator()
	{
		var input = "Hello, how are you?";
		var mem = input.AsMemory();
		var bytes = Encoding.UTF8.GetBytes(input);
		Tokenizer2.GetWords(mem);

		var tokens = Tokenizer2.GetWords(input);
		var first = new List<string>();
		while (tokens.MoveNext())
		{
			var s = tokens.Current.ToString();
			first.Add(s);
		}
		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing		

		var tokens2 = Tokenizer2.GetWords(input);
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
		var tokens = Tokenizer2.GetWords(example);
		var list = tokens.ToList();

		var i = 0;
		foreach (var token in tokens)
		{
			Assert.That(token.SequenceEqual(list[i]));
			i++;
		}

		Assert.That(list, Has.Count.EqualTo(i), "ToList should return the same number of tokens as iteration");

		// Tokenizer2 should reset back to the beginning
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
		var tokens = Tokenizer2.GetWords(example);
		var array = tokens.ToArray();

		var i = 0;
		foreach (var token in tokens)
		{
			Assert.That(token.SequenceEqual(array[i]));
			i++;
		}

		Assert.That(array, Has.Length.EqualTo(i), "ToArray should return the same number of tokens as iteration");

		// Tokenizer2 should reset back to the beginning
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
}
