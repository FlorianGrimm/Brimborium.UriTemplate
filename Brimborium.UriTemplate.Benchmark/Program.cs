/*

dotnet run -c Release -- --filter *

| Method              | Mean       | Error    | StdDev   | Ratio | RatioSD |
|-------------------- |-----------:|---------:|---------:|------:|--------:|
| StdBenchmark        | 1,359.7 us | 26.88 us | 33.01 us |  1.00 |    0.03 |
| BrimboriumBenchmark |   474.9 us |  7.96 us |  8.52 us |  0.35 |    0.01 |

| Method              | Mean       | Error    | StdDev   | Ratio | RatioSD |
|-------------------- |-----------:|---------:|---------:|------:|--------:|
| StdBenchmark        | 1,192.1 us | 23.70 us | 26.34 us |  1.00 |    0.03 |
| BrimboriumBenchmark |   679.6 us | 13.40 us | 13.16 us |  0.57 |    0.02 |

dotnet run -c Release -- --filter * --memory

| Method              | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0     | Allocated  | Alloc Ratio |
|-------------------- |-----------:|---------:|---------:|------:|--------:|---------:|-----------:|------------:|
| StdBenchmark        | 1,122.9 us | 22.25 us | 38.97 us |  1.00 |    0.05 | 306.6406 | 3777.21 KB |        1.00 |
| BrimboriumBenchmark |   465.4 us |  9.22 us | 16.15 us |  0.41 |    0.02 |  58.5938 |  726.56 KB |        0.19 |

| Method              | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0     | Allocated  | Alloc Ratio |
|-------------------- |-----------:|---------:|---------:|------:|--------:|---------:|-----------:|------------:|
| StdBenchmark        | 1,194.6 us | 23.65 us | 46.12 us |  1.00 |    0.05 | 306.6406 | 3777.21 KB |        1.00 |
| BrimboriumBenchmark |   655.0 us | 12.35 us | 14.22 us |  0.55 |    0.02 |  40.0391 |  492.19 KB |        0.13 |
*/
using System.Diagnostics;

namespace Brimborium.UriTemplate.Benchmark;

public class Program {
    public const int OuterLoopCount = 10_000;
    public const int InnerLoopCount = 1_000;

    public static void Main(string[] args) {
        //var summaries
#if false
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
            { "$filter", new ODataExpression(new ODataOperation(new ODataFieldName("aaa"), "eq", new ODataVariable("aaa", default)))}
        };
        this.ListTemplate = [
            "{+baseurl}/def/ghi",
            "{+baseurl}/def/ghi{?aaa,bbb,ccc}",
            "{+baseurl}/def/ghi{?%24top,%24skip,%24filter}",
            "{+baseurl}/def/ghi{?$top,$skip,$filter}"
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