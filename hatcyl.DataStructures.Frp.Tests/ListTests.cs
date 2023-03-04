using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodium.Frp;
using Sodium.Functional;
using System.Collections.Immutable;
using Stream = Sodium.Frp.Stream;

namespace hatcyl.DataStructures.Frp.Tests
{
    [TestClass()]
    public class ListTests
    {
        [TestMethod()]
        public void ListTest()
        {
            IImmutableList<string> initialState = ImmutableList.Create<string>().Add("Initial");
            StreamSink<string> add = Stream.CreateSink<string>();
            StreamSink<IEnumerable<string>> addRange = Stream.CreateSink<IEnumerable<string>>();
            StreamSink<string> remove = Stream.CreateSink<string>();
            StreamSink<IEnumerable<string>> removeRange = Stream.CreateSink<IEnumerable<string>>();
            StreamSink<(int index, string value)> setItem = Stream.CreateSink<(int index, string value)>();

            List<string> stringList = new List<string>
            (
                initialState,
                add,
                addRange,
                remove,
                removeRange,
                setItem
            );

            Assert.AreEqual(Maybe.Some("Initial"), stringList[0].Sample());

            add.Send("One");

            Assert.AreEqual(Maybe.Some("Initial"), stringList[0].Sample());
            Assert.AreEqual(Maybe.Some("One"), stringList[1].Sample());

            remove.Send("Initial");

            Assert.AreEqual(Maybe.Some("One"), stringList[0].Sample());

            setItem.Send((0, "Two"));

            Assert.AreEqual(Maybe.Some("Two"), stringList[0].Sample());
        }

        [TestMethod()]
        public void ListBuilderTest()
        {
            IImmutableList<string> initialState = ImmutableList.Create<string>().Add("Initial");
            StreamSink<string> add = Stream.CreateSink<string>();
            StreamSink<string> remove = Stream.CreateSink<string>();
            StreamSink<(int index, string value)> setItem = Stream.CreateSink<(int index, string value)>();

            List<string> stringList = (new ListBuilder<string>() with
            {
                InitialState = initialState,
                AddStream = add,
                RemoveStream = remove,
                SetItemStream = setItem
            }).Build();

            Assert.AreEqual(Maybe.Some("Initial"), stringList[0].Sample());

            add.Send("One");

            Assert.AreEqual(Maybe.Some("Initial"), stringList[0].Sample());
            Assert.AreEqual(Maybe.Some("One"), stringList[1].Sample());

            remove.Send("Initial");

            Assert.AreEqual(Maybe.Some("One"), stringList[0].Sample());

            setItem.Send((0, "Two"));

            Assert.AreEqual(Maybe.Some("Two"), stringList[0].Sample());
        }
    }
}