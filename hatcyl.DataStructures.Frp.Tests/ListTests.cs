using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sodium.Frp;
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
            StreamSink<(int index, string value)> setItem = Stream.CreateSink<(int index, string value)>();

            List<string> stringList = new List<string>
            (
                initialState,
                add,
                addRange,
                remove,
                setItem
            );

            Assert.AreEqual("Initial", stringList[0].Sample());

            add.Send("One");

            Assert.AreEqual("Initial", stringList[0].Sample());
            Assert.AreEqual("One", stringList[1].Sample());

            remove.Send("Initial");

            Assert.AreEqual("One", stringList[0].Sample());

            setItem.Send((0, "Two"));

            Assert.AreEqual("Two", stringList[0].Sample());
        }
    }
}