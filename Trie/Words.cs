namespace Trie;

using System.Reflection;
using ProtoBuf;

public static class Words
{
	private static ByteTrie wordsTrie;
	private static bool isInitialized = false;
	private static readonly object lockObject = new object();

	public static ByteTrie Get()
	{
		if (!isInitialized)
		{
			lock (lockObject)
			{
				if (!isInitialized)
				{
					var assembly = Assembly.Load("Trie");
					using var stream = assembly.GetManifestResourceStream("Trie.words.proto.bin");
					wordsTrie = Serializer.Deserialize<ByteTrie>(stream);
				}
			}
		}

		return wordsTrie;
	}
}
