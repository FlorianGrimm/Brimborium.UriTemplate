// TODO
namespace Brimborium.UriTemplate;

public sealed class UriTemplateAppendValueNative
    : IUriTemplateAppendValueSelectiv
    , IUriTemplateAppendValueHandler {
    public AppendValueHandlerResult GetAppendValueHandler(object value) {
        if (value is string or bool or int or long or float or double or decimal) {
            return new(true, false, this);
        } else {
            return new(false, false, UriTemplateAppendValueFailed.Instance);
        }
    }

    public void AppendValue(UriTemplateASTOperation astOperator, string name, object value, int maxChar, bool composite, in UriTemplateTarget target) {
        target.AddValue(astOperator, name, value, maxChar);
    }

    public void AddValueText(string? prefix, object? value, int maxChar, bool replaceReserved, in UriTemplateTarget target) {
        string stringValue = value switch {
            null => string.Empty,
            string strValue => strValue,
            bool boolValue => boolValue ? "true" : "false",
            int number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            long number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            float number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            decimal number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            //TODO: or TimeOnly or DateOnly or DateTime or DateTimeOffset
            _ => throw new ArgumentException($"Illegal class passed as substitution, found {value.GetType()}"),
        };

        target.AddValueText(prefix, stringValue, maxChar, replaceReserved);
    }
}
