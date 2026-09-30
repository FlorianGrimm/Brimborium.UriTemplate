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
        var success = result.AppendValue(target);
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

    [Test]
    public async Task TryGetValueUsingODataVariableFound() {
        ODataValue oDataValue = new("question");
        ODataValue oDataValueDefault = new("42");
        ODataVariable oDataVariable = new("abc", oDataValueDefault);
        ODataExpression sut = new(oDataVariable);
        Dictionary<string, object?> substitutions = new();
        substitutions.Add("abc", oDataValue);
        var succ = sut.TryGetValue(substitutions, out var result);
        await Assert.That(succ).IsTrue();
        await Assert.That(result).IsEquivalentTo(new ODataExpressionResolved(oDataValue));
    }

    [Test]
    public async Task TryGetValueUsingODataVariableNotFound() {
        ODataValue oDataValueDefault = new("42");
        ODataVariable oDataVariable = new("abc", oDataValueDefault);
        ODataExpression sut = new(oDataVariable);
        Dictionary<string, object?> substitutions = new();
        var succ = sut.TryGetValue(substitutions, out var result);
        await Assert.That(succ).IsTrue();
        await Assert.That(result).IsEquivalentTo(new ODataExpressionResolved(oDataValueDefault));
    }

    [Test]
    public async Task TryGetValueNotAIUriTemplateValue() {
        ODataValue oDataValue = new("42");
        ODataExpression sut = new(oDataValue);
        Dictionary<string, object?> substitutions = new();
        var succ = sut.TryGetValue(substitutions, out var result);
        await Assert.That(succ).IsTrue();
        await Assert.That(result).IsSameReferenceAs(oDataValue);
    }
}
