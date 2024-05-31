namespace uax29;

public static class Extensions
{
	public static IEnumerable<byte[]> TokenizeWords(this byte[] data)
	{
		return new Words.Segmenter(data);
	}

	public static IEnumerable<byte[]> TokenizeWords(this string data)
	{
		return new Words.Segmenter(data);
	}

	public static IEnumerable<byte[]> TokenizeSentences(this byte[] data)
	{
		return new Sentences.Segmenter(data);
	}

	public static IEnumerable<byte[]> TokenizeSentences(this string data)
	{
		return new Sentences.Segmenter(data);
	}

	public static IEnumerable<byte[]> TokenizeGraphemes(this byte[] data)
	{
		return new Graphemes.Segmenter(data);
	}

	public static IEnumerable<byte[]> TokenizeGraphemes(this string data)
	{
		return new Graphemes.Segmenter(data);
	}
}