namespace uax29;

public static partial class Words
{
	public class Segmenter(byte[] data) : uax29.Segmenter(SplitFunc, data) { }
}
