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
    public record DequeuedJob(T Value);

    public TimeSpan DequeuedJobExpiration { get; }
    public ITimerSystem<DateTime> TimerSystem { get; }
    public Dictionary<DequeuedJob, Stream<DequeuedJob>> Dictionary { get; }
    public Queue<T> Queue { get; }
    public List<T> List { get; }
    public Stream<Unit> ClearStream { get; }
    public Stream<T> CompleteStream { get; }
    public Stream<Unit> DequeStream { get; }
    public Stream<T> EnqueueStream { get; }
    public Stream<IEnumerable<T>> EnqueueManyStream { get; }

    public JobQueue
    (
        TimeSpan dequeuedJobExpiration,
        ITimerSystem<DateTime> timerSystem,
        DictionaryBuilder<DequeuedJob, Stream<DequeuedJob>> dictionaryBuilder,
        QueueBuilder<T> queueBuilder,
        ListBuilder<T> listBuilder,
        Stream<Unit> clearStream,
        Stream<T> completeStream,
        Stream<Unit> dequeStream,
        Stream<T> enqueueStream,
        Stream<IEnumerable<T>> enqueueManyStream
    )
    {
        (Dictionary, Queue, List) = Transaction.Run<(Dictionary<DequeuedJob, Stream<DequeuedJob>> Dictionary, Queue<T> Queue, List<T> List)>(() =>
        {


            var theStream = Stream.CreateLoop<IEnumerable<T>>();

            var q = (queueBuilder with
            {
                EnqueueStream = enqueueStream,
                EnqueueManyStream = theStream,
                DequeStream = dequeStream,
                ClearStream = clearStream,
            }).Build();

            var d = (dictionaryBuilder with
            {
                AddStream = dequeStream.Snapshot(q.Peek).FilterMaybe().Map(job => new DequeuedJob(job)).Map(dequeuedJob => (dequeuedJob, TimerSystem.At(Cell.Constant(Maybe.Some(DateTime.Now.Add(dequeuedJobExpiration)))).Once().MapTo(dequeuedJob)))
            }).Build();

            var l = (listBuilder with
            {
                AddStream = dequeStream.Snapshot(q.Peek).FilterMaybe(),
                RemoveStream = completeStream,
                RemoveRangeStream = d.Values.Values().Map(x => x.MergeToEnumerable()).Hold(Stream.Never<IEnumerable<DequeuedJob>>()).SwitchS().Map(x => x.Select(y => y.Value))
            }).Build();

            theStream.Loop(enqueueManyStream.Merge(d.Values.Values().Map(x => x.MergeToEnumerable()).Hold(Stream.Never<IEnumerable<DequeuedJob>>()).SwitchS().Snapshot(l.Enumerable, (jobs, list) => jobs.Select(y => y.Value).Where(x => list.Contains(x))), (x, y) => x.Concat(y)));


            return (d, q, l);
        });

        DequeuedJobExpiration = dequeuedJobExpiration;
        TimerSystem = timerSystem;
        ClearStream = clearStream;
        CompleteStream = completeStream;
        DequeStream = dequeStream;
        EnqueueStream = enqueueStream;
        EnqueueManyStream = enqueueManyStream;
    }

    public Cell<Maybe<T>> Peek => Queue.Peek;
}

