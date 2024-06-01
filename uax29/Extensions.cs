using System.Text;

namespace uax29;

public static class Extensions
{
	static IEnumerable<byte[]> Tokenize(SplitFunc split, byte[] data)
	{
		int pos = 0;
		int start = 0;
		int end = 0;

		while (pos < data.Length)
		{
			var b = data.AsSpan()[pos..];
			var advance = split(b, true);
			// Interpret as EOF
			if (advance == 0)
			{
				break;
			}

			start = pos;
			end = pos + advance;
			pos += advance;

			yield return data[start..end];
		}
	}

	static IEnumerable<byte[]> Tokenize(SplitFunc split, string data)
	{
		var d = Encoding.UTF8.GetBytes(data);
		return Tokenize(split, d);
	}

	public static IEnumerable<byte[]> TokenizeWords(this string data)
	{
		return Tokenize(Words.SplitFunc, data);
	}

	public static IEnumerable<byte[]> TokenizeSentences(this string data)
	{
		return Tokenize(Sentences.SplitFunc, data);
	}

	public static IEnumerable<byte[]> TokenizeGraphemes(this string data)
	{
		return Tokenize(Graphemes.SplitFunc, data);
	}
}