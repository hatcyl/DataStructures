using Microsoft.VisualStudio.TestTools.UnitTesting;
using hatcyl.DataStructures.Frp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodium.Frp;
using Sodium.Functional;
using System.Collections.Immutable;
using Stream = Sodium.Frp.Stream;

namespace hatcyl.DataStructures.Frp.Tests
{
    [TestClass()]
    public class JobQueueTests
    {
        [TestMethod()]
        public void JobQueueTest()
        {
            IImmutableQueue<string> initalState = ImmutableQueue.Create<string>().Enqueue("Initial");
            StreamSink<Unit> clearStream = Stream.CreateSink<Unit>();
            StreamSink<string> completeStream = Stream.CreateSink<string>();
            StreamSink<Unit> dequeStream = Stream.CreateSink<Unit>();
            StreamSink<string> enqueueStream = Stream.CreateSink<string>();

            JobQueue<string> stringQueue = new JobQueue<string>
            (
                new QueueBuilder<string>(),
                new ListBuilder<string>(),
                clearStream,
                completeStream,
                dequeStream,
                enqueueStream
            );

            stringQueue.List.Enumerable.Listen(x => { foreach (var value in x) Console.WriteLine(value); });

            enqueueStream.Send("One");
            enqueueStream.Send("Two");
            dequeStream.Send(Unit.Value);
            dequeStream.Send(Unit.Value);
            enqueueStream.Send("Three");
            dequeStream.Send(Unit.Value);
        }
    }
}