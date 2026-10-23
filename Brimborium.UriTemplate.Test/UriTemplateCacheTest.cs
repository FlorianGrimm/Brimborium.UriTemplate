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

    [Test]
    public async Task ODataDateTimeLiteralStyleIsAppliedByCache() {
        ODataOperation filter = new(
            new ODataFieldName("Created"),
            "gt",
            new ODataValue(new DateOnly(2024, 1, 2)));
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "$filter", filter },
        };
        string template = "/People{?$filter}";

        Brimborium.UriTemplate.UriTemplateCache quoted = new(
            options: ODataConvertOptions.Quoted,
            stringBuilderPool: null);
        _ = await Assert.That(quoted.Options.ODataDateTimeLiteralStyle).IsEqualTo(ODataDateTimeLiteralStyle.Quoted);
        _ = await Assert.That(quoted.Expand(template, substitutions))
            .IsEqualTo("/People?$filter=Created%20gt%20'2024-01-02'");

        Brimborium.UriTemplate.UriTemplateCache unquoted = new(
            options: ODataConvertOptions.Unquoted,
            stringBuilderPool: null);

        _ = await Assert.That(unquoted.Expand(template, substitutions))
            .IsEqualTo("/People?$filter=Created%20gt%202024-01-02");
        _ = await Assert.That(unquoted.Expand(unquoted.Parse(template), substitutions))
            .IsEqualTo("/People?$filter=Created%20gt%202024-01-02");
    }
}
