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
            Span<char> outputBuffer = stackalloc char[4096];
            UriTemplateTarget target = new(outputBuffer);
            ODataOperation sut = new(
                new ODataConstant(propertyName),
                operation,
                new ODataValue(value));
            sut.AppendValue(ref target);
            var result = target.ToStringAndDispose();
            await Assert.That(result).IsEqualTo(exptected);
        }
    }
}
