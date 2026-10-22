using System;
using System.Collections.Generic;
using System.Text;

namespace Brimborium.UriTemplate.Test;

public class UriEscapeDataStringTests {
    [Test]
    [Arguments("\r", "%0D")]
    [Arguments("\n", "%0A")]
    [Arguments("\r\n", "%0D%0A")]
    [Arguments("\t", "%09")]
    [Arguments("\0", "%00")]
    [Arguments("\u007F", "%7F")]
    [Arguments(" ", "%20")]
    [Arguments("\"", "%22")]
    [Arguments("<", "%3C")]
    [Arguments(">", "%3E")]
    [Arguments("\\", "%5C")]
    [Arguments("^", "%5E")]
    [Arguments("`", "%60")]
    [Arguments("{", "%7B")]
    [Arguments("|", "%7C")]
    [Arguments("}", "%7D")]
    [Arguments(":", "%3A")]
    [Arguments("/", "%2F")]
    [Arguments("?", "%3F")]
    [Arguments("#", "%23")]
    [Arguments("[", "%5B")]
    [Arguments("]", "%5D")]
    [Arguments("@", "%40")]
    [Arguments("!", "%21")]
    [Arguments("$", "%24")]
    [Arguments("&", "%26")]
    [Arguments("'", "%27")]
    [Arguments("(", "%28")]
    [Arguments(")", "%29")]
    [Arguments("*", "%2A")]
    [Arguments("+", "%2B")]
    [Arguments(",", "%2C")]
    [Arguments(";", "%3B")]
    [Arguments("=", "%3D")]
    public async Task UriEscapeDataStringSepecialChars(string value, string expected) {
        var act=Uri.EscapeDataString(value);
        await Assert.That(act).IsEqualTo(expected);
    }
}
