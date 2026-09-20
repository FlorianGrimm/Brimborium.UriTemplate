namespace Brimborium.UriTemplate.Test;

public class ODataExpressionTests {
    [Test]
    public async Task ODataExpressionWithSubstitutionsSuccess() {
        Dictionary<string, object?> substitutions = new() {
            { "greeting", "World" }
        };
        ODataExpression sut = new(
            new ODataOperation(
                new ODataConstant("Hello"),
                "eq",
                new ODataVariable("greeting", null)
            ));
        if (sut.TryGetValue(substitutions, out var result)) {
        } else {
            throw new Exception();
        }

        Span<char> outputBuffer = stackalloc char[4096];
        UriTemplateTarget target = new(outputBuffer);
        var success = result.AppendValue(ref target);
        var act = target.Output.ToStringAndDispose();
        await Assert.That(act).IsEqualTo("Hello%20eq%20'World'");
        await Assert.That(success).IsTrue();
    }

    [Test]
    public async Task ODataExpressionWithSubstitutionsFailed() {
        Dictionary<string, object?> substitutions = new() {
        };
        ODataExpression sut = new(
            new ODataOperation(
                new ODataConstant("Hello"),
                "eq",
                new ODataVariable("greeting", null)
            ));
        if (sut.TryGetValue(substitutions, out var result)) {
            throw new Exception();
        } else {
            // OK
        }
    }
}
