using hatcyl.DataStructures.Frp.Utilities;
using Sodium.Frp;
using System.Collections.Immutable;

namespace hatcyl.DataStructures.Frp;
public class List<T>
{
    private readonly Cell<IImmutableList<T>> _state;

    public IImmutableList<T> InitialState { get; }
    public Stream<T> AddStream { get; }
    public Stream<IEnumerable<T>> AddRangeStream { get; }
    public Stream<T> RemoveStream { get; }
    public Stream<(int index, T value)> SetItemStream { get; }

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

        InitialState = initialState;
        AddStream = addStream;
        AddRangeStream = addRangeStream;
        RemoveStream = removeStream;
        SetItemStream = setItemStream;
    }

    public Cell<T?> this[int index] => _state.Map(state => state.Count > index ? state[index] : default).Calm();
    public Cell<IEnumerable<T>> Enumerable => _state.Map(state => state.AsEnumerable());
}