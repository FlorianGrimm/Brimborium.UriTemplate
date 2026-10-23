using System;
using System.Collections.Generic;
using System.Text;

namespace Brimborium.UriTemplate.Test;

public class UriTemplateTargetTests {
    [Test]
    public async Task SurrogateAstonishedFaceTest() {
        // \u1234\ud800 supplementary value (U+1F632 ASTONISHED FACE)
        {
            StringBuilder output = new();
            UriTemplateTarget target = new UriTemplateTarget(output);
            target.AddValueText(null, char.ConvertFromUtf32(0x1F632).ToString(), -1, false);
            await Assert.That(output.ToString()).IsEqualTo("%F0%9F%98%B2");
        }
        {
            StringBuilder output = new();
            UriTemplateTarget target = new UriTemplateTarget(output);
            target.AddValueText(null, "\ud83d\ude32", -1, false);
            await Assert.That(output.ToString()).IsEqualTo("%F0%9F%98%B2");
        }
    }
    [Test]
    public async Task ReplaceReservedTest() {
        // \u1234\ud800 supplementary value (U+1F632 ASTONISHED FACE)
        {
            StringBuilder output = new();
            UriTemplateTarget target = new UriTemplateTarget(output);
            target.AddValueText(null, "a c", -1, false);
            await Assert.That(output.ToString()).IsEqualTo("a%20c");
        }
        {
            StringBuilder output = new();
            UriTemplateTarget target = new UriTemplateTarget(output);
            target.AddValueText(null, "a c", -1, true);
            await Assert.That(output.ToString()).IsEqualTo("a%20c");
        }
        {
            StringBuilder output = new();
            UriTemplateTarget target = new UriTemplateTarget(output);
            target.AddValueText(null, "a%20c", -1, false);
            await Assert.That(output.ToString()).IsEqualTo("a%20c");
        }
        {
            StringBuilder output = new();
            UriTemplateTarget target = new UriTemplateTarget(output);
            target.AddValueText(null, "a%20c", -1, true);
            await Assert.That(output.ToString()).IsEqualTo("a%2520c");
        }
    }

    [Test]
    [Arguments("", false)]
    [Arguments("", true)]
    [Arguments("a b,c/d?e#f", false)]
    [Arguments("a b,c/d?e#f", true)]
    [Arguments("%41%zz%4%", false)]
    [Arguments("%41%zz%4%", true)]
    [Arguments("\uD83D\uDE00x\uD800y\uDE00", false)]
    [Arguments("\uD83D\uDE00x\uD800y\uDE00", true)]
    [Arguments("é\uE000<>\"{}|\\^`\r\n", false)]
    [Arguments("é\uE000<>\"{}|\\^`\r\n", true)]
    public async Task AddValueTextCompleteMatchesMaxCharPath(string value, bool replaceReserved) {
        StringBuilder complete = new();
        new UriTemplateTarget(complete).AddValueText("=", value, -1, replaceReserved);

        StringBuilder withMaxChar = new();
        new UriTemplateTarget(withMaxChar).AddValueText("=", value, 9999, replaceReserved);

        await Assert.That(complete.ToString()).IsEqualTo(withMaxChar.ToString());
        await Assert.That(complete.ToString().StartsWith('=')).IsEqualTo(value.Length > 0);
    }

    [Test]
    [Arguments("a b", "a%20b")]
    [Arguments("a,b", "a%2Cb")]
    [Arguments("a:b", "a%3Ab")]
    [Arguments("a/b", "a%2Fb")]
    [Arguments("a&b", "a%26b")]
    [Arguments("a%b", "a%25b")]
    [Arguments("a=b", "a%3Db")]
    [Arguments("a?b", "a%3Fb")]
    [Arguments("a#b", "a%23b")]
    [Arguments("Tom & Jerry #1 + x", "Tom%20%26%20Jerry%20%231%20%2B%20x")]
    public async Task AddODataValueTests(string value, string expected) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new UriTemplateTarget(output);
            target.AddODataValue(value);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    public async Task ConvertNativeTypesLongTest() {
        await Assert.That(ODataConvert.ConvertValueToString(9223372036854775807L, ODataConvertOptions.Unquoted)).IsEqualTo("9223372036854775807");
    }

