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
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(9223372036854775807L)).IsEqualTo("9223372036854775807");
    }

    [Test]
    public async Task ConvertNativeTypesFloatTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(1.5f)).IsEqualTo("1.5");
    }

    [Test]
    public async Task ConvertNativeTypesDoubleTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(1.25d)).IsEqualTo("1.25");
    }

    [Test]
    public async Task ConvertNativeTypesDecimalTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(12.34m)).IsEqualTo("12.34");
    }

    [Test]
    public async Task ConvertNativeTypesTimeOnlyTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new TimeOnly(9, 5, 7))).IsEqualTo("09:05:07");
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new TimeOnly(19, 5, 7))).IsEqualTo("19:05:07");
    }

    [Test]
    public async Task ConvertNativeTypesDateOnlyTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new DateOnly(2024, 1, 2))).IsEqualTo("2024-01-02");
    }

    [Test]
    public async Task ConvertNativeTypesDateTimeTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc))).IsEqualTo("2024-01-02T03:04:05Z");
    }

    [Test]
    public async Task ConvertNativeTypesDateTimeOffsetTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(2)))).IsEqualTo("2024-01-02T03:04:05+02:00");
    }

    [Test]
    public async Task ConvertNativeTypesUnsupportedTypeTest() {
        await Assert.That(UriTemplateTarget.ConvertNativeTypes(new object())).IsNull();
    }
}
