using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using Sodium.Frp;
using Sodium.Functional;
using Stream = Sodium.Frp.Stream;

namespace hatcyl.DataStructures.Frp.Tests
{
    [TestClass()]
    public class QueueTests
    {
        [TestMethod()]
        public void QueueTest()
        {
            IImmutableQueue<string> initalState = ImmutableQueue.Create<string>().Enqueue("Initial");
            StreamSink<Unit> clearStream = Stream.CreateSink<Unit>();
            StreamSink<Unit> dequeStream = Stream.CreateSink<Unit>();
            StreamSink<string> enqueueStream = Stream.CreateSink<string>();

            Queue<string> stringQueue = new Queue<string>
            (
                initalState,
                clearStream,
                dequeStream,
                enqueueStream,
                null
            );

            Assert.AreEqual(Maybe.Some("Initial"), stringQueue.Peek.Sample());

            enqueueStream.Send("One");

            Assert.AreEqual(Maybe.Some("Initial"), stringQueue.Peek.Sample());

            dequeStream.Send(Unit.Value);

            Assert.AreEqual(Maybe.Some("One"), stringQueue.Peek.Sample());

            clearStream.Send(Unit.Value);

            Assert.AreEqual(Maybe.None, stringQueue.Peek.Sample());
        }

        [TestMethod()]
        public void QueueBuilderTest()
        {
            IImmutableQueue<string> initalState = ImmutableQueue.Create<string>().Enqueue("Initial");
            StreamSink<Unit> clearStream = Stream.CreateSink<Unit>();
            StreamSink<Unit> dequeStream = Stream.CreateSink<Unit>();
            StreamSink<string> enqueueStream = Stream.CreateSink<string>();

            Queue<string> stringQueue = (new QueueBuilder<string>() with
            {
                InitialState = initalState,
                ClearStream = clearStream,
                DequeStream = dequeStream,
                EnqueueStream = enqueueStream
            }).Build();

            Assert.AreEqual(Maybe.Some("Initial"), stringQueue.Peek.Sample());

            enqueueStream.Send("One");

            Assert.AreEqual(Maybe.Some("Initial"), stringQueue.Peek.Sample());

            dequeStream.Send(Unit.Value);

            Assert.AreEqual(Maybe.Some("One"), stringQueue.Peek.Sample());

            clearStream.Send(Unit.Value);

            Assert.AreEqual(Maybe.None, stringQueue.Peek.Sample());
        }
    }
}