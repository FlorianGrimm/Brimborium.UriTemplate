namespace Brimborium.UriTemplate.Test;

public class ODataValueTests {
    [Test]
    [Arguments("Hello", "'Hello'")]
    [Arguments("%", "'%25'")]
    [Arguments("Tom's car", "'Tom''s%20car'")]
    [Arguments("Tom & Jerry", "'Tom%20%26%20Jerry'")]
    [Arguments("Tom & Jerry #1 + x", "'Tom%20%26%20Jerry%20%231%20%2B%20x'")]
    public async Task ODataValueStringTest(string value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-124, "-124")]
    public async Task ODataValueIntTest(int value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-124, "-124")]
    public async Task ODataValueLongTest(long value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-1.24, "-1.24")]
    public async Task ODataValueFloatTest(float value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-1.24, "-1.24")]
    public async Task ODataValueDoubleTest(double value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-1.24, "-1.24")]
    public async Task ODataValueDecimalTest(decimal value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1970-01-01+01:00", "'1970-01-01T00:00:00%2B01:00'")]
    [Arguments("3141-05-09+02:00", "'3141-05-09T00:00:00%2B02:00'")]
    [Arguments("3141-05-09T02:06:53+05:00", "'3141-05-09T02:06:53%2B05:00'")]
    [Arguments("3141-05-09T02:06:53.59+06:00", "'3141-05-09T02:06:53.5900000%2B06:00'")]
    [Arguments("3141-05-09T02:06:53-06:00", "'3141-05-09T02:06:53-06:00'")]
    public async Task ODataValueDateTimeOffset(string text, string expected) {
        var value = DateTimeOffset.Parse(text);
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1970-01-01", "'1970-01-01T00:00:00'")]
    [Arguments("3141-05-09", "'3141-05-09T00:00:00'")]
    [Arguments("3141-05-09T02:06:53", "'3141-05-09T02:06:53'")]
    [Arguments("3141-05-09T02:06:53.59", "'3141-05-09T02:06:53.5900000'")]
    public async Task ODataValueDateTime(string text, string expected) {
        var value = DateTime.Parse(text);
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataValueUnquotedDateTimeLiterals() {
        (object Value, string Expected)[] cases = [
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)), "2024-01-02T03:04:05%2B02:00"),
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, 590, TimeSpan.FromHours(-6)), "2024-01-02T03:04:05.5900000-06:00"),
            (new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc), "2024-01-02T03:04:05Z"),
            (new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified), "2024-01-02T03:04:05Z"),
            (new DateTime(2024, 1, 2, 3, 4, 5, 590, DateTimeKind.Unspecified), "2024-01-02T03:04:05.5900000Z"),
            (new DateOnly(2024, 1, 2), "2024-01-02"),
            (new TimeOnly(14, 30, 0), "14:30:00"),
            (new TimeOnly(1, 2, 3, 456), "01:02:03.4560000"),
            ("Tom's car", "'Tom''s%20car'"),
        ];
        foreach (var (value, expected) in cases) {
            StringBuilder output = new();
            UriTemplateTarget target = new(output, ODataConvertOptions.Unquoted);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected).Because(value.GetType().Name);
        }
    }

    [Test]
    public async Task ODataValueUnquotedDateTimeLocalHasOffset() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output, ODataConvertOptions.Unquoted);
        ODataValue sut = new(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local));
        sut.AppendValue(target);
        await Assert.That(output.ToString()).DoesNotContain("'");
        await Assert.That(output.ToString()).Matches(@"^2024-01-02T03:04:05(%2B|-)\d\d:\d\d$");
    }

    [Test]
    public async Task ODataValueDateTimeLocalHasNoRawPlus() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local));
        sut.AppendValue(target);
        await Assert.That(output.ToString()).DoesNotContain("+");
    }

    [Test]
    public async Task ODataValueDateTimeUtc() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo("'2024-01-02T03:04:05Z'");
    }

    [Test]
    [Arguments("1970-01-01", "'1970-01-01'")]
    [Arguments("3141-05-09", "'3141-05-09'")]
    public async Task ODataValueDateOnly(string text, string expected) {
        var value = DateOnly.Parse(text);
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(0, 0, 0, "'00:00:00'")]
    [Arguments(9, 5, 7, "'09:05:07'")]
    [Arguments(14, 30, 0, "'14:30:00'")]
    [Arguments(23, 59, 59, "'23:59:59'")]
    public async Task ODataValueTimeOnly(int hour, int minute, int second, string expected) {
        var value = new TimeOnly(hour, minute, second);
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataValueTimeOnlyKeepsFractionalSeconds() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(new TimeOnly(1, 2, 3, 456));
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo("'01:02:03.4560000'");
    }

    [Test]
    [Arguments(1e20, "1E%2B20")]
    [Arguments(1e-20, "1E-20")]
    [Arguments(double.PositiveInfinity, "INF")]
    [Arguments(double.NegativeInfinity, "-INF")]
    [Arguments(double.NaN, "NaN")]
    public async Task ODataValueDoubleSpecialValues(double value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(1e20f, "1E%2B20")]
    [Arguments(float.PositiveInfinity, "INF")]
    [Arguments(float.NegativeInfinity, "-INF")]
    [Arguments(float.NaN, "NaN")]
    public async Task ODataValueFloatSpecialValues(float value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataValueLoneSurrogateIsReplaced() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new("\uD83D\uDE00\uD800");
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo("'%F0%9F%98%80%EF%BF%BD'");
    }

    [Test]
    public async Task SemicolonOperatorAddsEqualsForODataValues() {
        Brimborium.UriTemplate.UriTemplateCache sut = new();
        _ = await Assert.That(sut.Expand("{;x}", new Dictionary<string, object?> { { "x", new ODataValue("a") } }))
            .IsEqualTo(";x='a'");
        _ = await Assert.That(sut.Expand("{;x}", new Dictionary<string, object?> { { "x", new ODataConstant("a") } }))
            .IsEqualTo(";x=a");
        _ = await Assert.That(sut.Expand("{;x}", new Dictionary<string, object?> { { "x", new ODataExpression(new ODataFieldName("a")) } }))
            .IsEqualTo(";x=a");
    }

    [Test]
    [Arguments(true, "true")]
    [Arguments(false, "false")]
    public async Task ODataValueBool(bool value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        var result = sut.AppendValue(target);
        await Assert.That(result).IsTrue();
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataValueCrLfIsPercentEncoded() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new("a\r\nSet-Cookie: x=1");
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo("'a%0D%0ASet-Cookie%3A%20x%3D1'");
    }

    [Test]
    [Arguments("%27", "'%2527'")]
    [Arguments("%'", "'%25'''")]
    [Arguments("%'a", "'%25''a'")]
    [Arguments("%&x", "'%25%26x'")]
    [Arguments("% x", "'%25%20x'")]
    [Arguments("%#a", "'%25%23a'")]
    [Arguments("%%41", "'%25%2541'")]
    [Arguments("%C3%A9", "'%25C3%25A9'")]
    public async Task ODataValuePercentIsAlwaysEncoded(string value, string expected) {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(value);
        sut.AppendValue(target);
        await Assert.That(output.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataValueNull() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(null);
        var result = sut.AppendValue(target);
        await Assert.That(result).IsTrue();
        await Assert.That(output.ToString()).IsEqualTo("null");
    }

    [Test]
    public async Task ODataValueUnsupportedTypeThrows() {
        StringBuilder output = new();
        UriTemplateTarget target = new(output);
        ODataValue sut = new(Guid.Empty);
        await Assert.That(() => sut.AppendValue(target))
            .Throws<NotSupportedException>();
    }

    [Test]
    [Arguments("th-TH")]
    [Arguments("ar-SA")]
    [Arguments("de-DE")]
    [Arguments("sv-SE")]
    [Arguments("fa-IR")]
    public async Task ODataValueIsCultureInvariant(string cultureName) {
        (object? Value, string Expected)[] cases = [
            (null, "null"),
            ("Tom's car", "'Tom''s%20car'"),
            (-1234, "-1234"),
            (-1234567890123L, "-1234567890123"),
            (-1.5f, "-1.5"),
            (-1.5d, "-1.5"),
            (-1.5m, "-1.5"),
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)), "'2024-01-02T03:04:05%2B02:00'"),
            (new DateTimeOffset(2024, 1, 2, 3, 4, 5, 123, TimeSpan.FromHours(-5)), "'2024-01-02T03:04:05.1230000-05:00'"),
            (new DateTime(2024, 1, 2, 3, 4, 5), "'2024-01-02T03:04:05'"),
            (new DateTime(2024, 1, 2, 3, 4, 5, 123), "'2024-01-02T03:04:05.1230000'"),
            (new DateOnly(2024, 1, 2), "'2024-01-02'"),
            (new TimeOnly(14, 30, 0), "'14:30:00'"),
            (true, "true"),
            (false, "false"),
        ];

        var actual = await Task.Run(() => {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            return cases.Select(c => Format(new ODataValue(c.Value))).ToArray();
        });

        for (int i = 0; i < cases.Length; i++) {
            await Assert.That(actual[i]).IsEqualTo(cases[i].Expected)
                .Because($"{cultureName} {cases[i].Value?.GetType().Name ?? "null"} {cases[i].Value}");
        }

        static string Format(ODataValue value) {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            value.AppendValue(target);
            return output.ToString();
        }
    }

}
