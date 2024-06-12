namespace Tests;

using uax29;
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

	/// <summary>
	/// Ensure that streamed text and static text return identical results.
	/// </summary>
	[Test]
	public void Stream()
	{
		var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
		var examples = new List<string>()
		{
			example,											// smaller than the buffer
			string.Concat(Enumerable.Repeat(example, 999))		// larger than the buffer
		};

		foreach (var input in examples)
		{
			var bytes = Encoding.UTF8.GetBytes(input);
			var staticTokens = Tokenizer.Create(bytes);

			using var stream = new MemoryStream(bytes);
			var streamTokens = Tokenizer.Create(stream);

			foreach (var streamToken in streamTokens)
			{
				staticTokens.MoveNext();

				var staticCurrent = Encoding.UTF8.GetString(staticTokens.Current);
				var streamCurrent = Encoding.UTF8.GetString(streamToken);

				Assert.That(staticCurrent, Is.EqualTo(streamCurrent));
			}

			Assert.That(staticTokens.MoveNext(), Is.False, "Static tokens should have been consumed");
			Assert.That(streamTokens.MoveNext(), Is.False, "Stream tokens should have been consumed");
		}
	}

	/// <summary>
	/// Ensure that streamed text and static text return identical results.
	/// </summary>
	[Test]
	public void StreamReader()
	{
		var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
		var examples = new List<string>()
		{
			example,											// smaller than the buffer
			string.Concat(Enumerable.Repeat(example, 999))		// larger than the buffer
		};

		foreach (var input in examples)
		{
			var bytes = Encoding.UTF8.GetBytes(input);
			var staticTokens = Tokenizer.Create(bytes);

			using var stream = new MemoryStream(bytes);
			using var reader = new StreamReader(stream);
			var streamTokens = Tokenizer.Create(reader);

			foreach (var streamToken in streamTokens)
			{
				staticTokens.MoveNext();
				var staticCurrent = Encoding.UTF8.GetString(staticTokens.Current);

				Assert.That(staticCurrent, Is.EqualTo(streamToken.ToString()));
			}

			Assert.That(staticTokens.MoveNext(), Is.False, "Static tokens should have been consumed");
			Assert.That(streamTokens.MoveNext(), Is.False, "Stream tokens should have been consumed");
		}
	}

	[Test]
	public void SetStream()
	{
		var input = "Hello, how are you?";
		var bytes = Encoding.UTF8.GetBytes(input);
		using var stream = new MemoryStream(bytes);

		var tokens = Tokenizer.Create(stream);

		var first = new List<string>();
		foreach (var token in tokens)
		{
			var s = Encoding.UTF8.GetString(token);
			first.Add(s);
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		using var stream2 = new MemoryStream(bytes);

		tokens.SetStream(stream2);

		var second = new List<string>();
		foreach (var token in tokens)
		{
			var s = Encoding.UTF8.GetString(token);
			second.Add(s);
		}

		Assert.That(first.SequenceEqual(second));
	}

	[Test]
	public void SetStreamReader()
	{
		var input = "Hello, how are you?";
		var bytes = Encoding.UTF8.GetBytes(input);
		using var stream = new MemoryStream(bytes);
		using var reader = new StreamReader(stream);

		var tokens = Tokenizer.Create(reader);

		var first = new List<string>();
		foreach (var token in tokens)
		{
			var s = token.ToString();
			first.Add(s);
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		using var stream2 = new MemoryStream(bytes);
		using var reader2 = new StreamReader(stream2);

		tokens.SetStream(reader2);

		var second = new List<string>();
		foreach (var token in tokens)
		{
			var s = token.ToString();
			second.Add(s);
		}

		Assert.That(first.SequenceEqual(second));
	}

	[Test]
	public void Enumerator()
	{
		var input = "Hello, how are you?";

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
	public void StreamEnumerator()
	{
		var input = "Hello, how are you?";
		var bytes = Encoding.UTF8.GetBytes(input);
		using var stream = new MemoryStream(bytes);

		var tokens = Tokenizer.Create(stream);

		var first = new List<string>();
		while (tokens.MoveNext())
		{
			var s = tokens.Current.ToString();
			first.Add(s);
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		using var stream2 = new MemoryStream(bytes);
		var tokens2 = Tokenizer.Create(stream2);

		var second = new List<string>();
		foreach (var token in tokens2)
		{
			var s = token.ToString();
			second.Add(s);
		}
		Assert.That(first.SequenceEqual(second));
	}
}

