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
public class JobQueue<T>
{
    public Queue<T> Queue { get; }
    public List<T> List { get; }
    public Stream<Unit> ClearStream { get; }
    public Stream<T> CompleteStream { get; }
    public Stream<Unit> DequeStream { get; }
    public Stream<T> EnqueueStream { get; }

    public JobQueue
    (
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
            AddStream = dequeStream.Snapshot(Queue.Peek).FilterMaybe()
        }).Build();
        
        ClearStream = clearStream;
        CompleteStream = completeStream;
        DequeStream = dequeStream;
        EnqueueStream = enqueueStream;
    }
}

