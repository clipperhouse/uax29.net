using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using uax29;

var summary = BenchmarkRunner.Run<Benchmark>();

[MemoryDiagnoser]
public class Benchmark
{
	private readonly Segmenter segmenter = new Words.Segmenter(words);
	private static readonly byte[] words = Encoding.UTF8.GetBytes("This is a test, with some number or words you know.");

	[GlobalSetup]
	public void Setup()
	{
	}

	[Benchmark]
	public void Words()
	{
		while (segmenter.Next())
		{
			segmenter.Bytes();
		}
	}
}
