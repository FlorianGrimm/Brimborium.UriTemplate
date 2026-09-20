using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace Brimborium.UriTemplate;

public ref struct UriTemplateTarget {
    public UriTemplateTarget(int initialCapacity) {
        this.Output = new ValueStringBuilder(initialCapacity);
    }
    public UriTemplateTarget(
        Span<char> outputBuffer
        ) {
        this.Output = new ValueStringBuilder(outputBuffer);
    }
    public ValueStringBuilder Output;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char value) => this.Output.Append(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string value) => this.Output.Append(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNativeType(object value) => value is string or bool or int or long or float or double or decimal;
    //TODO: or TimeOnly or DateOnly or DateTime or DateTimeOffset

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ConvertNativeTypes(object? value) {
        return value switch {
            null => string.Empty,
            string strValue => strValue,
            bool boolValue => boolValue ? "true" : "false",
            int number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            long number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            float number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            decimal number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
            //TODO: or TimeOnly or DateOnly or DateTime or DateTimeOffset
            _ => null
            //_ => throw new ArgumentException($"Illegal class passed as substitution, found {value.GetType()}"),
        };
    }

    public bool AddListValue(UriTemplateASTOperation astOperator, string token, IList value, int maxChar, bool composite) {
        bool first = true;
        foreach (object innerValue in value) {
            if (first) {
                this.AddScalarValue(astOperator, token, innerValue, maxChar);
                first = false;
            } else {
                if (composite) {
                    astOperator.AddSeparator(ref this.Output);
                    this.AddScalarValue(astOperator, token, innerValue, maxChar);
                } else {
                    this.Output.Append(',');
                    this.AddValueElement(astOperator, token, innerValue, maxChar);
                }
            }
        }
        return !first;
    }

    public bool AddDictionaryValue(UriTemplateASTOperation astOperator, string token, IDictionary value, int maxChar, bool composite) {
        bool first = true;
        if (maxChar != -1) {
            throw new ArgumentException("Value trimming is not allowed on Dictionaries");
        }
        foreach (DictionaryEntry v in value) {
            if (composite) {
                if (!first) {
                    astOperator.AddSeparator(ref this.Output);
                }
                this.AddValueElement(astOperator, token, (string)v.Key, maxChar);
                this.Output.Append('=');
            } else {
                if (first) {
                    this.AddScalarValue(astOperator, token, (string)v.Key, maxChar);
                } else {
                    this.Output.Append(',');
                    this.AddValueElement(astOperator, token, (string)v.Key, maxChar);
                }
                this.Output.Append(',');
            }
            this.AddValueElement(astOperator, token, v.Value, maxChar);
            first = false;
        }
        return !first;
    }

    public void AddScalarValue(UriTemplateASTOperation astOperator, string token, object value, int maxChar) {
        if (astOperator.Operator is { } op) {
            switch (op) {
                case UriTemplateASTOperator.Plus:
                case UriTemplateASTOperator.Hash:
                    this.AddValueInner(null, value, maxChar, false);
                    return;
                case UriTemplateASTOperator.Questionmark:
                case UriTemplateASTOperator.Ampersand:
                    this.Output.Append(token);
                    this.Output.Append('=');
                    this.AddValueInner(null, value, maxChar, true);
                    return;
                case UriTemplateASTOperator.Semicolon:
                    this.Output.Append(token);
                    this.AddValueInner("=", value, maxChar, true);
                    return;
                case UriTemplateASTOperator.Dot:
                case UriTemplateASTOperator.Slash:
                case UriTemplateASTOperator.Noop:
                    this.AddValueInner(null, value, maxChar, true);
                    return;
            }
        }
    }

    public void AddValueElement(UriTemplateASTOperation astOperator, string token, object? value, int maxChar) {
        if (astOperator.Operator is { } op) {
            switch (op) {
                case UriTemplateASTOperator.Plus:
                case UriTemplateASTOperator.Hash:
                    this.AddValueInner(null, value, maxChar, false);
                    return;
                case UriTemplateASTOperator.Questionmark:
                case UriTemplateASTOperator.Ampersand:
                case UriTemplateASTOperator.Semicolon:
                case UriTemplateASTOperator.Dot:
                case UriTemplateASTOperator.Slash:
                case UriTemplateASTOperator.Noop:
                    this.AddValueInner(null, value, maxChar, true);
                    return;
            }
        }
    }

    public void AddValueInner(string? prefix, object? value, int maxChar, bool replaceReserved) {
        if (value is null) {
            this.AddValueText(prefix, string.Empty, maxChar, replaceReserved);
            return;
        }

        {
            var stringValue = ConvertNativeTypes(value);
            if (stringValue is { }) {
                this.AddValueText(prefix, stringValue, maxChar, replaceReserved);
            } else if (value is IUriTemplateValue uriTemplateValue) {
                uriTemplateValue.AppendValue(prefix, maxChar, replaceReserved, ref this);
            } else {
                throw new ArgumentException($"Illegal class passed as substitution, found {value.GetType()}");
            }
        }

    }

    public void AddValueText(string? prefix, string stringValue, int maxChar, bool replaceReserved) {
        Span<char> spanReservedBuffer = stackalloc char[8];
        using ValueStringBuilder reservedBuffer = new ValueStringBuilder(spanReservedBuffer);
        int codePointCount = 0;
        bool foundSurrogate = false;
        for (int ci = 0; ci < stringValue.Length; ci++) {
            if (char.IsHighSurrogate(stringValue[ci])
                && (ci + 1 < stringValue.Length)
                && char.IsLowSurrogate(stringValue[ci + 1])) {
                ci++;
                foundSurrogate = true;
            }
            codePointCount++;
        }
        int max = (maxChar != -1) ? Math.Min(maxChar, codePointCount) : codePointCount;
        this.Output.EnsureCapacity(this.Output.Length + max * 2); // hint to SB
        bool toReserved = false;

        if (max > 0 && prefix != null) {
            this.Append(prefix);
        }

        int charCount = 0;
        for (int pos = 0; pos < stringValue.Length && charCount < max; pos++) {
            char character = stringValue[pos];

            if (character == '%' && !replaceReserved) {
                reservedBuffer.Clear();
                toReserved = true;
            }

            string toAppend;
            if (foundSurrogate && IsSurrogate(character)) {
                toAppend = Uri.EscapeDataString(char.ConvertFromUtf32(char.ConvertToUtf32(stringValue, pos)));
                pos++; // skip the low surrogate
            } else if (replaceReserved || IsUcschar(character) || IsIprivate(character)) {
                toAppend = Uri.EscapeDataString(character.ToString());
            } else {
                if (toReserved) {
                    toAppend = character.ToString();
                } else {
                    if (character == ' ') {
                        this.Output.Append("%20");
                    } else if (character == '%') {
                        this.Output.Append("%25");
                    } else {
                        this.Output.Append(character);
                    }
                    continue;
                }
            }

            if (toReserved) {
                reservedBuffer.Append(toAppend);

                if (reservedBuffer.Length == 3) {
                    bool isEncoded = false;

                    var spanOriginal = reservedBuffer.AsSpan();
                    try {
                        isEncoded = spanOriginal.Contains('%') && !spanOriginal.SequenceEqual(Uri.UnescapeDataString(spanOriginal));
                    } catch (Exception) {
                        // ignore
                    }

                    if (isEncoded) {
                        this.Output.Append(spanOriginal);
                    } else {
                        this.Output.Append("%25");
                        // only if !replaceReserved
                        this.Output.Append(spanOriginal.Slice(1, 2));
                    }
                    reservedBuffer.Clear();
                    toReserved = false;
                }
            } else {
                if (character == ' ') {
                    this.Output.Append("%20");
                } else if (character == '%') {
                    this.Output.Append("%25");
                } else {
                    this.Output.Append(toAppend);
                }
            }

            charCount++;
        }

        if (toReserved) {
            this.Output.Append("%25");
            this.Output.Append(reservedBuffer.AsSpan(1, reservedBuffer.Length - 1));
        }
    }

    public void AddODataValue(string stringValue) {

        Span<char> reservedBufferBuffer = stackalloc char[1024];
        using ValueStringBuilder reservedBuffer = new(reservedBufferBuffer);

        int codePointCount = 0;
        bool foundSurrogate = false;
        for (int ci = 0; ci < stringValue.Length; ci++) {
            if (char.IsHighSurrogate(stringValue[ci])
                && (ci + 1 < stringValue.Length)
                && char.IsLowSurrogate(stringValue[ci + 1])) {
                ci++;
                foundSurrogate = true;
            }
            codePointCount++;
        }
        int max = codePointCount;
        this.Output.EnsureCapacity(this.Output.Length + max * 2); // hint to SB
        bool toReserved = false;

        int charCount = 0;
        for (int pos = 0; pos < stringValue.Length && charCount < max; pos++) {
            char character = stringValue[pos];

            if (character == '%') {
                reservedBuffer.Clear();
                toReserved = true;
            }

            string toAppend;
            if (foundSurrogate && IsSurrogate(character)) {
                toAppend = Uri.EscapeDataString(char.ConvertFromUtf32(char.ConvertToUtf32(stringValue, pos)));
                pos++; // skip the low surrogate
            } else if (IsUcschar(character) || IsIprivate(character)) {
                toAppend = Uri.EscapeDataString(character.ToString());
            } else {
                if (toReserved) {
                    toAppend = character.ToString();
                } else {
                    if (character == ' ') {
                        this.Output.Append("%20");
                    } else if (character == '%') {
                        this.Output.Append("%25");
                    } else if (character == '\'') {
                        this.Output.Append("\'\'");
                    } else {
                        this.Output.Append(character);
                    }
                    continue;
                }
            }

            if (toReserved) {
                reservedBuffer.Append(toAppend);

                if (reservedBuffer.Length == 3) {
                    bool isEncoded = false;

                    var original = reservedBuffer.ToStringAndClear();
                    try {
                        isEncoded = !original.Equals(Uri.UnescapeDataString(original));
                    } catch (Exception) {
                        // ignore
                    }

                    if (isEncoded) {
                        this.Output.Append(original);
                    } else {
                        this.Output.Append("%25");
                        // only if !replaceReserved
                        this.Output.Append(original.AsSpan(1, 2));
                    }
                    toReserved = false;
                }
            } else {
                if (character == ' ') {
                    this.Output.Append("%20");
                } else if (character == '%') {
                    this.Output.Append("%25");
                } else if (character == '\'') {
                    this.Output.Append("\'\'");
                } else {
                    this.Output.Append(toAppend);
                }
            }

            charCount++;
        }

        if (toReserved) {
            this.Output.Append("%25");
            this.Output.Append(reservedBuffer.AsSpan(1, reservedBuffer.Length - 1));
        }
    }

    public override string ToString()
        => this.Output.ToString();

    public string ToStringAndDispose()
        => this.Output.ToStringAndDispose();


    private static bool IsSurrogate(char cp)
        => (cp >= 0xD800 && cp <= 0xDFFF);

    private static bool IsIprivate(char cp)
        => (0xE000 <= cp && cp <= 0xF8FF);

    private static bool IsUcschar(char cp)
        => (0xA0 <= cp && cp <= 0xD7FF)
        || (0xF900 <= cp && cp <= 0xFDCF)
        || (0xFDF0 <= cp && cp <= 0xFFEF);
}
