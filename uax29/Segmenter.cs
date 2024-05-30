namespace uax29;

public class Segmenter(SplitFunc split, byte[] data)
{
	readonly SplitFunc split = split;
	readonly byte[] data = data;
	byte[] token = [];
	int pos;

	public bool Next()
	{
		while (this.pos < this.data.Length)
		{
			var b = this.data[this.pos..];
			(var advance, var token) = this.split(this.data[this.pos..], true);
			this.pos += advance;
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
		return this.token;
	}

}


