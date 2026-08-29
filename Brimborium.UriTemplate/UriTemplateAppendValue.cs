// TODO
using System.Collections.Immutable;

namespace Brimborium.UriTemplate;

public interface IUriTemplateValue {
    AppendValueHandlerResult DistributeAppendValue();
}

public interface IUriTemplateAppendValueSelectiv {
    AppendValueHandlerResult GetAppendValueHandler(object value);
}

public readonly record struct AppendValueHandlerResult(bool Matches, bool IsEmpty, IUriTemplateAppendValueHandler Handler);

public interface IUriTemplateAppendValueHandler {
    void AppendValue(
        UriTemplateASTOperation astOperator,
        string name,
        object value,
        int maxChar,
        bool composite,
        in UriTemplateTarget target);

    void AddValueText(
        string? prefix,
        object? value,
        int maxChar,
        bool replaceReserved,
        in UriTemplateTarget target);
}

public sealed class UriTemplateValueSelector : IUriTemplateAppendValueSelectiv {
    private static UriTemplateValueSelector? _Instance;

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public static UriTemplateValueSelector Instance {
        get {
            return (_Instance ??= CreateDefault());
        }
        set { _Instance = value; }
    }

#pragma warning disable IDE0301 // Simplify collection initialization
    private ImmutableArray<IUriTemplateAppendValueSelectiv> _Items = ImmutableArray<IUriTemplateAppendValueSelectiv>.Empty;
    private IUriTemplateAppendValueSelectiv[] _List = Array.Empty<IUriTemplateAppendValueSelectiv>();
#pragma warning restore IDE0301 // Simplify collection initialization

    public ImmutableArray<IUriTemplateAppendValueSelectiv> Items {
        get {
            return this._Items;
        }

        set {
            this._Items = value;
            this._List = this._Items.ToArray();
        }
    }

    public static UriTemplateValueSelector CreateDefault() {
        UriTemplateValueSelector result = new();
        result.Add(new UriTemplateAppendValueNative());
        result.Add(new UriTemplateAppendValueIList());
        result.Add(new UriTemplateAppendValueIDictionary());
        return result;
    }

    public void Add(IUriTemplateAppendValueSelectiv distributeAppendValue) {
        this._Items = this._Items.Add(distributeAppendValue);
        this._List = this._Items.ToArray();
    }

    public AppendValueHandlerResult GetAppendValueHandler(object value) {
        foreach (var distributeAppendValue in this._List) {
            var result = distributeAppendValue.GetAppendValueHandler(value);
            if (result.Matches) {
                return result;
            }
        }
        return UriTemplateAppendValueFailed.Instance.GetAppendValueHandler(value);
    }
}
