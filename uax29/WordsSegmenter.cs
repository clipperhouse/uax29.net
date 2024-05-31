namespace uax29;

public static partial class Words
{
	public static Segmenter Segment(byte[] data)
	{
		return new Segmenter(data);
	}

	public class Segmenter : uax29.Segmenter
	{
		public Segmenter(byte[] data) : base(Words.SplitFunc, data) { }
	}
}
