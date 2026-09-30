namespace Brimborium.UriTemplate;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage()]
public static class UriTemplate {
    public static string Expand(
        string template,
        IReadOnlyDictionary<string, object?> substitutions)
        => UriTemplateCache.Default.Expand(template, substitutions);

    public static UriTemplateASTSequence Parse(
        string template)
        => UriTemplateCache.Default.Parse(template);
}
