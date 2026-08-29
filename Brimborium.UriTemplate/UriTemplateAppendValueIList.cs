// TODO
namespace Brimborium.UriTemplate;

public sealed class UriTemplateAppendValueIList
    : IUriTemplateAppendValueSelectiv
    , IUriTemplateAppendValueHandler {
    public AppendValueHandlerResult GetAppendValueHandler(object value) {
        if (value is System.Collections.IList) {
            return new(true, false, this);
        } else {
            return new(false, false, UriTemplateAppendValueFailed.Instance);
        }
    }

    public void AppendValue(UriTemplateASTOperation astOperator, string token, object value, int maxChar, bool composite, in UriTemplateTarget target) {
        bool first = true;
        foreach (object innerValue in (System.Collections.IList)value) {
            if (first) {
                target.AddValue(astOperator, token, innerValue, maxChar);
                first = false;
            } else {
                if (composite) {
                    astOperator.AddSeparator(target.Output);
                    target.AddValue(astOperator, token, innerValue, maxChar);
                } else {
                    _ = target.Append(',');
                    target.AddValueElement(astOperator, token, innerValue, maxChar);
                }
            }
        }
        //return !first;
    }

    public void AddValueText(string? prefix, object? value, int maxChar, bool replaceReserved, in UriTemplateTarget target) {
        throw new NotImplementedException();
    }
}
