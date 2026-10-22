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
    public async Task TestOD() {
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
    [Arguments("{v}")]
    [Arguments("{+v}")]
    [Arguments("{#v}")]
    [Arguments("{.v}")]
    [Arguments("{/v}")]
    [Arguments("{;v}")]
    [Arguments("{?v}")]
    [Arguments("{&v}")]
    [Arguments("{+list}")]
    [Arguments("{+list*}")]
    [Arguments("{#list}")]
    [Arguments("{#list*}")]
    [Arguments("{+keys}")]
    [Arguments("{+keys*}")]
    [Arguments("{#keys}")]
    [Arguments("{#keys*}")]
    public async Task CrLfInValueIsPercentEncoded(string template) {
        _ = await Assert.That(() => UriTemplateParser.Parse(template)).IsNotNull();

    }

    [Test]
    [Arguments("{+v}", "a\r\nSet-Cookie: x=1", "a%0D%0ASet-Cookie:%20x=1")]
    [Arguments("{#v}", "a\r\nSet-Cookie: x=1", "#a%0D%0ASet-Cookie:%20x=1")]
    [Arguments("{+v}", "%\r\n", "%25%0D%0A")]
    [Arguments("{+v}", "a<b>{c}|d", "a%3Cb%3E%7Bc%7D%7Cd")]
    [Arguments("{+v}", "a\"b\\c^d`e", "a%22b%5Cc%5Ed%60e")]
    [Arguments("{+v}", "a\tb\0c\u007Fd", "a%09b%00c%7Fd")]
    [Arguments("{+v}", "%20\r", "%20%0D")]
    [Arguments("{+v}", ":/?#[]@!$&'()*+,;=", ":/?#[]@!$&'()*+,;=")]
    public async Task ReservedExpansionEncodesCharactersNotAllowedInUri(string template, string value, string expected) {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "v", value },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        var actual = sut.Expand(template, substitutions);
        _ = await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [Arguments("/a\r\nb{v}")]
    [Arguments("/a\rb{v}")]
    [Arguments("/a\nb{v}")]
    [Arguments("/a\tb{v}")]
    [Arguments("/a\0b{v}")]
    [Arguments("/a\u001Fb{v}")]
    [Arguments("/a\u007Fb{v}")]
    public async Task ControlCharacterInLiteralIsRejected(string template) {
        _ = await Assert.That(() => UriTemplateParser.Parse(template)).Throws<ArgumentException>();

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "v", "x" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();
        await Assert.That(() => sut.Expand(template, substitutions))
            .Throws<ArgumentException>();
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

