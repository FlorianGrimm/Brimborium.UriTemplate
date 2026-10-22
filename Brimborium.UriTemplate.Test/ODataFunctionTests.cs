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

    [Test]
    public async Task ODataFunctionTrailingUnresolvedArgumentHasNoTrailingComma() {
        ODataFunction sut = new(
            "f",
            [
                new ODataFieldName("a"),
                new ODataVariable("missing", null)
            ]);
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        var result = sut.AppendValue(target);
        _ = await Assert.That(output.ToString()).IsEqualTo("f(a)");
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ODataFunctionMiddleUnresolvedArgumentIsSkipped() {
        ODataFunction sut = new(
            "f",
            [
                new ODataFieldName("a"),
                new ODataVariable("missing", null),
                new ODataFieldName("c")
            ]);
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        var result = sut.AppendValue(target);
        _ = await Assert.That(output.ToString()).IsEqualTo("f(a,c)");
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ODataFunctionOnlyUnresolvedArgumentsHasEmptyArgumentList() {
        ODataFunction sut = new(
            "f",
            [
                new ODataVariable("missing1", null),
                new ODataVariable("missing2", null)
            ]);
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        var result = sut.AppendValue(target);
        _ = await Assert.That(output.ToString()).IsEqualTo("f()");
        _ = await Assert.That(result).IsFalse();
    }
}
