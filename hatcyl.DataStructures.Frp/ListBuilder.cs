using Sodium.Frp;
using System.Collections.Immutable;
using Stream = Sodium.Frp.Stream;

namespace hatcyl.DataStructures.Frp;
public record class ListBuilder<T>
{
    public IImmutableList<T> InitialState { get; init; } = ImmutableList.Create<T>();
    public Stream<T> AddStream { get; init; } = Stream.Never<T>();
    public Stream<IEnumerable<T>> AddRangeStream { get; init; } = Stream.Never<IEnumerable<T>>();
    public Stream<T> RemoveStream { get; init; } = Stream.Never<T>();
    public Stream<(int index, T value)> SetItemStream { get; init; } = Stream.Never<(int index, T value)>();

    public List<T> Build() => new
    (
        InitialState,
        AddStream,
        AddRangeStream,
        RemoveStream,
        SetItemStream
    );
}