using System.Text;

namespace uax29;

public static partial class Words
{
	public partial class Segmenter : uax29.Segmenter
	{
		public Segmenter(byte[] data) : base(SplitFunc, data) { }

		public Segmenter(string data) : this(Encoding.UTF8.GetBytes(data)) { }
	}
}
