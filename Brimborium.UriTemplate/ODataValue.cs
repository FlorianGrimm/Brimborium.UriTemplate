using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Brimborium.UriTemplate;

public interface IODataValue {
    bool AppendValue(ref UriTemplateTarget target);
}


public sealed record class ODataExpression(
    IODataValue Value
    ) : IUriTemplateValue, IODataValue {
    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result
        ) {
        {
            if (this.Value is IUriTemplateValue uriTemplateValue) {
                if (uriTemplateValue.TryGetValue(substitutions, out var nextValue)) {
                    result = new ODataExpressionResolved(nextValue);
                    return true;
                } else {
                    result = default;
                    return false;
                }
            }
        }
        {
            result = this.Value;
            return true;
        }
    }

    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        return this.Value.AppendValue(ref target);
    }

}

public sealed record class ODataExpressionResolved(
    IODataValue Value
    ) : IUriTemplateValue, IODataValue {
    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result
        ) {
        {
            result = this;
            return true;
        }
    }

    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        if (prefix is { Length: > 0 }) { throw new ArgumentException("should be null", nameof(prefix)); }
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        return this.Value.AppendValue(ref target);
    }

}

public sealed record ODataFieldName(string Value) : IUriTemplateValue, IODataValue {
    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result) {
        result = this;
        return true;
    }
    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        var output = target.Output;
        target.Append(this.Value);
        return true;
    }
}

public sealed record ODataRawString(string Value) : IUriTemplateValue, IODataValue {
    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result) {
        result = this;
        return true;
    }
    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        var output = target.Output;
        target.Append(this.Value);
        return true;
    }
}

public sealed record ODataConstant(string Value) : IUriTemplateValue, IODataValue {
    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result) {
        result = this;
        return true;
    }
    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        var output = target.Output;
        target.AddODataValue(this.Value);
        return true;
    }
}

public sealed record ODataSequence(
    string Name,
    IODataValue[] ListItem
    ) : IUriTemplateValue, IODataValue {

    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result
        ) {
        var thisListItem = this.ListItem;
        IODataValue[]? nextListItem = null;
        for (var i = 0; i < thisListItem.Length; i++) {
            if (thisListItem[i] is IUriTemplateValue uriTemplateValue) {
                if (uriTemplateValue.TryGetValue(substitutions, out var nextItem)) {
                    if (ReferenceEquals(uriTemplateValue, nextItem)) {
                        // no change
                    } else {
                        if (nextListItem is null) {
                            nextListItem = thisListItem.ToArray();
                            nextListItem[i] = nextItem;
                        } else {
                            nextListItem[i] = nextItem;
                        }
                    }
                }
            }
        }
        if (nextListItem is null) {
            result = this;
            return true;
        } else {
            result = new ODataSequence(this.Name, nextListItem);
            return true;
        }
    }

    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        bool result = true;
        foreach (var item in ListItem) {
            var subResult = item.AppendValue(ref target);
            if (subResult) {
                // OK
            } else {
                result = false;
            }
        }
        return result;
    }
}

public sealed record ODataList(
    string Name,
    string Seperation,
    IODataValue[] ListItem
    ) : IUriTemplateValue, IODataValue {

    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result
        ) {
        var thisListItem = this.ListItem;
        IODataValue[]? nextListItem = null;
        for (var i = 0; i < thisListItem.Length; i++) {
            if (thisListItem[i] is IUriTemplateValue uriTemplateValue) {
                if (uriTemplateValue.TryGetValue(substitutions, out var nextItem)) {
                    if (ReferenceEquals(uriTemplateValue, nextItem)) {
                        // no change
                    } else {
                        if (nextListItem is null) {
                            nextListItem = thisListItem.ToArray();
                            nextListItem[i] = nextItem;
                        } else {
                            nextListItem[i] = nextItem;
                        }
                    }
                }
            }
        }
        if (nextListItem is null) {
            result = this;
            return true;
        } else {
            result = new ODataList(this.Name, this.Seperation, nextListItem);
            return true;
        }
    }

    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
#if false
        bool result = true;
        bool isSeperatedAppened = true;
        foreach (var item in ListItem) {
            if (isSeperatedAppened) {
                //
            } else { 
                target.Append(this.Seperation);
                isSeperatedAppened = true;
            }
            var subResult = item.AppendValue(target);
            if (subResult) {
                // OK
                isSeperatedAppened = false;
            } else {
                result = false;
            }
        }
        return result;
#endif
        bool result = true;
        int lastSeperate = -1;
        foreach (var item in ListItem) {
            var subResult = item.AppendValue(ref target);
            if (subResult) {
                // OK
                lastSeperate = target.Output.Length;
                target.Append(this.Seperation);
            } else {
                result = false;
            }
        }
        if (0 <= lastSeperate) {
            target.Output.Length = lastSeperate;
        }
        return result;
    }
}

