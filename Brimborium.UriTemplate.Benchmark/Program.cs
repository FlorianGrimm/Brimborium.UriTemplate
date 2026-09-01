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