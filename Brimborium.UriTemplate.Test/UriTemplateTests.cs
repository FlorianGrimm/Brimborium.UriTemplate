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
    [Arguments("{+v}", "%%41", "%25%41")]
    [Arguments("{+v}", "%4 x", "%254%20x")]
    [Arguments("{+v}", "% x", "%25%20x")]
    [Arguments("{#v}", "% x", "#%25%20x")]
    [Arguments("{+v}", "%C3%A9", "%C3%A9")]
    [Arguments("{+v}", "%E2%82%AC", "%E2%82%AC")]
    [Arguments("{+v}", "%80", "%80")]
    public async Task ReservedExpansionHandlesPercent(string template, string value, string expected) {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "v", value },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        var actual = sut.Expand(template, substitutions);
        _ = await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [Arguments("{v}", "a\uD800b", "a%EF%BF%BDb")]
    [Arguments("{+v}", "a\uD800b", "a%EF%BF%BDb")]
    [Arguments("{v}", "\uD83D\uDE00\uD800", "%F0%9F%98%80%EF%BF%BD")]
    [Arguments("{+v}", "\uDE00\uD83D\uDE00", "%EF%BF%BD%F0%9F%98%80")]
    public async Task LoneSurrogateIsReplaced(string template, string value, string expected) {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "v", value },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        var actual = sut.Expand(template, substitutions);
        _ = await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    [Arguments("/a b{v}", "/a%20bx")]
    [Arguments("/a%zz{v}", "/a%25zzx")]
    [Arguments("/a%4{v}", "/a%254x")]
    [Arguments("/a%{v}", "/a%25x")]
    [Arguments("/a%20b{v}", "/a%20bx")]
    [Arguments("/a\"<>\\^`|{v}", "/a%22%3C%3E%5C%5E%60%7Cx")]
    [Arguments("/People('{v}')", "/People('x')")]
    public async Task LiteralNotAllowedInUriIsPercentEncoded(string template, string expected) {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "v", "x" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        var actual = sut.Expand(template, substitutions);
        _ = await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task SemicolonListAndDictionaryKeepEqualsForEmptyFirstItem() {
        Brimborium.UriTemplate.UriTemplateCache sut = new();
        _ = await Assert.That(sut.Expand("{;list}", new Dictionary<string, object?> { { "list", new List<string> { "", "x" } } }))
            .IsEqualTo(";list=,x");
        _ = await Assert.That(sut.Expand("{;list}", new Dictionary<string, object?> { { "list", new List<string> { "" } } }))
            .IsEqualTo(";list");
        _ = await Assert.That(sut.Expand("{;keys}", new Dictionary<string, object?> { { "keys", new Dictionary<string, string> { { "", "v" } } } }))
            .IsEqualTo(";keys=,v");
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
}
