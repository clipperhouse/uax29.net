namespace Tests;

using uax29;
using NUnit.Framework.Internal;

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
		var words = test.input.TokenizeWords();
		var i = 0;
		foreach (var got in words)
		{
			var expected = test.expected[i];
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
		var words = test.input.TokenizeGraphemes();
		var i = 0;
		foreach (var got in words)
		{
			var expected = test.expected[i];
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
		var words = test.input.TokenizeSentences();
		var i = 0;
		foreach (var got in words)
		{
			var expected = test.expected[i];
			Assert.That(expected.SequenceEqual(got), $@"{test.comment}
				input {test.input}
				expected {expected}
				got {got}
				");
			i++;
		}
	}
}
