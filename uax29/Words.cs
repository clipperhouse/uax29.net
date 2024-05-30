namespace uax29;

using System.Reflection;
using Microsoft.VisualBasic;
using ProtoBuf;

public static class Words
{
	private static RuneTrie wordsTrie;
	private static bool isInitialized = false;
	private static readonly object lockObject = new object();

	public static RuneTrie Get()
	{
		if (!isInitialized)
		{
			lock (lockObject)
			{
				if (!isInitialized)
				{
					var assembly = Assembly.Load("uax29");
					using var stream = assembly.GetManifestResourceStream("uax29.words.proto.bin");
					wordsTrie = Serializer.Deserialize<RuneTrie>(stream);
				}
			}
		}

		return wordsTrie;
	}

	// SplitFunc splitFunc = (byte[] data, bool atEOF) =>
	// {

	// };

	//	SplitFunc(data []byte, atEOF bool) (advance int, token []byte, err error)
}
