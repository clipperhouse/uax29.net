namespace Tests;

using uax29;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;

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

		var tokens = Tokenizer.Create(example);

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

		expected *= 2;  // One regular call, one extension call

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

			input.GetWords(); got++;
			Tokenizer.GetWords(input); got++;

			var array = input.ToCharArray();
			array.GetWords(); got++;
			Tokenizer.GetWords(array); got++;

			var span = new Span<char>(array);
			span.GetWords(); got++;
			Tokenizer.GetWords(span); got++;

			ReadOnlySpan<char> rspan = input.AsSpan();
			rspan.GetWords(); got++;
			Tokenizer.GetWords(rspan); got++;

			var mem = new Memory<char>(array);
			mem.GetWords(); got++;
			Tokenizer.GetWords(mem); got++;

			ReadOnlyMemory<char> rmem = input.AsMemory();
			rmem.GetWords(); got++;
			Tokenizer.GetWords(rmem); got++;

			reader.GetWords(); got++;
			Tokenizer.GetWords(reader); got++;
		}

		{
			// bytes

			bytes.GetWords(); got++;
			Tokenizer.GetWords(bytes); got++;

			Span<byte> span = bytes.AsSpan();
			span.GetWords(); got++;
			Tokenizer.GetWords(span); got++;

			ReadOnlySpan<byte> rspan = bytes.AsSpan();
			rspan.GetWords(); got++;
			Tokenizer.GetWords(rspan); got++;

			Memory<byte> mem = bytes.AsMemory();
			mem.GetWords(); got++;
			Tokenizer.GetWords(mem); got++;

			ReadOnlyMemory<byte> rmem = bytes.AsMemory();
			rmem.GetWords(); got++;
			Tokenizer.GetWords(rmem); got++;

			stream.GetWords(); got++;
			Tokenizer.GetWords(stream); got++;
		}

		Assert.That(got, Is.EqualTo(expected));
	}

	[Test]
	public void Enumerator()
	{
		var input = "Hello, how are you?";
		var mem = input.AsMemory();
		var bytes = Encoding.UTF8.GetBytes(input);
		mem.GetWords();

		var tokens = Tokenizer.Create(input);
		var first = new List<string>();
		while (tokens.MoveNext())
		{
			var s = tokens.Current.ToString();
			first.Add(s);
		}
		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing		

		var tokens2 = Tokenizer.Create(input);
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
		var tokens = Tokenizer.Create(example);
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
		var tokens = Tokenizer.Create(example);
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
}
