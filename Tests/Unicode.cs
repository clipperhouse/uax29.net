namespace Tests;

using uax29;
using NUnit.Framework.Internal;
using System.Text;

[TestFixture]
public class Unicode
{

	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Segmenter()
	{
		var s = Encoding.UTF8.GetBytes("This is a test, with some number or words you know.");
		var seg = new Words.Segmenter(s);

		while (seg.Next())
		{
			var word = Encoding.UTF8.GetString(seg.Bytes());
			Console.WriteLine(word);
		}
	}

	[Test]
	public void Words()
	{
		foreach (var test in UnicodeTests.Words)
		{
			var seg = new Words.Segmenter(test.input);
			var i = 0;
			while (seg.Next())
			{
				var expected = test.expected[i];
				var got = seg.Bytes();
				Assert.That(expected.SequenceEqual(got), $@"{test.comment}
				input {test.input}
				expected {expected}
				got {got}
				");
				i++;
			}
		}
	}

	[Test]
	public void Graphemes()
	{
		foreach (var test in UnicodeTests.Graphemes)
		{
			var seg = new Graphemes.Segmenter(test.input);
			var i = 0;
			while (seg.Next())
			{
				var expected = test.expected[i];
				var got = seg.Bytes();
				Assert.That(expected.SequenceEqual(got), $@"{test.comment}
				input {test.input}
				expected {expected}
				got {got}
				");
				i++;
			}
		}
	}
	[Test]
	public void Sentences()
	{
		foreach (var test in UnicodeTests.Sentences)
		{
			var seg = new Sentences.Segmenter(test.input);
			var i = 0;
			while (seg.Next())
			{
				var expected = test.expected[i];
				var got = seg.Bytes();
				Console.WriteLine($"testing '{test.comment}'");
				Assert.That(expected.SequenceEqual(got), $@"{test.comment}
				input {test.input}
				expected {expected}
				got {got}
				");
				i++;
			}
		}
	}
}
