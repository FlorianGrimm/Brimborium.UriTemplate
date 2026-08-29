// TODO
namespace Brimborium.UriTemplate;

public sealed class UriTemplateAppendValueFailed
    : IUriTemplateAppendValueSelectiv
    , IUriTemplateAppendValueHandler {
    private readonly static UriTemplateAppendValueFailed _Instance = new();
    public static UriTemplateAppendValueFailed Instance => _Instance;

    public AppendValueHandlerResult GetAppendValueHandler(object value) {
        return new(false, true, this);
    }

    public void AppendValue(UriTemplateASTOperation astOperator, string name, object value, int maxChar, bool composite, in UriTemplateTarget target) { }

    public void AddValueText(string? prefix, object? value, int maxChar, bool replaceReserved, in UriTemplateTarget target) { }
}
