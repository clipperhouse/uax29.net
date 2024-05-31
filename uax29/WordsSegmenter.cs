namespace uax29;

public static partial class Words
{
	public static Segmenter Segment(byte[] data)
	{
		return new Segmenter(data);
	}

	public class Segmenter(byte[] data) : uax29.Segmenter(SplitFunc, data) { }
}
