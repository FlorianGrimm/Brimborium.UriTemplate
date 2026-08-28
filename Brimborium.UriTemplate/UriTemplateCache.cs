using Microsoft.Extensions.ObjectPool;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Brimborium.UriTemplate;

public sealed class UriTemplateCache {
    private static UriTemplateCache? _Default;

    [AllowNull]
    public static UriTemplateCache Default {
        get {
            {
                if (_Default is { } result) {
                    return result;
                }
            }
            {
                lock (typeof(UriTemplateCache)) {
                    return _Default ??= new();
                }
            }
        }
        set {
            _Default = value;
        }
    }

    private readonly ConcurrentDictionary<string, UriTemplateASTSequence> _CachedItems = new();

    private readonly DefaultObjectPool<StringBuilder> _StringBuilderPool;

    public UriTemplateCache() {
        var policy = new StringBuilderPooledObjectPolicy {
            InitialCapacity = 4 * 1024,
            MaximumRetainedCapacity = 16 * 1024,
        };
        this._StringBuilderPool = new DefaultObjectPool<StringBuilder>(policy);
    }

    public string Expand(
            string template,
            IReadOnlyDictionary<string, object?> substitutions
        ) {
        var output = this._StringBuilderPool.Get();
        this.Parse(template).Expand(substitutions, output);
        var result = output.ToStringAndClear();
        this._StringBuilderPool.Return(output);
        return result;
    }

    public UriTemplateASTSequence Parse(
        string template
        ) {
        if (this._CachedItems.TryGetValue(template, out var result)) {
            return result;
        } else {
            result = UriTemplateParser.Parse(template);
            _ = this._CachedItems.TryAdd(template, result);
            return result;
        }
    }
}