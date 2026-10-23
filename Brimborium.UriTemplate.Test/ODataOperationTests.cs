namespace Brimborium.UriTemplate.Test;

public class ODataOperationTests {
    [Test]
    [Arguments("Hello", "eq", "World", "Hello%20eq%20'World'")]
    [Arguments("ab", "eq", "c'd", "ab%20eq%20'c''d'")]
    public async Task ODataOperationSimpleTest(
        string propertyName,
        string operation,
        string value,
        string exptected
        ) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataOperation sut = new(
                new ODataConstant(propertyName),
                operation,
                new ODataValue(value));
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(exptected);
        }
    }

    [Test]
    public async Task ODataOperationUnresolvedOperandLeavesNoOutput() {
        StringBuilder output = new("x");
        UriTemplateTarget target = new(output);
        ODataOperation sut = new(
            new ODataVariable("missing", null),
            "eq",
            new ODataValue(1));
        var result = sut.AppendValue(target);
        await Assert.That(result).IsFalse();
        await Assert.That(output.ToString()).IsEqualTo("x");
    }
}
