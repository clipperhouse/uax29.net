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

	static ulong Concat(uint upper, uint lower)
	{
		ulong concatenated = ((ulong)upper << 32) | ((ulong)lower & 0xFFFFFFFF);
		return concatenated;
	}


	static bool Matches(ulong currentLast, ulong properties)
	{
		var current = currentLast >> 32;
		var last = currentLast & 0x00000000FFFFFFFF;
		var propCurrent = properties >> 32;
		var propLast = properties & 0x0000000000000000FFFFFFFFFFFFFFFF;
		var matches = ((current & propCurrent) + (last & propLast)) > 1;
		return matches;
	}

	static string BitString(ulong val)
	{
		return Convert.ToString((long)val, 2).PadLeft(64, '0');
	}

	[Test]
	public void Concatenate()
	{
		uint upper = 0b_1100110011001100_1100110011001100;
		uint lower = 0b_0011001100110011_0011001100110011;

		ulong concatenated = Concat(upper, lower);

		var got = BitString(concatenated);

		var expected = "11001100110011001100110011001100" + "00110011001100110011001100110011";
		Assert.That(got == expected);
	}

	[Test]
	public void LongMatch()
	{
		const uint current = 2;
		const uint last = 4;
		var currentLast = Concat(current, last);

		var current2 = currentLast >> 32;
		var last2 = currentLast & 0x0000_0000_FFFF_FFFF;

		Assert.That(current == current2);
		Assert.That((current & current2) == current);
		Assert.That(last == last2);
		Assert.That((last & last2) == last);

		const uint currentProp = 2;
		const uint lastProp = 4;

		var bothProps = Concat(currentProp, lastProp);
		var currentProp2 = bothProps >> 32;
		var lastProp2 = bothProps & 0x0000_0000_FFFF_FFFF;

		Assert.That(currentProp == currentProp2);
		Assert.That((currentProp & currentProp2) == currentProp);
		Assert.That(lastProp == lastProp2);
		Assert.That((lastProp & lastProp2) == lastProp);

		Assert.That((current & currentProp) == currentProp);
		Assert.That((last & lastProp) == lastProp);


		// var matches = ((current & propCurrent) + (last & propLast)) > 1;
		// return matches;

	}

	[Test]
	public void MatchAgain()
	{
		const uint current = 2;
		const uint last = 4;

		const uint currentProp = 2;
		const uint lastProp = 4;
		var concat = Concat(current, last);
		var concatProp = Concat(currentProp, lastProp);
		var ored = concat | concatProp;

		var upper = ored >> 32;
		var lower = ored & 0x00000000FFFFFFFF;

		var upperLowerSum = upper + lower;
		var propSum = currentProp + lastProp;

		// var matches = (upper + lower) >= (currentProp + lastProp);
	}
}
