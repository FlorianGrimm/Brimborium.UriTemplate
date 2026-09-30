using System.Diagnostics.CodeAnalysis;

namespace Brimborium.UriTemplate;

public interface IUriTemplateValue {
    bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result);

    void AppendValue(string? prefix, int maxChar, bool replaceReserved, in UriTemplateTarget target);
}
