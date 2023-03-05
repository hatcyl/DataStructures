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
    public record DequeuedJob (T Value);

    public Dictionary<DequeuedJob, Stream<DequeuedJob>> ExpiredDequesdJobsFrp { get; }
    public Cell<IImmutableDictionary<DequeuedJob, Stream<DequeuedJob>>> ExpiredDequeuedJobs { get; }
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

        ExpiredDequeuedJobs = new StateCellBuilder<IImmutableDictionary<DequeuedJob, Stream<DequeuedJob>>>(ImmutableDictionary.Create<DequeuedJob, Stream<DequeuedJob>>())
            .WithMethod
            (
                dequeStream.Snapshot(Queue.Peek).FilterMaybe().Map(x => new DequeuedJob(x)),
                value => state => state.Add(value, TimerSystem.At(Cell.Constant(Maybe.Some(DateTime.Now.Add(dequeuedJobExpiration)))).Once().MapTo(value))
            )
            .Build();

        List = (listBuilder with
        {
            AddStream = dequeStream.Snapshot(Queue.Peek).FilterMaybe(),
            RemoveRangeStream = ExpiredDequeuedJobs.Values().Map(x => x.Values).Map(x => x.MergeToEnumerable()).Hold(Stream.Never<IEnumerable<DequeuedJob>>()).SwitchS().Map(x => x.Select(y => y.Value))
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

