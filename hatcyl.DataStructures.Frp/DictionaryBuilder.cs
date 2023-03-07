using Sodium.Frp;
using System.Collections.Immutable;
using Stream = Sodium.Frp.Stream;

namespace hatcyl.DataStructures.Frp;
public record class DictionaryBuilder<TKey, TValue>
{
    public IImmutableDictionary<TKey, TValue> InitialState { get; init;  } = ImmutableDictionary<TKey, TValue>.Empty;
    public Stream<(TKey key, TValue value)> AddStream { get; init; } = Stream.Never<(TKey key, TValue value)>();
    public Stream<TKey> RemoveStream { get; init; } = Stream.Never<TKey>();

    public Dictionary<TKey, TValue> Build() => new
    (
        InitialState,
        AddStream,
        RemoveStream
    );
}