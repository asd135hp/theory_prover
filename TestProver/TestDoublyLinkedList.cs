using NUnit.Framework;
using Prover.Representation.Parser;

namespace TestAssignment2
{
    class TestDoublyLinkedList
    {
        private Block Root, OtherRoot;

        [SetUp]
        public void Setup()
        {
            Root = ClauseParser.Parse("a&b");
            OtherRoot = ClauseParser.Parse("c||d");
            Assert.AreEqual("a&b", Root.ToString());
            Assert.AreEqual("c||d", OtherRoot.ToString());
        }

        [Test]
        public void TestForwardChaining()
        {
            Assert.AreNotEqual(Root.GetHashCode(), Root.NextBlock.GetHashCode());
            Assert.AreEqual(Root.NextBlock.NextBlock.NextBlock.GetHashCode(), Root.GetHashCode());
        }

        [Test]
        public void TestBackwardChaining()
        {
            Assert.AreNotEqual(Root.NextBlock.GetHashCode(), Root.GetHashCode());
            Assert.AreEqual(Root.NextBlock.NextBlock.GetHashCode(), Root.PreviousBlock.GetHashCode());
        }

        [Test]
        public void TestRemoveFront()
        {
            Root.NextBlock.NextBlock.RemoveFront();
            Assert.AreEqual("ab", Root.ToString());
            Assert.AreEqual(Root.NextBlock.GetHashCode(), Root.PreviousBlock.GetHashCode());
        }

        [Test]
        public void TestRemoveBack()
        {
            Root.NextBlock.RemoveBack();
            Assert.AreEqual("a&", Root.ToString());
            Assert.AreEqual(Root.NextBlock.GetHashCode(), Root.PreviousBlock.GetHashCode());
        }

        [Test]
        public void TestExtendFront()
        {
            Root.ExtendBack(OtherRoot);
            Assert.AreEqual("ac||d&b", Root.ToString());
        }

        [Test]
        public void TestExtendBack()
        {
            Root.ExtendFront(OtherRoot);
            Assert.AreEqual("a&bc||d", Root.ToString());
        }
    }
}
