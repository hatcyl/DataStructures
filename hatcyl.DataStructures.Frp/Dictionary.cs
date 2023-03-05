using hatcyl.DataStructures.Frp.Utilities;
using Sodium.Frp;
using Sodium.Functional;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hatcyl.DataStructures.Frp;
public class Dictionary<TKey, TValue>
{
    public readonly Cell<IImmutableDictionary<TKey, TValue>> _state;

    public IImmutableDictionary<TKey, TValue> InitialState { get; }
    public Stream<(TKey key, TValue value)> AddStream { get; }
    public Stream<TKey> RemoveStream { get; }

    public Dictionary
    (
        IImmutableDictionary<TKey, TValue> initialState,
        Stream<(TKey key, TValue value)> addStream,
        Stream<TKey> removeStream
    )
    {
        _state = new StateCellBuilder<IImmutableDictionary<TKey, TValue>>(initialState)
        .WithMethod(addStream, value => state => state.Add(value.key, value.value))
        .WithMethod(removeStream, value => state => state.Remove(value))
        .Build();

        InitialState = initialState;
        AddStream = addStream;
        RemoveStream = removeStream;
    }

    public Cell<IEnumerable<KeyValuePair<TKey, TValue>>> Enumerable => _state.Map(state => state.AsEnumerable());

    public Cell<IEnumerable<TValue>> Values => _state.Map(state => state.Values);
}
