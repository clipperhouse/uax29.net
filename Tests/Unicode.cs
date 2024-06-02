namespace Tests;

using uax29;
using NUnit.Framework.Internal;
using System.Linq;

[TestFixture]
public class Unicode
{

	[SetUp]
	public void Setup()
	{
	}

	static void TestTokenizer(Tokenizer tokens, UnicodeTest test)
	{
		var i = 0;
		while (tokens.MoveNext())
		{
			var got = tokens.Current;
			var expected = test.expected[i];
			Assert.That(expected.AsSpan().SequenceEqual(got), $@"{test.comment}
				input {test.input}
				expected {expected}
				got {got.ToArray()}
				");
			i++;
		}
	}

	static readonly UnicodeTest[] WordsTests = UnicodeTests.Words;
	[Test, TestCaseSource(nameof(WordsTests))]
	public void WordsTokenizer(UnicodeTest test)
	{
		var tokens = new Tokenizer(test.input);
		TestTokenizer(tokens, test);
	}

	static readonly UnicodeTest[] SentencesTests = UnicodeTests.Sentences;
	[Test, TestCaseSource(nameof(SentencesTests))]
	public void SentencesTokenizer(UnicodeTest test)
	{
		var tokens = new Tokenizer(test.input, TokenType.Sentences);
		TestTokenizer(tokens, test);
	}

	static readonly UnicodeTest[] GraphemesTests = UnicodeTests.Graphemes;
	[Test, TestCaseSource(nameof(GraphemesTests))]
	public void GraphemesTokenizer(UnicodeTest test)
	{
		var tokens = new Tokenizer(test.input, TokenType.Graphemes);
		TestTokenizer(tokens, test);
	}
}
