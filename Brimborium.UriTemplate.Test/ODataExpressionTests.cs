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

        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        var success=result.AppendValue(target);
        var act = target.ToString();
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
