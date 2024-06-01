using System.Buffers;
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
	private static byte[] sample = [];
	private static string sampleStr = "";

	[GlobalSetup]
	public void Setup()
	{
		sample = File.ReadAllBytes("/Users/msherman/Documents/code/src/github.com/clipperhouse/uax29.net/Benchmarks/sample.txt");
		sampleStr = Encoding.UTF8.GetString(sample);
	}

	[Benchmark]
	public void WordsExtension()
	{
		var words = sampleStr.TokenizeWords();
		foreach (var word in words)
		{
		}
	}

	[Benchmark]
	public void WordsTokenizer()
	{
		var tokens = new Tokenizer(sample);
		while (tokens.MoveNext())
		{
		}
	}

	[Benchmark]
	public double Throughput()
	{
		const int runs = 10;
		var stopwatch = Stopwatch.StartNew();
		double bytes = 0;

		for (var i = 0; i < runs; i++)
		{
			var tokens = new Tokenizer(sample);
			while (tokens.MoveNext())
			{

			}
			// var words = sample.TokenizeWords();
			// foreach (var word in words)
			// {

			// }
			bytes += sample.Length;
		}

		stopwatch.Stop();

		double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
		var megabytes = bytes / (1024.0 * 1024.0);

		return megabytes / elapsedSeconds;
	}
}
