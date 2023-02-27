using Sodium.Frp;
using Sodium.Functional;
using System.Collections.Immutable;
using Stream = Sodium.Frp.Stream;

namespace hatcyl.DataStructures.Frp;
public record class QueueBuilder<T>
{
    public IImmutableQueue<T> InitialState { get; init; } = ImmutableQueue.Create<T>();
    public Stream<Unit> ClearStream { get; init; } = Stream.Never<Unit>();
    public Stream<Unit> DequeStream { get; init; } = Stream.Never<Unit>();
    public Stream<T> EnqueueStream { get; init; } = Stream.Never<T>();

    public Queue<T> Build() => new
    (
        InitialState,
        ClearStream,
        DequeStream,
        EnqueueStream
    );
}