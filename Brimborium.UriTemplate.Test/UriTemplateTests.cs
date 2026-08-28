#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Runtime.CompilerServices;

namespace Brimborium.UriTemplate;

public class UriTemplateTests {
    [Test]
    public async Task TestPlus() {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi";
        string expected = "https://server/abc/def/ghi";
        var actStd = global::Std.UriTemplate.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);

        var astActual = sut.Parse(template);
        _ = await Assert.That(astActual).IsEquivalentTo(
            new UriTemplateASTSequence(
                new UriTemplateASTOperation(UriTemplateASTOperator.Plus, [new UriTemplateASTPlaceholder("baseurl", false, -1)]),
                new UriTemplateASTConstant("/def/ghi")
                ));

        var actBrimborium = sut.Expand(template, substitutions);
        _ = await Assert.That(actBrimborium).IsEqualTo(expected);
    }

    [Test]
    public async Task TestQuestionmark() {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();
        string template = "{+baseurl}/def/ghi{?aaa,bbb,ccc}";
        string expected = "https://server/abc/def/ghi?aaa=AA1&bbb=BB2&ccc=CC3";
        var actStd = global::Std.UriTemplate.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);

        var astActual = sut.Parse(template);
        _ = await Assert.That(astActual).IsEquivalentTo(
            new UriTemplateASTSequence(
                new UriTemplateASTOperation(
                    UriTemplateASTOperator.Plus,
                    [new UriTemplateASTPlaceholder("baseurl", false, -1)]),
                new UriTemplateASTConstant("/def/ghi"),
                new UriTemplateASTOperation(
                    UriTemplateASTOperator.Questionmark,
                    [
                        new UriTemplateASTPlaceholder("aaa", false, -1),
                            new UriTemplateASTPlaceholder("bbb", false, -1),
                            new UriTemplateASTPlaceholder("ccc", false, -1),

                        ])
                ));

        var actBrimborium = sut.Expand(template, substitutions);
        _ = await Assert.That(actBrimborium).IsEqualTo(expected);
    }

    [Test]
    public async Task TestFiles() {
        var content_extended_tests = await System.IO.File.ReadAllTextAsync(GetFileName("extended-tests.json"));
        _ = await Assert.That(content_extended_tests).IsNotNullOrEmpty();
    }

    private static string GetFileName(string filename) {
        return System.IO.Path.Combine(GetFolder(), filename);

        static string GetFolder([CallerFilePath] string callerFilePath = "") {
            return System.IO.Path.GetDirectoryName(callerFilePath) ?? throw new Exception();
        }
    }
}

