namespace uax29;

public delegate (int advance, byte[] token) SplitFunc(byte[] data, bool atEOF);

public static class SplitFuncs
{
	public static readonly SplitFunc Whitespace = (byte[] data, bool atEOF) =>
	{
		var pos = 0;

		while (pos < data.Length)
		{
			if (data[pos] == ' ')
			{
				// consume the token without the whitespace
				var token = data[..pos];
				// consume the whitespace
				pos++;
				return (pos, token);
			}

			pos++;
		}

		return (pos, data[..pos]);
	};

}
