/*

dotnet run -c Release
dotnet run -c Release -- --memory

*/
namespace Brimborium.UriTemplate.Benchmark;

public class Program {
    public static void Main(string[] args) {
        //var summaries
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

[MemoryDiagnoser]
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
        foreach (var template in this.ListTemplate) {
            var act = global::Std.UriTemplate.Expand(template, Substitutions);
            if (act is not { Length: > 0 }) { throw new Exception(); }
        }
    }

    private readonly Brimborium.UriTemplate.UriTemplateCache _Cache = new();


    [Benchmark]
    public void BrimboriumBenchmark() {
        foreach (var template in this.ListTemplate) {
            var act = _Cache.Expand(template, Substitutions);
            if (act is not { Length: > 0 }) { throw new Exception(); }
        }
    }
}