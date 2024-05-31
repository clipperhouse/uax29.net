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

	static readonly UnicodeTest[] wordTests = UnicodeTests.Words;
	[Test, TestCaseSource(nameof(wordTests))]
	public void Words(UnicodeTest test)
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

	static readonly UnicodeTest[] graphemeTests = UnicodeTests.Graphemes;
	[Test, TestCaseSource(nameof(graphemeTests))]
	public void Graphemes(UnicodeTest test)
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

	static readonly UnicodeTest[] sentenceTests = UnicodeTests.Sentences;
	[Test, TestCaseSource(nameof(sentenceTests))]
	public void Sentences(UnicodeTest test)
	{
		var seg = new Sentences.Segmenter(test.input);
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
