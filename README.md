# Brimborium.UriTemplate

based on [https://github.com/std-uritemplate/std-uritemplate-csharp](https://github.com/std-uritemplate/std-uritemplate-csharp)

## Syntax

- `{+base}index`- http://example.com/home/index - Path segment expansion, encoded.
- `{base}index` - http%3A%2F%2Fexample.com%2Fhome%2Findex - Path segment expansion, reserved characters are not percent-encoded.
- `{+path}/here` - /foo/bar/here - Path segment expansion, reserved characters are not percent-encoded.

- `{count}` - one,two,three
- `{count*}` - one,two,three
- `{/count}` - /one,two,three
- `{/count*}` - /one/two/three
- `{;count}` - ;count=one,two,three
- `{;count*}` - ;count=one;count=two;count=three
- `{?count}` - ?count=one,two,three
- `{?count*}` - ?count=one&count=two&count=three
- `{&count*}` - &count=one&count=two&count=three

```json
{
    "base"       : "http://example.com/home/",
    "path"       : "/foo/bar",
    "count"      : ["one", "two", "three"]
}
```

## Additional Features

ODataValue sources can be used as UriTemplate parameters values.
Parser produces an AST (Abstract Syntax Tree) of the UriTemplate, which can be cached for reuse. The AST can be used to analyze the UriTemplate, e.g. to determine which parameters are required and which are optional.

## Specifications

- [RFC 6570 - URI Template](https://www.rfc-editor.org/rfc/rfc6570) - template syntax, operators and expansion rules.
- [RFC 3986 - Uniform Resource Identifier (URI): Generic Syntax](https://www.rfc-editor.org/rfc/rfc3986) - reserved / unreserved characters and percent-encoding.
- [RFC 3987 - Internationalized Resource Identifiers (IRIs)](https://www.rfc-editor.org/rfc/rfc3987) - `ucschar` and `iprivate` character ranges.
- [RFC 3629 - UTF-8](https://www.rfc-editor.org/rfc/rfc3629) - encoding of non-ASCII characters before percent-encoding.
- [OData Version 4.01 Part 2: URL Conventions](https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part2-url-conventions.html) (OASIS standard) - OData query options and URL rules.
- [OData ABNF Construction Rules Version 4.01](https://docs.oasis-open.org/odata/odata/v4.01/os/abnf/odata-abnf-construction-rules.txt) (OASIS standard) - literal syntax for OData values (strings, numbers, dates and times).

## build

```cmd
dotnet build "Brimborium.UriTemplate\Brimborium.UriTemplate.csproj"
```

## Testing

```cmd
dotnet run --project .\Brimborium.UriTemplate.Test\Brimborium.UriTemplate.Test.csproj
```
