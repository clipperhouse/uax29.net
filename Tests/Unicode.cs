namespace Tests;

using uax29;
using NUnit.Framework.Internal;
using System.Linq;
using System.Text;

[TestFixture]
public class Unicode
{

	[SetUp]
	public void Setup()
	{
	}

	static void TestTokenizer(Tokenizer<byte> tokens, UnicodeTest test)
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
		var tokens = Tokenizer.Create(test.input);
		TestTokenizer(tokens, test);
	}

	[Test]
	public void InvalidUTF8()
	{
		byte[] invalidUtf8Bytes =
		[
			0xE2, 0x28, 0xA1, 					// Invalid sequence: 0xE2 should be followed by two bytes in the range 0x80 to 0xBF
            (byte)'a', (byte)'b', (byte)'c',
			0xF0, 0x90, 0x28, 0xBC, 			// Invalid sequence: 0x90 should be in the range 0x80 to 0xBF when preceded by 0xF0
            0xC3, 0x28, 						// Invalid sequence: 0xC3 should be followed by a byte in the range 0x80 to 0xBF
			..Encoding.UTF8.GetBytes("你好，世界"),
			0x61, 								// Valid ASCII character 'a'
            0xE1, 0x80, 						// Incomplete sequence
            (byte)'d', (byte)'e', (byte)'f',
		];

		foreach (TokenType tokenType in Enum.GetValues(typeof(TokenType)))
		{
			var results = new List<byte>();
			var tokens = Tokenizer.Create(invalidUtf8Bytes, tokenType);
			while (tokens.MoveNext())
			{
				results.AddRange(tokens.Current);
			}

			Assert.That(results.SequenceEqual(invalidUtf8Bytes));
		}
	}

	static readonly UnicodeTest[] SentencesTests = UnicodeTests.Sentences;
	[Test, TestCaseSource(nameof(SentencesTests))]
	public void SentencesTokenizer(UnicodeTest test)
	{
		var tokens = Tokenizer.Create(test.input, TokenType.Sentences);
		TestTokenizer(tokens, test);
	}

	static readonly UnicodeTest[] GraphemesTests = UnicodeTests.Graphemes;
	[Test, TestCaseSource(nameof(GraphemesTests))]
	public void GraphemesTokenizer(UnicodeTest test)
	{
		var tokens = Tokenizer.Create(test.input, TokenType.Graphemes);
		TestTokenizer(tokens, test);
	}
}