    [Test]
    public async Task ConvertNativeTypesFloatTest() {
        await Assert.That(ODataConvert.ConvertValueToString(1.5f, ODataConvertOptions.Unquoted)).IsEqualTo("1.5");
    }

    [Test]
    public async Task ConvertNativeTypesDoubleTest() {
        await Assert.That(ODataConvert.ConvertValueToString(1.25d, ODataConvertOptions.Unquoted)).IsEqualTo("1.25");
    }

    [Test]
    public async Task ConvertNativeTypesDecimalTest() {
        await Assert.That(ODataConvert.ConvertValueToString(12.34m, ODataConvertOptions.Unquoted)).IsEqualTo("12.34");
    }

    [Test]
    public async Task ConvertNativeTypesTimeOnlyTest() {
        await Assert.That(ODataConvert.ConvertValueToString(new TimeOnly(9, 5, 7), ODataConvertOptions.Unquoted)).IsEqualTo("09:05:07");
        await Assert.That(ODataConvert.ConvertValueToString(new TimeOnly(19, 5, 7), ODataConvertOptions.Unquoted)).IsEqualTo("19:05:07");
        await Assert.That(ODataConvert.ConvertValueToString(new TimeOnly(1, 2, 3, 456), ODataConvertOptions.Unquoted)).IsEqualTo("01:02:03.4560000");
    }

