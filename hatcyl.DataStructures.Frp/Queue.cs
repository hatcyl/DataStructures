using hatcyl.DataStructures.Frp.Utilities;
using Sodium.Frp;
using Sodium.Functional;
using System.Collections.Immutable;

namespace hatcyl.DataStructures.Frp;
public class Queue<T>
{
    public readonly Cell<IImmutableQueue<T>> _state;

    public IImmutableQueue<T> InitialState { get; }
    public Stream<Unit> ClearStream { get; }
    public Stream<Unit> DequeStream { get; }
    public Stream<T> EnqueueStream { get; }

    public Queue
    (
        IImmutableQueue<T> initialState,
        Stream<Unit> clearStream,
        Stream<Unit> dequeStream,
        Stream<T> enqueueStream
    )
    {
        _state = new StateCellBuilder<IImmutableQueue<T>>(initialState)
        .WithMethod(clearStream, value => state => state.Clear())
        .WithMethod(dequeStream, value => state => state.Dequeue())
        .WithMethod(enqueueStream, value => state => state.Enqueue(value))
        .Build();

        InitialState = initialState;
        ClearStream = clearStream;
        DequeStream = dequeStream;
        EnqueueStream = enqueueStream;
    }

    public Cell<Maybe<T>> Peek => _state.Map(state => !state.IsEmpty ? Maybe<T>.Some(state.Peek()) : Maybe<T>.None);
    public Cell<IEnumerable<T>> Enumerable => _state.Map(state => state.AsEnumerable());
}