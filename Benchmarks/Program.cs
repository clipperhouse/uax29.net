using System.Diagnostics;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using uax29;

var summary = BenchmarkRunner.Run<Benchmark>();

// var benchmark = new Benchmark();
// benchmark.Setup();
// var throughput = benchmark.Throughput();
// Console.WriteLine($"Throughput: {Math.Round(throughput, 1)} MB/s");

[MemoryDiagnoser]
public class Benchmark
{
	static byte[] sample = [];
	static string sampleStr = "";
	Stream sampleStream = Stream.Null;

	[GlobalSetup]
	public void Setup()
	{
		sample = File.ReadAllBytes("/Users/msherman/Documents/code/src/github.com/clipperhouse/uax29.net/Benchmarks/sample.txt");
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
	public void TokenizeString()
	{
		var tokens = Tokenizer.GetWords(sampleStr);
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

	public double Throughput()
	{
		const int runs = 1000;

		// warmup
		for (var i = 0; i < runs; i++)
		{
			var tokens = Tokenizer.GetWords(sample);
			foreach (var token in tokens)
			{

			}
		}
		Thread.Sleep(100);

		var stopwatch = Stopwatch.StartNew();
		double bytes = 0;

		for (var i = 0; i < runs; i++)
		{
			var tokens = Tokenizer.GetWords(sample);
			foreach (var token in tokens)
			{

			}
			bytes += sample.Length;
		}

		stopwatch.Stop();

		double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
		var megabytes = bytes / (1024.0 * 1024.0);

		return megabytes / elapsedSeconds;
	}
}