    [Test]
    public async Task UriTemplateTargetConvertNativeTypesTimeOnlyKeepsFractionalSeconds() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new TimeOnly(9, 5, 7))).IsEqualTo("09:05:07");
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new TimeOnly(1, 2, 3, 456))).IsEqualTo("01:02:03.4560000");

        UriTemplateCache sut = new();
        var actual = sut.Expand("{t}", new Dictionary<string, object?> { { "t", new TimeOnly(1, 2, 3, 456) } });
        await Assert.That(actual).IsEqualTo("01%3A02%3A03.4560000");
    }

    [Test]
    public async Task UriTemplateTargetConvertNativeTypesPlainValues() {
        (object? Value, string? Expected)[] cases = [
            (null, ""),
            ("a b", "a b"),
            (true, "true"),
            (-12, "-12"),
            (9223372036854775807L, "9223372036854775807"),
            (1.5f, "1.5"),
            (float.PositiveInfinity, "Infinity"),
            (1.25d, "1.25"),
            (double.NegativeInfinity, "-Infinity"),
            (double.NaN, "NaN"),
            (12.34m, "12.34"),
            (new DateOnly(2024, 1, 2), "2024-01-02"),
            (new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified), "2024-01-02T03:04:05"),
            (new DateTime(2024, 1, 2, 3, 4, 5, 590, DateTimeKind.Unspecified), "2024-01-02T03:04:05.5900000"),
            (new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), "2024-01-02T03:04:05Z"),
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)), "2024-01-02T03:04:05+02:00"),
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, 590, TimeSpan.FromHours(-6)), "2024-01-02T03:04:05.5900000-06:00"),
            (new object(), null),
        ];
        foreach (var (value, expected) in cases) {
            await Assert.That(UriTemplateTarget.ConvertNativeTypes(value)).IsEqualTo(expected).Because($"{value?.GetType().Name} {value}");
        }

        UriTemplateCache sut = new();
        var actual = sut.Expand("{x}", new Dictionary<string, object?> { { "x", new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified) } });
        await Assert.That(actual).IsEqualTo("2024-01-02T03%3A04%3A05");
    }

    [Test]
    public async Task ODataConvertNullOptionsBehaveLikeQuoted() {
        var unspecified = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);
        await Assert.That(ODataConvert.ConvertValueToString(unspecified, null)).IsEqualTo("2024-01-02T03:04:05");
        await Assert.That(ODataConvert.ConvertValueToString(unspecified, ODataConvertOptions.Quoted)).IsEqualTo("2024-01-02T03:04:05");

        StringBuilder output = new();
        new ODataValue(new DateOnly(2024, 1, 2)).AppendValue(new UriTemplateTarget(output, null!));
        await Assert.That(output.ToString()).IsEqualTo("'2024-01-02'");
    }

    [Test]
    public async Task ConvertNativeTypesDateOnlyTest() {
        await Assert.That(ODataConvert.ConvertValueToString(new DateOnly(2024, 1, 2), ODataConvertOptions.Unquoted)).IsEqualTo("2024-01-02");
    }

    [Test]
    public async Task ConvertNativeTypesDateTimeTest_Utc() {
        await Assert.That(ODataConvert.ConvertValueToString(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), ODataConvertOptions.Unquoted)).IsEqualTo("2024-01-02T03:04:05Z");
    }

    [Test]
    public async Task ConvertNativeTypesDateTimeTest_Unspecified() {
        await Assert.That(ODataConvert.ConvertValueToString(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified), ODataConvertOptions.Unquoted)).IsEqualTo("2024-01-02T03:04:05Z");
    }

    [Test]
    public async Task ConvertNativeTypesDateTimeOffsetTest() {
        await Assert.That(ODataConvert.ConvertValueToString(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)), ODataConvertOptions.Unquoted)).IsEqualTo("2024-01-02T03:04:05+02:00");
    }

    [Test]
    public async Task ConvertNativeTypesUnsupportedTypeTest() {
        await Assert.That(ODataConvert.ConvertValueToString(new object(), ODataConvertOptions.Unquoted)).IsNull();
    }

    [Test]
    [Arguments("{keys}")]
    [Arguments("{keys*}")]
    [Arguments("{?keys}")]
    [Arguments("{?keys*}")]
    public async Task AddDictionaryValueNonStringKeyThrows(string template) {
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "keys", new Dictionary<int, string> { { 1, "a" }, { 2, "b" } } },
        };
        UriTemplateCache sut = new();
        await Assert.That(() => sut.Expand(template, substitutions))
            .Throws<ArgumentException>()
            .WithMessageContaining("Dictionary key must be a string");
    }

    [Test]
    [Arguments("th-TH")]
    [Arguments("ar-SA")]
    [Arguments("de-DE")]
    [Arguments("sv-SE")]
    [Arguments("fa-IR")]
    public async Task ConvertNativeTypesIsCultureInvariant(string cultureName) {
        (object? Value, string Expected)[] cases = [
            (null, ""),
            ("Tom's car", "Tom's car"),
            (true, "true"),
            (false, "false"),
            (-1234, "-1234"),
            (-1234567890123L, "-1234567890123"),
            (-1.5f, "-1.5"),
            (-1.5d, "-1.5"),
            (-1.5m, "-1.5"),
            (new TimeOnly(14, 30, 0), "14:30:00"),
            (new DateOnly(2024, 1, 2), "2024-01-02"),
            (new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), "2024-01-02T03:04:05Z"),
            (new DateTime(2024, 1, 2, 3, 4, 5, 123, DateTimeKind.Utc), "2024-01-02T03:04:05.1230000Z"),
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)), "2024-01-02T03:04:05+02:00"),
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, 123, TimeSpan.FromHours(-5)), "2024-01-02T03:04:05.1230000-05:00"),
        ];

        var actual = await Task.Run(() => {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            return cases.Select(c => ODataConvert.ConvertValueToString(c.Value, ODataConvertOptions.Unquoted)).ToArray();
        });

        for (int i = 0; i < cases.Length; i++) {
            await Assert.That(actual[i]).IsEqualTo(cases[i].Expected)
                .Because($"{cultureName} {cases[i].Value?.GetType().Name ?? "null"} {cases[i].Value}");
        }
    }
}
