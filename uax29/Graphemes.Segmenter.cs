namespace uax29;

public static partial class Graphemes
{
	public class Segmenter(byte[] data) : uax29.Segmenter(SplitFunc, data) { }
}
