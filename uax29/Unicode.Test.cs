namespace Tests;

using UAX29;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

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

	internal static void TestTokenizerBytes(Tokenizer<byte> tokens, UnicodeTest test)
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

	internal static void TestTokenizerChars(Tokenizer<char> tokens, UnicodeTest test)
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

	private delegate Tokenizer<byte> ByteMethod(byte[] input);
	static readonly ByteMethod[] byteMethods = [Tokenizer.GetWords, Tokenizer.GetGraphemes, Tokenizer.GetSentences];

	private delegate Tokenizer<char> CharMethod(char[] input);
	static readonly CharMethod[] charMethods = [Tokenizer.GetWords, Tokenizer.GetGraphemes, Tokenizer.GetSentences];

	[Test]
	public void InvalidEncoding()
	{
		// All bytes and char that go into the tokenizer should come back out, even if invalid

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

		foreach (var method in byteMethods)
		{
			var results = new List<byte>();
			var tokens = method(invalidUtf8Bytes);
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

		foreach (var method in charMethods)
		{
			var results = new List<char>();
			var tokens = method(invalidChars);
			foreach (var token in tokens)
			{
				results.AddRange(token);
			}

			Assert.That(results.SequenceEqual(invalidChars));
		}

		using var rng = RandomNumberGenerator.Create();
		foreach (var i in Enumerable.Range(1, 100))
		{
			var bytes = new byte[i];
			rng.GetBytes(bytes);
			var s = Encoding.UTF8.GetString(bytes);

			foreach (var method in byteMethods)
			{
				{
					var tokens = method(bytes);

					var results = new List<byte>();
					foreach (var token in tokens)
					{
						results.AddRange(token);
					}
					Assert.That(results.SequenceEqual(bytes));
				}
			}

			foreach (var method in charMethods)
			{
				var tokens = method(s.ToCharArray());

				var results = new List<char>();
				foreach (var token in tokens)
				{
					results.AddRange(token);
				}
				Assert.That(results.SequenceEqual(s));
			}
		}
	}
}
