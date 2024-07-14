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
	public void TokenizeBytes()
	{
		var tokens = Tokenizer.GetWords(sample);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void TokenizeBytesOmitWhitespace()
	{
		var tokens = Tokenizer.GetWords(sample, Options.OmitWhitespace);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void TokenizeString()
	{
		var tokens = Tokenizer.GetWords(sampleStr);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void TokenizeStringOmitWhitespace()
	{
		var tokens = Tokenizer.GetWords(sampleStr, Options.OmitWhitespace);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void TokenizeStream()
	{
		var stream = new MemoryStream(sample);
		var tokens = Tokenizer.GetWords(stream);
		foreach (var token in tokens)
		{
		}
	}

	[Benchmark]
	public void TokenizeSetStream()
	{
		// This is to test to observe allocations.

		// The creation will allocate a buffer of 1024 bytes
		var tokens = Tokenizer.GetWords(sampleStream);

		var runs = 10;
		// keep in mind the 10 runs when interpreting the benchmark
		for (var i = 0; i < runs; i++)
		{
			// subsequent runs should allocate less by using SetStream
			sampleStream.Seek(0, SeekOrigin.Begin);
			tokens.SetStream(sampleStream);
			foreach (var token in tokens)
			{
			}
		}
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
	public void TokenizerGraphemes()
	{
		var tokens = Tokenizer.GetGraphemes(sample);
		foreach (var token in tokens)
		{
		}
	}
}
