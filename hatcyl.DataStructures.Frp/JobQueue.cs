using hatcyl.DataStructures.Frp.Utilities;
using Sodium.Frp;
using Sodium.Frp.Time;
using Sodium.Functional;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stream = Sodium.Frp.Stream;

namespace hatcyl.DataStructures.Frp;
public class JobQueue<T>
{
    public TimeSpan DequeuedJobExpiration { get; }
    public ITimerSystem<DateTime> TimerSystem { get; }
    public Queue<T> Queue { get; }
    public List<T> List { get; }
    public Stream<Unit> ClearStream { get; }
    public Stream<T> CompleteStream { get; }
    public Stream<Unit> DequeStream { get; }
    public Stream<T> EnqueueStream { get; }

    public JobQueue
    (
        TimeSpan dequeuedJobExpiration,
        ITimerSystem<DateTime> timerSystem,
        QueueBuilder<T> queueBuilder,
        ListBuilder<T> listBuilder,
        Stream<Unit> clearStream,
        Stream<T> completeStream,
        Stream<Unit> dequeStream,
        Stream<T> enqueueStream
    )
    {
        Queue = (queueBuilder with
        {
            EnqueueStream = enqueueStream,
            DequeStream = dequeStream,
            ClearStream = clearStream,
        }).Build();

        List = (listBuilder with
        {
            AddStream = dequeStream.Snapshot(Queue.Peek).FilterMaybe(),
            RemoveRangeStream = new StateCellBuilder<Stream<IEnumerable<T>>>(Stream.Never<IEnumerable<T>>())
            .WithMethod
            (
                dequeStream.Snapshot(Queue.Peek).FilterMaybe(),
                value => state => state.Merge
                (
                    TimerSystem
                        .At(Cell.Constant(Maybe.Some(DateTime.Now.Add(dequeuedJobExpiration)))).Once()
                        .MapTo(Enumerable.Empty<T>().Append(value)),
                    (mergedStream, newStream) => mergedStream.Concat(newStream)
                )
            )
            .Build().SwitchS()
        }).Build();

        DequeuedJobExpiration = dequeuedJobExpiration;
        TimerSystem = timerSystem;
        ClearStream = clearStream;
        CompleteStream = completeStream;
        DequeStream = dequeStream;
        EnqueueStream = enqueueStream;
    }

    public Cell<Maybe<T>> Peek => Queue.Peek;
}

