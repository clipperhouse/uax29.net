using System.Collections;
using System.Text;

namespace uax29;

public class Segmenter(SplitFunc split, byte[] data) : IEnumerable<byte[]>
{
	readonly SplitFunc split = split;
	readonly byte[] data = data;
	byte[] token = [];
	int pos;

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

	public bool Next()
	{
		while (pos < data.Length)
		{
			var b = data[pos..];
			(var advance, var token) = split(data[pos..], true);
			pos += advance;
			this.token = token;

			// Interpret as EOF
			if (advance == 0)
			{
				return false;
			}
			// Interpret as EOF
			if (this.token.Length == 0)
			{
				return false;
			}

			return true;
		}
		return false;
	}

	public byte[] Bytes()
	{
		return token;
	}

	public override string ToString()
	{
		return Encoding.UTF8.GetString(token);
	}
}


