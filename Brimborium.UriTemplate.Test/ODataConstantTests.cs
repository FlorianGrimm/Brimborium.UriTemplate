namespace Brimborium.UriTemplate.Test;

public class ODataConstantTests {
    // TODO add tests for ODataConstant

    [Test]
    public async Task ODataConstantCrLfIsNotEmittedRaw() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataConstant sut = new("a\r\nSet-Cookie: x=1");
        sut.AppendValue(target);
        await Assert.That(output.ToString()).DoesNotContain("\r");
        await Assert.That(output.ToString()).DoesNotContain("\n");
    }

    [Test]
    public async Task ODataFieldNameCrLfIsNotEmittedRaw() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataFieldName sut = new("a\r\nSet-Cookie: x=1");
        sut.AppendValue(target);
        await Assert.That(output.ToString()).DoesNotContain("\r");
        await Assert.That(output.ToString()).DoesNotContain("\n");
    }
}