public sealed record ODataOperation(
    IODataValue Left,
    string Name,
    IODataValue Right
    ) : IUriTemplateValue, IODataValue {

    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result) {
        IODataValue nextLeft;
        bool changed = false;
        if (this.Left is IUriTemplateValue uriTemplateValueLeft) {
            if (uriTemplateValueLeft.TryGetValue(substitutions, out var innerLeft)) {
                if (ReferenceEquals(uriTemplateValueLeft, innerLeft)) {
                    nextLeft = innerLeft;
                    // no change
                } else {
                    nextLeft = innerLeft;
                    changed = true;
                }
            } else {
                result = null;
                return false;
            }
        } else {
            nextLeft = this.Left;
        }
        IODataValue nextRight;
        if (this.Right is IUriTemplateValue uriTemplateValueRight) {
            if (uriTemplateValueRight.TryGetValue(substitutions, out var innerRight)) {
                if (ReferenceEquals(uriTemplateValueRight, innerRight)) {
                    nextRight = innerRight;
                    // no change
                } else {
                    nextRight = innerRight;
                    changed = true;
                }
            } else {
                result = null;
                return false;
            }
        } else {
            nextRight = this.Right;
        }
        if (changed) {
            result = new ODataOperation(nextLeft, this.Name, nextRight);
            return true;
        } else {
            result = this;
            return true;
        }
    }

    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        this.AppendValue(ref target);
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        bool result = true;
        var subResultLeft = this.Left.AppendValue(ref target);
        if (!subResultLeft) { result = false; }
        target.Append("%20");
        target.Append(this.Name);
        target.Append("%20");
        var subResultRight = this.Right.AppendValue(ref target);
        if (!subResultRight) { result = false; }
        return result;
    }
}

public sealed record ODataFunction(
    string Name,
    IODataValue[] ListItem
    ) : IUriTemplateValue, IODataValue {

    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result) {
        var thisListItem = this.ListItem;
        IODataValue[]? nextListItem = null;
        for (int i = 0; i < thisListItem.Length; i++) {
            if (thisListItem[i] is IUriTemplateValue uriTemplateValue) {
                if (uriTemplateValue.TryGetValue(substitutions, out var nextItem)) {
                    if (ReferenceEquals(uriTemplateValue, nextItem)) {
                        // no change
                    } else {
                        if (nextListItem is null) {
                            nextListItem = thisListItem.ToArray();
                            nextListItem[i] = nextItem;
                        } else {
                            nextListItem[i] = nextItem;
                        }
                    }
                } else {
                    result = null;
                    return false;
                }
            }
        }
        if (nextListItem is null) {
            result = this;
            return true;
        } else {
            result = new ODataFunction(this.Name, nextListItem);
            return true;
        }
    }

    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        throw new NotImplementedException();
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        bool result = true;
        target.Append(this.Name);
        target.Append('(');
        bool first = true;
        foreach (var item in ListItem) {
            var subResult = item.AppendValue(ref target);
            if (subResult) {
                // OK
            } else {
                result = false;
            }
            if (first) {
                first = false;
            } else {
                target.Append(',');
            }
        }
        target.Append(')');
        return result;
    }
}

public sealed record ODataVariable(
    string Name,
    ODataValue? Value
    ) : IUriTemplateValue, IODataValue {

    public bool TryGetValue(
        IReadOnlyDictionary<string, object?> substitutions,
        [MaybeNullWhen(false)] out IODataValue result) {
        if (substitutions.TryGetValue(this.Name, out var substitutionValue)) {
            result = new ODataValue(substitutionValue);
            return true;
        }

        if (this.Value is { } fallbackValue) {
            result = fallbackValue;
            return true;
        }

        result = null;
        return false;
    }

    public void AppendValue(string? prefix, int maxChar, bool replaceReserved, ref UriTemplateTarget target) {
        throw new NotImplementedException();
    }

    public bool AppendValue(ref UriTemplateTarget target) {
        return false;
    }
}

public sealed record ODataValue(object? Value) : IODataValue {
    public bool AppendValue(ref UriTemplateTarget target) {
        if (this.Value is null) {
            target.Output.Append("null");
            return true;
        } else if (this.Value is string stringValue) {
            target.Output.Append('\'');
            target.AddODataValue(stringValue);
            target.Output.Append('\'');
            return true;
        } else if (this.Value is int intValue) {
            target.Append(intValue.ToString());
            return true;
        } else if (this.Value is long longValue) {
            target.Append(longValue.ToString());
            return true;
        } else if (this.Value is float floatValue) {
            target.Append(floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return true;
        } else if (this.Value is double doubleValue) {
            target.Append(doubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return true;
        } else if (this.Value is decimal decimalValue) {
            target.Append(decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return true;
        } else if (this.Value is DateTimeOffset dateTimeOffsetValue) {
            target.Append($"'{dateTimeOffsetValue:O}'");
            return true;
        } else if (this.Value is DateTime dateTimeValue) {
            target.Append($"'{dateTimeValue:O}'");
            return true;
        } else if (this.Value is DateOnly dateOnlyValue) {
            target.Append($"'{dateOnlyValue:yyyy-MM-dd}'");
            return true;
        } else if (this.Value is TimeOnly timeOnlyValue) {
            target.Append($"'{timeOnlyValue:hh:mm:ss}'");
            return true;
        } else if (this.Value is bool boolValue) {
            if (boolValue) {
                target.Append("true");
            } else {
                target.Append("false");
            }
            return true;
        } else {
            return false;
        }
    }
}