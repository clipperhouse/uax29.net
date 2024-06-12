namespace Tests;

using uax29;
using NUnit.Framework.Internal;
using System.Linq;
using System.Text;

public class UnicodeTest(byte[] input, byte[][] expected, string comment)
{
	public readonly byte[] input = input;
	public readonly byte[][] expected = expected;
	public readonly string comment = comment;
}

[TestFixture]
public class TestUnicode
{

	[SetUp]
	public void Setup()
	{
	}

	static void TestTokenizerBytes(Tokenizer<byte> tokens, UnicodeTest test)
	{
		var i = 0;
		foreach (var token in tokens)
		{
			var got = token;
			var expected = test.expected[i];
			Assert.That(expected.AsSpan().SequenceEqual(got), $"{test.comment}");
			i++;
		}
	}
	static void TestTokenizerChars(Tokenizer<char> tokens, UnicodeTest test)
	{
		var i = 0;
		foreach (var token in tokens)
		{
			var got = token;
			var expected = test.expected[i];
			var s = Encoding.UTF8.GetString(expected).AsSpan();
			Assert.That(s.SequenceEqual(got), $"{test.comment}");
			i++;
		}
	}

	static readonly UnicodeTest[] WordsTests = UnicodeTests.Words;
	[Test, TestCaseSource(nameof(WordsTests))]
	public void WordsBytes(UnicodeTest test)
	{
		var tokens = Tokenizer.Create(test.input);
		TestTokenizerBytes(tokens, test);
	}
	[Test, TestCaseSource(nameof(WordsTests))]
	public void WordsString(UnicodeTest test)
	{
		var s = Encoding.UTF8.GetString(test.input);
		var tokens = Tokenizer.Create(s);
		TestTokenizerChars(tokens, test);
	}

	static readonly UnicodeTest[] SentencesTests = UnicodeTests.Sentences;
	[Test, TestCaseSource(nameof(SentencesTests))]
	public void SentencesBytes(UnicodeTest test)
	{
		var tokens = Tokenizer.Create(test.input, TokenType.Sentences);
		TestTokenizerBytes(tokens, test);
	}
	[Test, TestCaseSource(nameof(SentencesTests))]
	public void SentencesString(UnicodeTest test)
	{
		var s = Encoding.UTF8.GetString(test.input);
		var tokens = Tokenizer.Create(s, TokenType.Sentences);
		TestTokenizerChars(tokens, test);
	}

	static readonly UnicodeTest[] GraphemesTests = UnicodeTests.Graphemes;
	[Test, TestCaseSource(nameof(GraphemesTests))]
	public void GraphemesBytes(UnicodeTest test)
	{
		var tokens = Tokenizer.Create(test.input, TokenType.Graphemes);
		TestTokenizerBytes(tokens, test);
	}
	[Test, TestCaseSource(nameof(GraphemesTests))]
	public void GraphemesString(UnicodeTest test)
	{
		var s = Encoding.UTF8.GetString(test.input);
		var tokens = Tokenizer.Create(s, TokenType.Graphemes);
		TestTokenizerChars(tokens, test);
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
			foreach (var token in tokens)
			{
				results.AddRange(token);
			}

			Assert.That(results.SequenceEqual(invalidUtf8Bytes));
		}

		char[] invalidChars = [
			'\uDC00', // Low surrogate without a high surrogate
			'\uD800', // High surrogate without a low surrogate
            '\uFFFF', // Invalid Unicode character
            '\uD800', '\uD800', // Two high surrogates
            '\uDC00', '\uDC00', // Two low surrogates
		];

		foreach (TokenType tokenType in Enum.GetValues(typeof(TokenType)))
		{
			var results = new List<char>();
			var tokens = Tokenizer.Create(invalidChars, tokenType);
			foreach (var token in tokens)
			{
				results.AddRange(token);
			}

			Assert.That(results.SequenceEqual(invalidChars));
		}
	}
}
