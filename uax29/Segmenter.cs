using System.Collections;
using System.Text;

namespace uax29;

public class Segmenter(SplitFunc split, byte[] data) : IEnumerable<byte[]>
{
	readonly SplitFunc split = split;
	byte[] data = data;
	int pos;
	int start;
	int end;

	public IEnumerator<byte[]> GetEnumerator()
	{
		while (Next())
		{
			yield return Bytes();
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void SetText(byte[] data)
	{
		this.data = data;
		this.start = 0;
		this.end = 0;
	}

	public bool Next()
	{
		while (pos < data.Length)
		{
			var b = data.AsSpan()[pos..];
			var advance = split(b, true);
			this.start = pos;
			this.end = pos + advance;
			pos += advance;

			// Interpret as EOF
			if (advance == 0)
			{
				return false;
			}

			return true;
		}
		return false;
	}

	public byte[] Bytes()
	{
		return this.data[this.start..this.end];
	}

	public override string ToString()
	{
		return Encoding.UTF8.GetString(Bytes());
	}
}


