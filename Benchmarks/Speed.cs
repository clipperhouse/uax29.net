using BenchmarkDotNet.Running;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;

public class Speed : IColumn
{
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        if (summary is null || benchmarkCase is null || benchmarkCase.Parameters is null)
        {
            return "N/A";
        }
        var ourReport = summary.Reports.First(x => x.BenchmarkCase.Equals(benchmarkCase));
        long length = new FileInfo("sample.txt").Length;
        var mean = ourReport.ResultStatistics!.Mean;
        return $"{length / mean:#####.000} GB/s";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public bool IsAvailable(Summary summary) => true;

    public string Id { get; } = nameof(Speed);
    public string ColumnName { get; } = "Throughput";
    public bool AlwaysShow { get; } = true;
    public ColumnCategory Category { get; } = ColumnCategory.Custom;
    public int PriorityInCategory { get; }
    public bool IsNumeric { get; }
    public UnitType UnitType { get; } = UnitType.Dimensionless;
    public string Legend { get; } = "The speed in gigabytes per second";
}
