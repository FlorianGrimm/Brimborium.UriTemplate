namespace Brimborium.UriTemplate.Test;

public class ODataListTests {
        [Test]
    public async Task ODataListEmpty() {
        ODataList select = new(
            "$select",
            ",",
            [
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$select", select},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        string expected = "https://server/abc/def/ghi?$select=";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataList1() {
        ODataList select = new(
            "$select",
            ",",
            [
                new ODataFieldName("columnABC")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$select", select},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        string expected = "https://server/abc/def/ghi?$select=columnABC";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataList2() {
        ODataList select = new(
            "$select",
            ",",
            [
                new ODataFieldName("columnABC"),
                new ODataFieldName("columnDEF")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$select", select},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        string expected = "https://server/abc/def/ghi?$select=columnABC,columnDEF";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }
}
