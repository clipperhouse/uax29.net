namespace uax29;

public class Segmenter(SplitFunc split, byte[] data)
{
	readonly SplitFunc split = split;
	readonly byte[] data = data;
	byte[] token = [];
	int pos;

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
}


