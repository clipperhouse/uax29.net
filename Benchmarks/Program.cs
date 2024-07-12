using System.Buffers;
using System.Diagnostics;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using UAX29;


var summary = BenchmarkRunner.Run<Benchmark>();

// var benchmark = new Benchmark();
// benchmark.Setup();
// var throughput = benchmark.Throughput();
// Console.WriteLine($"Throughput: {Math.Round(throughput, 1)} MB/s");


public class Speed : IColumn
{
	public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
	{
		if (summary is null || benchmarkCase is null || benchmarkCase.Parameters is null)
		{
			return "N/A";
		}
		var ourReport = summary.Reports.First(x => x.BenchmarkCase.Equals(benchmarkCase));
		long length = new System.IO.FileInfo("sample.txt").Length;
		var mean = ourReport.ResultStatistics.Mean;
		return $"{(length / ourReport.ResultStatistics.Mean):#####.00}";
	}

	public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
	public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
	public bool IsAvailable(Summary summary) => true;

	public string Id { get; } = nameof(Speed);
	public string ColumnName { get; } = "Speed (GB/s)";
	public bool AlwaysShow { get; } = true;
	public ColumnCategory Category { get; } = ColumnCategory.Custom;
	public int PriorityInCategory { get; }
	public bool IsNumeric { get; }
	public UnitType UnitType { get; } = UnitType.Dimensionless;
	public string Legend { get; } = "The speed in gigabytes per second";
}
// [Config(typeof(Config))]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 3)]
[Config(typeof(Config))]
//[MemoryDiagnoser]
public class Benchmark
{
	private class Config : ManualConfig
	{
		public Config()
		{
			AddColumn(new Speed());
			AddDiagnoser(new EventPipeProfiler(EventPipeProfile.CpuSampling));
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
		for (var i = 0; i < 2 * runs; i++)
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
