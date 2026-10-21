namespace Brimborium.UriTemplate.Test;

public class ODataVariableTests {
    [Test]
    public async Task ODataVariableTest001() {
        ODataVariable sut = new(
            "aaa",
            new ODataValue("???")
        );
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        var actStd = sut.TryGetValue(substitutions, out var result);
        _ = await Assert.That(actStd).IsTrue();
        _ = await Assert.That(result).IsNotNull();
        var target = new UriTemplateTarget(new StringBuilder());
        _ = await Assert.That(result!.AppendValue(target)).IsTrue();
        _ = await Assert.That(target.Output.ToString()).IsEqualTo("'AA1'");
    }

    [Test]
    public async Task ODataVariableTest002() {
        ODataVariable sut = new(
            "aaa",
            new ODataValue("???")
        );
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "aaa", "aa&bb=cc/dd:ee?ff,gg" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        var actStd = sut.TryGetValue(substitutions, out var result);
        _ = await Assert.That(actStd).IsTrue();
        _ = await Assert.That(result).IsNotNull();
        var target = new UriTemplateTarget(new StringBuilder());
        _ = await Assert.That(result!.AppendValue(target)).IsTrue();
        _ = await Assert.That(target.Output.ToString()).IsEqualTo("'aa%26bb%3Dcc%2Fdd%3Aee%3Fff%2Cgg'");
    }
}
