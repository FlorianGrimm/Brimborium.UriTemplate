// TODO
using System.Collections;

namespace Brimborium.UriTemplate;

public sealed class UriTemplateAppendValueIDictionary
    : IUriTemplateAppendValueSelectiv
    , IUriTemplateAppendValueHandler {
    public AppendValueHandlerResult GetAppendValueHandler(object value) {
        if (value is System.Collections.IDictionary) {
            return new(true, false, this);
        } else {
            return new(false, false, UriTemplateAppendValueFailed.Instance);
        }
    }

    public void AppendValue(UriTemplateASTOperation astOperator, string token, object value, int maxChar, bool composite, in UriTemplateTarget target) {
        bool first = true;
        if (maxChar != -1) {
            throw new ArgumentException("Value trimming is not allowed on Dictionaries");
        }
        foreach (DictionaryEntry v in (IDictionary)value) {
            if (composite) {
                if (!first) {
                    astOperator.AddSeparator(target.Output);
                }
                target.AddValueElement(astOperator, token, (string)v.Key, maxChar);
                _ = target.Append('=');
            } else {
                if (first) {
                    target.AddValue(astOperator, token, (string)v.Key, maxChar);
                } else {
                    _ = target.Append(',');
                    target.AddValueElement(astOperator, token, (string)v.Key, maxChar);
                }
                _ = target.Append(',');
            }
            target.AddValueElement(astOperator, token, v.Value, maxChar);
            first = false;
        }
        //return !first;
    }

    public void AddValueText(string? prefix, object? value, int maxChar, bool replaceReserved, in UriTemplateTarget target) {
        throw new NotImplementedException();
    }
}
