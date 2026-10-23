/*

dotnet run -c Release -- --filter *
dotnet run -c Release -- --memory

| Method              | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| StdBenchmark        | 1,146.7 ns | 22.90 ns | 44.67 ns |  1.00 |    0.05 | 0.3071 |    3864 B |        1.00 |
| BrimboriumBenchmark |   485.6 ns |  9.04 ns |  8.46 ns |  0.42 |    0.02 | 0.0591 |     744 B |        0.19 |

*/
using System.Diagnostics;

namespace Brimborium.UriTemplate.Benchmark;

public class Program {
    public const int OuterLoopCount = 10_000;
    public const int InnerLoopCount = 1_000;

    public static void Main(string[] args) {
        //var summaries
#if true
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#else
        CommonBenchmark commonBenchmark = new CommonBenchmark();
        commonBenchmark.BrimboriumBenchmark();

        var start = Stopwatch.GetTimestamp();
        for (int i = 0; i < OuterLoopCount; i++) {
            commonBenchmark.BrimboriumBenchmark();
        }
        var e = Stopwatch.GetElapsedTime(start);
        const int LoopCount = InnerLoopCount * OuterLoopCount;
        System.Console.WriteLine($"{e.TotalNanoseconds / LoopCount} ns");
        // 5006999800 ns
#endif
    }
}

/* [MemoryDiagnoser] */
public class CommonBenchmark {
    public readonly Dictionary<string, object?> Substitutions;
    public readonly string[] ListTemplate;
    public CommonBenchmark() {
        this.Substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        this.ListTemplate = [
            "{+baseurl}/def/ghi",
            "{+baseurl}/def/ghi{?aaa,bbb,ccc}",
            "{+baseurl}/def/ghi{?%24top,%24skip,%24filter}"
            ];
    }

    [Benchmark(Baseline = true)]
    public void StdBenchmark() {
        var listTemplate = this.ListTemplate;
        {
            for (int innerIdx = 0; innerIdx < listTemplate.Length; innerIdx++) {
                var template = listTemplate[innerIdx];
                var act = global::Std.UriTemplate.Expand(template, Substitutions);
                if (act is not { Length: > 0 }) { throw new Exception(); }
            }
        }
        for (int loopIdx = 0; loopIdx < Program.InnerLoopCount; loopIdx++) {
            for (int innerIdx = 0; innerIdx < listTemplate.Length; innerIdx++) {
                var template = listTemplate[innerIdx];
                var act = global::Std.UriTemplate.Expand(template, Substitutions);
                if (act is not { Length: > 0 }) { throw new Exception(); }
            }
        }
    }

    private readonly Brimborium.UriTemplate.UriTemplateCache _Cache = new();


    [Benchmark]
    public void BrimboriumBenchmark() {
        var listTemplate = this.ListTemplate;
        for (int loopIdx = 0; loopIdx < Program.InnerLoopCount; loopIdx++) {
            for (int innerIdx = 0; innerIdx < listTemplate.Length; innerIdx++) {
                var template = listTemplate[innerIdx];
                var act = _Cache.Expand(template, Substitutions);
                if (act is not { Length: > 0 }) { throw new Exception(); }
            }
        }
    }
}

/*

dotnet run -c Release -- --filter *AddValueTextBenchmark* --job short

Compares UriTemplateTarget.AddValueText without a prefix modifier ({x}, AddValueTextComplete)
against a prefix modifier that never truncates ({x:9999}, AddValueTextWithMaxChar); both produce the same output.

*/
/*


| Method   | Length | Operator | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------- |------- |--------- |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| MaxChar  | 8      |          |    148.95 ns |   2.983 ns |   3.664 ns |  1.00 |    0.03 | 0.0241 |     304 B |        1.00 |
| Complete | 8      |          |    140.60 ns |   2.854 ns |   5.073 ns |  0.94 |    0.04 | 0.0241 |     304 B |        1.00 |
|          |        |          |              |            |            |       |         |        |           |             |
| MaxChar  | 8      | +        |     69.75 ns |   1.432 ns |   1.195 ns |  1.00 |    0.02 | 0.0038 |      48 B |        1.00 |
| Complete | 8      | +        |     64.34 ns |   1.331 ns |   2.367 ns |  0.92 |    0.04 | 0.0038 |      48 B |        1.00 |
|          |        |          |              |            |            |       |         |        |           |             |
| MaxChar  | 64     |          |    917.66 ns |  17.824 ns |  27.220 ns |  1.00 |    0.04 | 0.1802 |    2264 B |        1.00 |
| Complete | 64     |          |    973.46 ns |  19.423 ns |  35.024 ns |  1.06 |    0.05 | 0.1793 |    2264 B |        1.00 |
|          |        |          |              |            |            |       |         |        |           |             |
| MaxChar  | 64     | +        |    302.82 ns |   6.077 ns |   9.813 ns |  1.00 |    0.05 | 0.0143 |     184 B |        1.00 |
| Complete | 64     | +        |    262.65 ns |   3.594 ns |   3.186 ns |  0.87 |    0.03 | 0.0143 |     184 B |        1.00 |
|          |        |          |              |            |            |       |         |        |           |             |
| MaxChar  | 1024   |          | 13,761.51 ns | 266.846 ns | 337.475 ns |  1.00 |    0.03 | 2.8534 |   35864 B |        1.00 |
| Complete | 1024   |          | 13,051.57 ns | 256.633 ns | 351.282 ns |  0.95 |    0.03 | 2.8534 |   35864 B |        1.00 |
|          |        |          |              |            |            |       |         |        |           |             |
| MaxChar  | 1024   | +        |  4,111.03 ns |  80.707 ns | 155.495 ns |  1.00 |    0.05 | 0.1984 |    2584 B |        1.00 |
| Complete | 1024   | +        |  3,357.28 ns |  63.471 ns |  65.180 ns |  0.82 |    0.03 | 0.2022 |    2584 B |        1.00 |
 
*/
[MemoryDiagnoser]
public class AddValueTextBenchmark {
    private readonly Brimborium.UriTemplate.UriTemplateCache _Cache = new();
    private Dictionary<string, object?> _Substitutions = new();
    private string _TemplateMaxChar = string.Empty;
    private string _TemplateComplete = string.Empty;

    [Params(8, 64, 1024)]
    public int Length { get; set; }

    [Params("", "+")]
    public string Operator { get; set; } = string.Empty;

    [GlobalSetup]
    public void Setup() {
        const string pattern = "ab cd/ef";
        var value = string.Concat(Enumerable.Repeat(pattern, Length / pattern.Length + 1))[..Length];
        this._Substitutions = new Dictionary<string, object?> { { "x", value } };
        this._TemplateComplete = $"{{{Operator}x}}";
        this._TemplateMaxChar = $"{{{Operator}x:9999}}";
        if (this.MaxChar() != this.Complete()) {
            throw new InvalidOperationException("Both paths must produce the same output.");
        }
    }

    [Benchmark(Baseline = true)]
    public string MaxChar() => this._Cache.Expand(this._TemplateMaxChar, this._Substitutions);

    [Benchmark]
    public string Complete() => this._Cache.Expand(this._TemplateComplete, this._Substitutions);
}