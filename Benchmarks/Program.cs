using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<Benchmark>();

[MemoryDiagnoser]
public class Benchmark
{
	[GlobalSetup]
	public void Setup()
	{
	}

	// [Benchmark]
	// public void Dict()
	// {
	// 	int cat;
	// 	Words.dict.TryGetValue('א', out cat);
	// }

}
