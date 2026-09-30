namespace Brimborium.UriTemplate.Test;

public class ODataFunctionTests {
    [Test]
    public async Task ODataFunctionTest001() {
        ODataFunction filter = new(
            "func",
            [
                new ODataFieldName("caption1"),
                new ODataVariable("aaa", new ODataValue("???"))
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$filter", filter},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}{?$filter}";
        string expected = "https://server/abc?$filter=func(caption1,'AA1')";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }
}
