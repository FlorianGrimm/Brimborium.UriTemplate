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
}
