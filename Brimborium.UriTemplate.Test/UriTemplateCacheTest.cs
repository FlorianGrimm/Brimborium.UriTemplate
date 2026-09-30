namespace Brimborium.UriTemplate.Test;

public class UriTemplateCacheTests {
    [Test]
    [Arguments("bbb", "eq", "World's Hello", "https://server/abc/def/ghi?$filter=bbb%20eq%20'World''s%20Hello'")]
    [Arguments("ccc", "eq", "c'd", "https://server/abc/def/ghi?$filter=ccc%20eq%20'c''d'")]
    public async Task OdataFilter(
        string propertyName,
        string operation,
        string value,
        string expected
        ) {
        ODataOperation filter = new(
            new ODataConstant(propertyName),
            operation,
            new ODataValue(value));
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$filter", filter},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ExpandWithList() {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$filter", new List<string>(){ "AA1", "BB2" } },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        var actTemplate = sut.Parse(template);
        var act = sut.Expand(actTemplate, substitutions);
        _ = await Assert.That(act).IsEqualTo("https://server/abc/def/ghi?$filter=AA1,BB2");
    }
}