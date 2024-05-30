using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using uax29;

var summary = BenchmarkRunner.Run<Benchmark>();

[MemoryDiagnoser]
public class Benchmark
{
	private Segmenter seg;
	private byte[] somewords = Encoding.UTF8.GetBytes("This is a test, with some number or words you know.");

	[GlobalSetup]
	public void Setup()
	{
		seg = new Segmenter(uax29.Words.SplitFunc, somewords);
		uax29.Words.dict.TryGetValue('א', out int val);
	}

	[Benchmark]
	public void Lookup()
	{
		var b = Encoding.UTF8.GetBytes("א");
		var p = uax29.Words.Lookup(b, out int w, out OperationStatus _);
	}

	[Benchmark]
	public void Dict()
	{
		uax29.Words.dict.TryGetValue('א', out int val);
	}

	[Benchmark]
	public void Words()
	{
		while (seg.Next())
		{
			seg.Bytes();
		}
	}

}
