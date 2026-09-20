namespace Brimborium.UriTemplate.Test;

public class UriTemplateTargetTests {
    [Test]
    public async Task SurrogateAstonishedFaceTest() {
        // \u1234\ud800 supplementary value (U+1F632 ASTONISHED FACE)
        {
            Span<char> outputBuffer = stackalloc char[4096];
            UriTemplateTarget target = new(outputBuffer);
            target.AddValueText(null, char.ConvertFromUtf32(0x1F632).ToString(), -1, false);
            var result = target.ToStringAndDispose();
            await Assert.That(result).IsEqualTo("%F0%9F%98%B2");
        }
        {
            Span<char> outputBuffer = stackalloc char[4096];
            UriTemplateTarget target = new(outputBuffer);
            target.AddValueText(null, "\ud83d\ude32", -1, false);
            var result = target.ToStringAndDispose();
            await Assert.That(result).IsEqualTo("%F0%9F%98%B2");
        }
    }
    [Test]
    public async Task ReplaceReservedTest() {
        // \u1234\ud800 supplementary value (U+1F632 ASTONISHED FACE)
        {
            Span<char> outputBuffer = stackalloc char[4096];
            UriTemplateTarget target = new(outputBuffer);
            target.AddValueText(null, "a c", -1, false);
            var result = target.ToStringAndDispose();
            await Assert.That(result).IsEqualTo("a%20c");
        }
        {
            Span<char> outputBuffer = stackalloc char[4096];
            UriTemplateTarget target = new(outputBuffer);
            target.AddValueText(null, "a c", -1, true);
            var result = target.ToStringAndDispose();
            await Assert.That(result).IsEqualTo("a%20c");
        }
        {
            Span<char> outputBuffer = stackalloc char[4096];
            UriTemplateTarget target = new(outputBuffer);
            target.AddValueText(null, "a%20c", -1, false);
            var result = target.ToStringAndDispose();
            await Assert.That(result).IsEqualTo("a%20c");
        }
        {
            Span<char> outputBuffer = stackalloc char[4096];
            UriTemplateTarget target = new(outputBuffer);
            target.AddValueText(null, "a%20c", -1, true);
            var result = target.ToStringAndDispose();
            await Assert.That(result).IsEqualTo("a%2520c");
        }
    }
    
}
