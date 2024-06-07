namespace Tests;

using uax29;
using NUnit.Framework.Internal;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

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

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		tokens.Reset();

		var second = new List<string>();
		while (tokens.MoveNext())
		{
			// do something with tokens.Current
			second.Add(tokens.Current.ToString());
		}

		Assert.That(first.SequenceEqual(second));
	}

	[Test]
	public void SetText()
	{
		var example = "Hello, how are you?";

		var tokens = Tokenizer.Create(example);

		var first = new List<string>();
		while (tokens.MoveNext())
		{
			first.Add(tokens.Current.ToString());
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		tokens.SetText(example);

		var second = new List<string>();
		while (tokens.MoveNext())
		{
			second.Add(tokens.Current.ToString());
		}

		Assert.That(first.SequenceEqual(second));
	}

	/// <summary>
	/// Ensure that streamed text and static text return identical results.
	/// </summary>
	[Test]
	public void StreamTest()
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

			while (streamTokens.MoveNext())
			{
				staticTokens.MoveNext();

				var staticCurrent = Encoding.UTF8.GetString(staticTokens.Current);
				var streamCurrent = Encoding.UTF8.GetString(streamTokens.Current);

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
	public void StreamReaderTest()
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

			while (streamTokens.MoveNext())
			{
				staticTokens.MoveNext();

				var staticCurrent = Encoding.UTF8.GetString(staticTokens.Current);
				var streamCurrent = streamTokens.Current.ToString();

				Assert.That(staticCurrent, Is.EqualTo(streamCurrent));
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
		while (tokens.MoveNext())
		{
			var s = Encoding.UTF8.GetString(tokens.Current);
			first.Add(s);
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		using var stream2 = new MemoryStream(bytes);

		tokens.SetStream(stream2);

		var second = new List<string>();
		while (tokens.MoveNext())
		{
			var s = Encoding.UTF8.GetString(tokens.Current);
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
		while (tokens.MoveNext())
		{
			var s = tokens.Current.ToString();
			first.Add(s);
		}

		Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

		using var stream2 = new MemoryStream(bytes);
		using var reader2 = new StreamReader(stream2);

		tokens.SetStream(reader2);

		var second = new List<string>();
		while (tokens.MoveNext())
		{
			var s = tokens.Current.ToString();
			second.Add(s);
		}

		Assert.That(first.SequenceEqual(second));
	}
}

