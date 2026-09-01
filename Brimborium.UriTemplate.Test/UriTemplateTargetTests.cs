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
    
}
