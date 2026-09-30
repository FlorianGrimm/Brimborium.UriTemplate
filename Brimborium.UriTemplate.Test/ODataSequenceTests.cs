namespace Brimborium.UriTemplate.Test;

public class ODataSequenceTests {
    [Test]
    public async Task ODataSequence1() {
        ODataSequence greeeting = new(
            "greeeting",
            [
                new ODataRawString("caption1")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "greeeting", greeeting},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{#greeeting}";
        string expected = "https://server/abc/def/ghi#caption1";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataSequence2() {
        ODataSequence greeeting = new(
            "greeeting",
            [
                new ODataRawString("caption1"),
                new ODataRawString("title2")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "greeeting", greeeting},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{#greeeting}";
        string expected = "https://server/abc/def/ghi#caption1title2";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }
}
