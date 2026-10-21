using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Brimborium.UriTemplate.Test;

public class UriTemplateParserTests {
    [Test]
    public async Task EmptyVariableNamesAreRejected() {
        await Assert.That(() => UriTemplateParser.Parse("{}"))
            .Throws<ArgumentException>();
        await Assert.That(() => UriTemplateParser.Parse("{,foo}"))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task SpecialCasesTest() {
        {
            var ast = UriTemplateParser.Parse("{keys:1}");
            _ = await Assert.That(ast).IsNotNull();
        }
    }

    private static string GetFilename(string filename) {
        return System.IO.Path.Combine(
                GetFolder(),
                filename
            );
        static string GetFolder([System.Runtime.CompilerServices.CallerFilePath] string filename = "") {
            return System.IO.Path.GetDirectoryName(filename) ?? throw new ArgumentException("Unable to determine folder");
        }
    }

    [Test]
    [Arguments("spec-examples.json")]
    [Arguments("extended-tests.json")]
    [Arguments("negative-tests.json")]
    public async Task FileTests(string filename) {
        var filenameFQN = GetFilename(filename);
        // Read and parse the JSON file
        var content = System.IO.File.ReadAllText(filenameFQN);
        var jsonDocument = System.Text.Json.JsonDocument.Parse(content);
        foreach (var testObject in jsonDocument.RootElement.EnumerateObject()) {
            var name = testObject.Name;
            var testObjectValue = testObject.Value;
            if (!testObjectValue.TryGetProperty("variables", out var jsonVariables)) { throw new Exception($"variables {filenameFQN}"); }
            if (!testObjectValue.TryGetProperty("testcases", out var jsonTestcases)) { throw new Exception($"testcases {filenameFQN}"); }

            var dictVariables = ConvertToDictionary(jsonVariables);
            if (dictVariables is null) { throw new Exception($"Unable to deserialize variables {filenameFQN}"); }

            foreach (var testCase in jsonTestcases.EnumerateArray()) {
                if (testCase.ValueKind != JsonValueKind.Array) {
                    throw new Exception($"testCase is not array {name} {filenameFQN}");
                }
                if (testCase.GetArrayLength() != 2) {
                    throw new Exception($"testCase is not array of 2 elements {name} {filenameFQN}");
                }
                using var e = testCase.EnumerateArray();
                if (!e.MoveNext()) { throw new Exception($"testCase is empty {name} {filenameFQN}"); }
                var jsonTemplate = e.Current;
                if (!e.MoveNext()) { throw new Exception($"testCase has only one element {name} {filenameFQN}"); }
                var jsonExpected = e.Current;

                var template = jsonTemplate.GetString() ?? throw new Exception($"template is null {name} {filenameFQN}");
                var expected = ConvertValue(jsonExpected);
                if (expected is string stringExpected) {
                    if (template == "{+path:6}/here") { 
                    }
                    var ast = UriTemplateParser.Parse(template);
                    var output = new StringBuilder();
                    UriTemplateTarget target = new(output);
                    _ = ast.Expand(dictVariables, target);
                    _ = await Assert.That(output.ToString()).IsEqualTo(stringExpected).Because($"{filename} {name} {template} {stringExpected}");
                } else if (expected is bool boolExpected) {
                    UriTemplateASTSequence? ast;
                    Exception? error;
                    try {
                        ast = UriTemplateParser.Parse(template);
                        error = null; ;
                    } catch (Exception ex) {
                        error = ex;
                        ast = null;
                    }
                    if (boolExpected) {
                        _ = await Assert.That(error).IsNull();
                        _ = await Assert.That(ast).IsNotNull();
                        _ = await Assert.That(ast.ListChild).IsNotEmpty();
                    } else {
                        if (error is not null) {
                            _ = await Assert.That(ast).IsNull();
                        } else {
                            if (ast is null) {
                                throw new Exception("cannot be..");
                            } else {
                                if (0 == ast.ListChild.Length) {
                                    // OK
                                } else {
                                    // _ = await Assert.That(ast.ListChild).IsEmpty();
                                    throw new Exception($"{filename} {name} {template} {boolExpected}");
                                }
                            }
                        }
                    }
                } else {
                    // System.Console.Out.WriteLine($"Unexpected expected value: {filename} {name} {expected}");
                }
            }
        }
    }


    private static object? ConvertValue(JsonElement jsonValue) {
        switch (jsonValue.ValueKind) {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.Array:
                return ConvertToList(jsonValue);
            case JsonValueKind.Object:
                return ConvertToDictionary(jsonValue);
            case JsonValueKind.String:
                return jsonValue.GetString();
            case JsonValueKind.Number:
                if (jsonValue.TryGetInt32(out var intValue)) {
                    return intValue;
                } else if (jsonValue.TryGetDouble(out var doubleValue)) {
                    return doubleValue;
                } else {
                    throw new Exception($"Unable to parse number {jsonValue}");
                }
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            default:
                throw new Exception($"Unsupported variable type {jsonValue.ValueKind}");
        }
    }

    private static Dictionary<string, object?> ConvertToDictionary(JsonElement jsonValue) {
        if (jsonValue.ValueKind != JsonValueKind.Object) {
            throw new Exception($"variables is not object");
        }
        Dictionary<string, object?> result = new();
        foreach (var prop in jsonValue.EnumerateObject()) {
            var name = prop.Name;
            var value = prop.Value;
            switch (value.ValueKind) {
                case JsonValueKind.Null:
                    result.Add(name, null);
                    break;
                case JsonValueKind.Array:
                    result.Add(name, ConvertToList(value));
                    break;
                case JsonValueKind.Object:
                    result.Add(name, ConvertToDictionary(value));
                    break;
                case JsonValueKind.String:
                    result.Add(name, value.GetString());
                    break;
                case JsonValueKind.Number:
                    if (value.TryGetInt32(out var intValue)) {
                        result.Add(name, intValue);
                    } else if (value.TryGetDouble(out var doubleValue)) {
                        result.Add(name, doubleValue);
                    } else {
                        throw new Exception($"Unable to parse number {name}");
                    }
                    break;
                case JsonValueKind.True:
                    result.Add(name, true);
                    break;
                case JsonValueKind.False:
                    result.Add(name, false);
                    break;
                default:
                    throw new Exception($"Unsupported variable type {name} {value.ValueKind}");
            }
        }
        return result;
    }

    private static List<object?> ConvertToList(JsonElement jsonValue) {
        if (jsonValue.ValueKind != JsonValueKind.Array) {
            throw new Exception($"value is not array");
        }
        List<object?> result = new();
        foreach (var item in jsonValue.EnumerateArray()) {
            switch (item.ValueKind) {
                case JsonValueKind.Null:
                    result.Add(null);
                    break;
                case JsonValueKind.Array:
                    result.Add(ConvertToList(item));
                    break;
                case JsonValueKind.Object:
                    result.Add(ConvertToDictionary(item));
                    break;
                case JsonValueKind.String:
                    result.Add(item.GetString());
                    break;
                case JsonValueKind.Number:
                    if (item.TryGetInt32(out var intValue)) {
                        result.Add(intValue);
                    } else if (item.TryGetDouble(out var doubleValue)) {
                        result.Add(doubleValue);
                    } else {
                        throw new Exception($"Unable to parse number {item.ToString()}");
                    }
                    break;
                case JsonValueKind.True:
                    result.Add(true);
                    break;
                case JsonValueKind.False:
                    result.Add(false);
                    break;
                default:
                    throw new Exception($"Unsupported variable type {item.ValueKind} {item.ToString()}");
            }
        }
        return result;
    }
}