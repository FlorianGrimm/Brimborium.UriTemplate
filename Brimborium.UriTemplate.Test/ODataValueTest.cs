namespace Brimborium.UriTemplate.Test;

public class ODataValueTest {
    [Test]
    [Arguments("Hello", "'Hello'")]
    [Arguments("%", "'%25'")]
    [Arguments("Tom's car", "'Tom''s%20car'")]
    public async Task ODataValueStringTest(string value, string expected) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-124, "-124")]
    public async Task ODataValueIntTest(int value, string expected) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-124, "-124")]
    public async Task ODataValueLongTest(long value, string expected) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-1.24, "-1.24")]
    public async Task ODataValueFloatTest(float value, string expected) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-1.24, "-1.24")]
    public async Task ODataValueDoubleTest(double value, string expected) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments(0, "0")]
    [Arguments(1, "1")]
    [Arguments(42, "42")]
    [Arguments(-1.24, "-1.24")]
    public async Task ODataValueDecimalTest(decimal value, string expected) {
        {
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments("1970-01-01+01:00", "'1970-01-01T00:00:00.0000000+01:00'")]
    [Arguments("3141-05-09+02:00", "'3141-05-09T00:00:00.0000000+02:00'")]
    [Arguments("3141-05-09T02:06:53+05:00", "'3141-05-09T02:06:53.0000000+05:00'")]
    [Arguments("3141-05-09T02:06:53.59+06:00", "'3141-05-09T02:06:53.5900000+06:00'")]
    public async Task ODataValueDateTimeOffset(string text, string expected) {
        {
            var value = DateTimeOffset.Parse(text);
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments("1970-01-01", "'1970-01-01T00:00:00.0000000'")]
    [Arguments("3141-05-09", "'3141-05-09T00:00:00.0000000'")]
    [Arguments("3141-05-09T02:06:53", "'3141-05-09T02:06:53.0000000'")]
    [Arguments("3141-05-09T02:06:53.59", "'3141-05-09T02:06:53.5900000'")]
    public async Task ODataValueDateTime(string text, string expected) {
        {
            var value = DateTime.Parse(text);
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments("1970-01-01", "'1970-01-01'")]
    [Arguments("3141-05-09", "'3141-05-09'")]
    public async Task ODataValueDateOnly(string text, string expected) {
        {
            var value = DateOnly.Parse(text);
            StringBuilder output = new();
            UriTemplateTarget target = new(output);
            ODataValue sut = new(value);
            sut.AppendValue(target);
            await Assert.That(output.ToString()).IsEqualTo(expected);
        }
    }
    [Test]
    public async Task ODataSequenceEmpty() {
        ODataSequence greeeting = new(
            "greeeting",
            [
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "greeeting", greeeting},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{#greeeting}";
        string expected = "https://server/abc/def/ghi#";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataSequence1() {
        ODataSequence greeeting = new(
            "greeeting",
            [
                new ODataRawString("caption1")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "greeeting", greeeting},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{#greeeting}";
        string expected = "https://server/abc/def/ghi#caption1";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataSequence2() {
        ODataSequence greeeting = new(
            "greeeting",
            [
                new ODataRawString("caption1"),
                new ODataRawString("title2")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "greeeting", greeeting},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{#greeeting}";
        string expected = "https://server/abc/def/ghi#caption1title2";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataListEmpty() {
        ODataList select = new(
            "$select",
            ",",
            [
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$select", select},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        string expected = "https://server/abc/def/ghi?$select=";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataList1() {
        ODataList select = new(
            "$select",
            ",",
            [
                new ODataFieldName("columnABC")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$select", select},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        string expected = "https://server/abc/def/ghi?$select=columnABC";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    public async Task ODataList2() {
        ODataList select = new(
            "$select",
            ",",
            [
                new ODataFieldName("columnABC"),
                new ODataFieldName("columnDEF")
            ]);

        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$select", select},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        string expected = "https://server/abc/def/ghi?$select=columnABC,columnDEF";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }

    [Test]
    [Arguments("bbb", "eq", "World's Hello", "https://server/abc/def/ghi?$filter=bbb%20eq%20'World''s%20Hello'")]
    [Arguments("ccc", "eq", "c'd", "https://server/abc/def/ghi?$filter=ccc%20eq%20'c''d'")]
    public async Task OdataFilter(
        string propertyName,
        string operation,
        string value,
        string expected
        ) {
        ODataOperation filter = new(
            new ODataConstant(propertyName),
            operation,
            new ODataValue(value));
        IReadOnlyDictionary<string, object?> substitutions = new Dictionary<string, object?> {
            { "baseurl", "https://server/abc" },
            { "$filter", filter},
            { "aaa", "AA1" },
            { "bbb", "BB2" },
            { "ccc", "CC3" },
        };
        Brimborium.UriTemplate.UriTemplateCache sut = new();

        string template = "{+baseurl}/def/ghi{?$select,$filter}";
        var actStd = sut.Expand(template, substitutions);
        _ = await Assert.That(actStd).IsEqualTo(expected);
    }
}