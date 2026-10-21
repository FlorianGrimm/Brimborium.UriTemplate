# Brimborium.UriTemplate

based on Std.UriTemplate

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

## build

```cmd
dotnet build "Brimborium.UriTemplate\Brimborium.UriTemplate.csproj"
```

## Testing

```cmd
dotnet run --project .\Brimborium.UriTemplate.Test\Brimborium.UriTemplate.Test.csproj
```