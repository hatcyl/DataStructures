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
using Sodium.Frp.Time;

namespace hatcyl.DataStructures.Frp.Tests
{
    [TestClass()]
    public class JobQueueTests
    {
        [TestMethod()]
        public async Task JobQueueTest()
        {
            IImmutableQueue<string> initalState = ImmutableQueue.Create<string>().Enqueue("Initial");
            StreamSink<Unit> clearStream = Stream.CreateSink<Unit>();
            StreamSink<string> completeStream = Stream.CreateSink<string>();
            StreamSink<Unit> dequeStream = Stream.CreateSink<Unit>();
            StreamSink<string> enqueueStream = Stream.CreateSink<string>();

            JobQueue<string> stringQueue = new JobQueue<string>
            (
                TimeSpan.FromSeconds(5),
                new SystemClockTimerSystem(Console.WriteLine),
                new QueueBuilder<string>(),
                new ListBuilder<string>(),
                clearStream,
                completeStream,
                dequeStream,
                enqueueStream
            );

            stringQueue.List.RemoveRangeStream.Listen(x => { foreach (var y in x) Console.WriteLine($"REMOVED {y}"); });
            stringQueue.List.RemoveStream.Listen(x => Console.WriteLine($"REMOVED {x}"));
            stringQueue.List.Enumerable.Listen(x => { foreach (var value in x) Console.WriteLine(value); });

            enqueueStream.Send("One");
            enqueueStream.Send("Two");
            dequeStream.Send(Unit.Value);

            await Task.Delay(2000);

            dequeStream.Send(Unit.Value);
            //enqueueStream.Send("Three");
            //dequeStream.Send(Unit.Value);

            await Task.Delay(4000);
        }

        [TestMethod()]
        public async Task TimingTest()
        {
            var timerSystem = new SystemClockTimerSystem(Console.WriteLine);
            StreamSink<Unit> stream = Stream.CreateSink<Unit>();

            stream.Listen(x => Console.WriteLine($"Stream {DateTime.Now}"));
            stream.Map(x => timerSystem.At(Cell.Constant(Maybe.Some(DateTime.Now.AddSeconds(2))))).Hold(Stream.Never<DateTime>()).SwitchS()
                .Listen(x => Console.WriteLine($"Timer {x}"));

            stream.Send(Unit.Value);

            await Task.Delay(1000);

            stream.Send(Unit.Value);

            await Task.Delay(1000);

            stream.Send(Unit.Value);

            await Task.Delay(5000);
        }
    }
}