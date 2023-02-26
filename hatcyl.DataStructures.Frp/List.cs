using hatcyl.DataStructures.Frp.Utilities;
using Sodium.Frp;
using System.Collections.Immutable;

namespace hatcyl.DataStructures.Frp;
public class List<T>
{
    private readonly Cell<IImmutableList<T>> _state;

    public List
    (
        IImmutableList<T> initialState,
        Stream<T> addStream,
        Stream<IEnumerable<T>> addRangeStream,
        Stream<T> removeStream,
        Stream<(int index, T value)> setItemStream
    )
    {
        _state = new StateCellBuilder<IImmutableList<T>>(initialState)
        .WithMethod(addStream, value => state => state.Add(value))
        .WithMethod(addRangeStream, value => state => state.AddRange(value))
        .WithMethod(removeStream, value => state => state.Remove(value))
        .WithMethod(setItemStream, values => state => state.SetItem(values.index, values.value))
        .Build();
    }

    public Cell<T?> this[int index] => _state.Map(state => state.Count > index ? state[index] : default).Calm();
    public Cell<IEnumerable<T>> Enumerable => _state.Map(state => state.AsEnumerable());
}