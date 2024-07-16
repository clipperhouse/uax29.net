using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using UAX29;

BenchmarkRunner.Run<Benchmark>();

[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 3)]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class Benchmark
{
	private class Config : ManualConfig
	{
		public Config()
		{
			AddColumn(new Speed());
			// AddDiagnoser(new EventPipeProfiler(EventPipeProfile.CpuSampling));
			// You can also use other profilers like:
			// AddDiagnoser(new EtwProfiler());
			// AddDiagnoser(new PerfCollectProfiler()); // for Linux
		}
	}

	static byte[] sample = [];
	static string sampleStr = "";
	Stream sampleStream = Stream.Null;

	public string FileName = "sample.txt";

	[GlobalSetup]
	public void Setup()
	{
		sample = File.ReadAllBytes("sample.txt");
		sampleStr = Encoding.UTF8.GetString(sample);
		sampleStream = new MemoryStream(sample);
	}

	[Benchmark]
	public void SplitBytes()
	{
		var tokens = Split.Words(sample);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void SplitBytesOmitWhitespace()
	{
		var tokens = Split.Words(sample, Options.OmitWhitespace);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void SplitString()
	{
		var tokens = Split.Words(sampleStr);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void SplitStringOmitWhitespace()
	{
		var tokens = Split.Words(sampleStr, Options.OmitWhitespace);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void SplitStream()
	{
		sampleStream.Seek(0, SeekOrigin.Begin);
		var tokens = Split.Words(sampleStream);
		foreach (var token in tokens) { }
	}

	static readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;

	[Benchmark]
	public void SplitStreamArrayPool()
	{
		var storage = pool.Rent(2048);

		sampleStream.Seek(0, SeekOrigin.Begin);
		var tokens = Split.Words(sampleStream, minBufferBytes: 1024, bufferStorage: storage);
		tokens.SetStream(sampleStream);
		foreach (var token in tokens) { }

		pool.Return(storage);
	}

	[Benchmark]
	public void StringInfoGraphemes()
	{
		var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(sampleStr);
		while (enumerator.MoveNext())
		{
		}
	}

	[Benchmark]
	public void SplitGraphemes()
	{
		var tokens = Split.Graphemes(sample);
		foreach (var token in tokens)
		{
		}
	}
}
