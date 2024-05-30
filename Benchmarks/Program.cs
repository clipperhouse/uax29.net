using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using uax29;

var summary = BenchmarkRunner.Run<ByteTrieBenchmark>();

[MemoryDiagnoser]
public class ByteTrieBenchmark
{
	private RuneTrie trie;

	[GlobalSetup]
	public void Setup()
	{
		// trie = Words.Get();
	}

	// [Benchmark]
	// public (bool found, int payload, int length) Search() => trie.Search("א");

	[Benchmark]
	public void Deserialize() => Words.Get();

	// [Benchmark]
	// public void Insert()
	// {
	//     trie.Insert("apple", 100);
	//     trie.Insert("banana", 100);
	//     trie.Insert("application", 100);
	//     trie.Insert("casaba", 100);
	//     trie.Insert("date", 100);
	//     trie.Insert("elderberry", 100);
	//     trie.Insert("fig", 100);
	//     trie.Insert("guava", 100);
	// }
}
