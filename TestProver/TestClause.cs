using System;
using NUnit.Framework;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace TestProver
{
    public class TestClause
    {
        [Test]
        public void TestClauseInit()
        {
            Clause clause1 = ClauseParser.ParseToClause("a&b&c||d"),
                clause2 = ClauseParser.ParseToClause("a=>(b&c||d&e)");

            Assert.AreEqual("((a&b)&c)||d", clause1.ToString());
            Assert.AreEqual(7, clause1.Length);
            Assert.AreEqual("a=>((b&c)||(d&e))", clause2.ToString());
            Assert.AreEqual(9, clause2.Length);
        }

        [Test]
        public void TestCNFClause()
        {
            CNFClause clause1 = new CNFClause(ClauseParser.Parse("a&b&c||d")),
                clause2 = new CNFClause(ClauseParser.Parse("a=>(b&c||d&e)"));
            Console.WriteLine("{0}\n{1}", clause1.ToString(), clause2.ToString());
        }
    }
}
