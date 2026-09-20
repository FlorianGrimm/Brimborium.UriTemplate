#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable IDE1006 // Naming Styles

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable enable

namespace System.Text;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public ref partial struct ValueStringBuilder {
    private char[]? _ArrayToReturnToPool;
    private Span<char> _SpanChars;
    private int _Pos;

    public ValueStringBuilder(Span<char> initialBuffer) {
        this._ArrayToReturnToPool = null;
        this._SpanChars = initialBuffer;
        this._Pos = 0;
    }

    public ValueStringBuilder(int initialCapacity) {
        this._ArrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
        this._SpanChars = this._ArrayToReturnToPool;
        this._Pos = 0;
    }

    public int Length {
        get => this._Pos;
        set {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= this._SpanChars.Length);
            this._Pos = value;
        }
    }

    public void Clear() {
        this._Pos = 0;
    }

    public string ToStringAndClear() {
        string result = this._SpanChars.Slice(0, this._Pos).ToString();
        this._Pos = 0;
        return result;
    }

    public int Capacity => this._SpanChars.Length;

    public void EnsureCapacity(int capacity) {
        // This is not expected to be called this with negative capacity
        Debug.Assert(capacity >= 0);

        // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
        if ((uint)capacity > (uint)this._SpanChars.Length) {
            this.Grow(capacity - this._Pos);
        }
    }

    /// <summary>
    /// Ensures that the builder is terminated with a NUL character.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NullTerminate() {
        this.EnsureCapacity(this._Pos + 1);
        this._SpanChars[this._Pos] = '\0';
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length"/>
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)"
    /// </summary>
    public ref char GetPinnableReference() {
        return ref MemoryMarshal.GetReference(this._SpanChars);
    }

    public ref char this[int index] {
        get {
            Debug.Assert(index < this._Pos);
            return ref this._SpanChars[index];
        }
    }

    // ToString() clears the builder, so we need a side-effect free debugger display.
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => this.AsSpan().ToString();

    public override string ToString() {
        string result = this._SpanChars.Slice(0, this._Pos).ToString();
        return result;
    }

    public string ToStringAndDispose() {
        string result = this._SpanChars.Slice(0, this._Pos).ToString();
        this.Dispose();
        return result;
    }

    /// <summary>Returns the underlying storage of the builder.</summary>
    public Span<char> RawChars => this._SpanChars;

    public ReadOnlySpan<char> AsSpan() => this._SpanChars.Slice(0, this._Pos);
    public ReadOnlySpan<char> AsSpan(int start) => this._SpanChars.Slice(start, this._Pos - start);
    public ReadOnlySpan<char> AsSpan(int start, int length) => this._SpanChars.Slice(start, length);

    public void Insert(int index, char value, int count) {
        if (this._Pos > this._SpanChars.Length - count) {
            this.Grow(count);
        }

        int remaining = this._Pos - index;
        this._SpanChars.Slice(index, remaining).CopyTo(this._SpanChars.Slice(index + count));
        this._SpanChars.Slice(index, count).Fill(value);
        this._Pos += count;
    }

    public void Insert(int index, string? s) {
        if (s == null) {
            return;
        }

        int count = s.Length;

        if (this._Pos > (this._SpanChars.Length - count)) {
            this.Grow(count);
        }

        int remaining = this._Pos - index;
        this._SpanChars.Slice(index, remaining).CopyTo(this._SpanChars.Slice(index + count));
        s
#if !NET
                .AsSpan()
#endif
            .CopyTo(this._SpanChars.Slice(index));
        this._Pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c) {
        int pos = this._Pos;
        Span<char> chars = this._SpanChars;
        if ((uint)pos < (uint)chars.Length) {
            chars[pos] = c;
            this._Pos = pos + 1;
        } else {
            this.GrowAndAppend(c);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s) {
        if (s == null) {
            return;
        }

        int pos = this._Pos;
        if (s.Length == 1 && (uint)pos < (uint)this._SpanChars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        {
            this._SpanChars[pos] = s[0];
            this._Pos = pos + 1;
        } else {
            this.AppendSlow(s);
        }
    }

    private void AppendSlow(string s) {
        int pos = this._Pos;
        if (pos > this._SpanChars.Length - s.Length) {
            this.Grow(s.Length);
        }

        s.AsSpan().CopyTo(this._SpanChars.Slice(pos));
        this._Pos += s.Length;
    }

    public void Append(char c, int count) {
        if (this._Pos > this._SpanChars.Length - count) {
            this.Grow(count);
        }

        Span<char> dst = this._SpanChars.Slice(this._Pos, count);
        for (int i = 0; i < dst.Length; i++) {
            dst[i] = c;
        }
        this._Pos += count;
    }

    public void Append(scoped ReadOnlySpan<char> value) {
        int pos = this._Pos;
        if (pos > this._SpanChars.Length - value.Length) {
            this.Grow(value.Length);
        }

        value.CopyTo(this._SpanChars.Slice(this._Pos));
        this._Pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length) {
        int origPos = this._Pos;
        if (origPos > this._SpanChars.Length - length) {
            this.Grow(length);
        }

        this._Pos = origPos + length;
        return this._SpanChars.Slice(origPos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c) {
        this.Grow(1);
        this.Append(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_Pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos) {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(this._Pos > this._SpanChars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
        // to double the size if possible, bounding the doubling to not go beyond the max array length.
        int newCapacity = (int)Math.Max(
            (uint)(this._Pos + additionalCapacityBeyondPos),
            Math.Min((uint)this._SpanChars.Length * 2, ArrayMaxLength));

        // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
        // This could also go negative if the actual required length wraps around.
        char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

        this._SpanChars.Slice(0, this._Pos).CopyTo(poolArray);

        char[]? toReturn = this._ArrayToReturnToPool;
        this._SpanChars = this._ArrayToReturnToPool = poolArray;
        if (toReturn != null) {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {
        char[]? toReturn = this._ArrayToReturnToPool;
        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
        if (toReturn != null) {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    internal void AppendSpanFormattable<T>(T value, string? format = null, IFormatProvider? provider = null) where T : ISpanFormattable {
        if (value.TryFormat(this._SpanChars.Slice(this._Pos), out int charsWritten, format, provider)) {
            this._Pos += charsWritten;
        } else {
            this.Append(value.ToString(format, provider));
        }
    }
}
