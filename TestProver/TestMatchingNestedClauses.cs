using NUnit.Framework;
using Prover.Representation;

namespace TestProver
{
    class TestMatchingNestedClauses
    {
        [Test]
        public void Test_1_0_0_Clause()
        {
            string clause = "f&(q||k)&~a";
            Assert.AreEqual("(f&(q||k))&~a", new KnowledgeBase(clause).ToString());
        }

        [Test]
        public void Test_3_0_0_Clause()
        {
            string clause = "(y||f)&(q||k)&(~a&b)";
            Assert.AreEqual("((y||f)&(q||k))&(~a&b)", new KnowledgeBase(clause).ToString());
        }

        [Test]
        public void Test_1_2_0_Clause()
        {
            string clause = "(y||f&(q||k)&(~a&b))";
            Assert.AreEqual("(y||((f&(q||k))&(~a&b)))", new KnowledgeBase(clause).ToString());
        }
        [Test]
        public void Test_1_2_1_Clause()
        {
            string clause = "(y||f&(q||(~k&q))&(~(a||l)&b))";
            Assert.AreEqual("(y||((f&(q||(~k&q)))&(~(a||l)&b)))", new KnowledgeBase(clause).ToString());
        }
        [Test]
        public void Test_2_1_2_Clause()
        {
            string clause = "k1&m1||(y||f&(q||(k||m)&~(o&u)||~a1)&~a)&(m2||(k3&(b4||d)&f||~(k&m))&p)";
            Assert.AreEqual(
                "(k1&m1)||((y||((f&((q||((k||m)&~(o&u)))||~a1))&~a))&(m2||((((k3&(b4||d))&f)||~(k&m))&p)))",
                new KnowledgeBase(clause).ToString());
        }
        [Test]
        public void Test_3_1_1_Clause()
        {
            string clause = "(y||(f&~k))&(q||~(o&i||n)&k)&(~a||(j&~s)&b)";
            Assert.AreEqual(
                "((y||(f&~k))&(q||(~((o&i)||n)&k)))&(~a||((j&~s)&b))",
                new KnowledgeBase(clause).ToString());
        }
    }
}
