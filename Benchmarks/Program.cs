using System.Buffers;
using System.Diagnostics;
using System.Globalization;
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
	private static byte[] sample = [];
	static string sampleStr = "";

	private static TokenType tokenType = TokenType.Words;

	[GlobalSetup]
	public void Setup()
	{
		sample = File.ReadAllBytes("/Users/msherman/Documents/code/src/github.com/clipperhouse/uax29.net/Benchmarks/sample.txt");
		sampleStr = Encoding.UTF8.GetString(sample);
	}

	[Benchmark]
	public void Tokenizer()
	{
		var tokens = new Tokenizer(sample, tokenType);
		while (tokens.MoveNext())
		{
		}
	}

	//	[Benchmark]
	public void StringInfo()
	{
		var enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(sampleStr);
		while (enumerator.MoveNext())
		{
		}
	}

	//	[Benchmark]
	public void Graphemes()
	{
		var tokens = new Tokenizer(sample, TokenType.Graphemes);
		while (tokens.MoveNext())
		{
		}
	}

	public double Throughput()
	{
		const int runs = 1000;

		// warmup
		for (var i = 0; i < runs; i++)
		{
			var tokens = new Tokenizer(sample, tokenType);
			while (tokens.MoveNext())
			{

			}
		}
		Thread.Sleep(100);

		var stopwatch = Stopwatch.StartNew();
		double bytes = 0;

		for (var i = 0; i < runs; i++)
		{
			var tokens = new Tokenizer(sample, tokenType);
			while (tokens.MoveNext())
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
