// TODO
using System.Collections.Immutable;

namespace Brimborium.UriTemplate;

public interface IUriTemplateValue {
    UriTemplateValueResult DistributeAppendValue();
}

public interface IUriTemplateDistributeAppendValue {
    UriTemplateValueResult DistributeAppendValue(object value);
}

public readonly record struct UriTemplateValueResult(bool Matches, bool IsEmpty, IUriTemplateAppendValue AppendValue);

public interface IUriTemplateAppendValue {
    void AppendValue(UriTemplateASTOperation astOperator, string name, object value, int maxChar, bool composite);
}

public sealed class UriTemplateValueCollection : IUriTemplateDistributeAppendValue {
#pragma warning disable IDE0301 // Simplify collection initialization
    private ImmutableArray<IUriTemplateDistributeAppendValue> _Items = ImmutableArray<IUriTemplateDistributeAppendValue>.Empty;
#pragma warning restore IDE0301 // Simplify collection initialization

    public void Add(IUriTemplateDistributeAppendValue distributeAppendValue) {
        this._Items = this._Items.Add(distributeAppendValue);
    }

    public UriTemplateValueResult DistributeAppendValue(object value) {
        foreach (var distributeAppendValue in this._Items) {
            var result = distributeAppendValue.DistributeAppendValue(value);
            if (result.Matches) {
                return result;
            }
        }

        return UriTemplateDistributeAppendValueFailed.Instance.DistributeAppendValue(value);
    }
}

public sealed class UriTemplateDistributeAppendValueFailed 
    : IUriTemplateDistributeAppendValue
    , IUriTemplateAppendValue {
    private readonly static UriTemplateDistributeAppendValueFailed _Instance = new();
    public static UriTemplateDistributeAppendValueFailed Instance => _Instance;

    public UriTemplateValueResult DistributeAppendValue(object value) {
        return new(false, true, this);
    }

    public void AppendValue(UriTemplateASTOperation astOperator, string name, object value, int maxChar, bool composite) {
    }
}