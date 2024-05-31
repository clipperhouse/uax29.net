namespace uax29;

public static partial class Sentences
{
	public class Segmenter(byte[] data) : uax29.Segmenter(SplitFunc, data) { }
}
