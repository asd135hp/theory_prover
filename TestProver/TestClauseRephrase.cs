using NUnit.Framework;
using System.Collections.Generic;
using Prover.Engine.Rephraser;
using Prover.Representation.Parser;

namespace TestAssignment2
{
    public class TestClauseRephrase
    {
        private readonly string[] StartingClauses = new string[]
        {
            "a",
            "a=>b",
            "a&(b||c)",
            "(d=>g&c)||(d&a)",
            "(b<=>u)&(a||c)"
        }, ExpectedClauses = new string[]
        {
            "a",
            "~a||b",
            "((a&b)||(a&c))",
            "~d||(~d&c)||(g&~d)||(g&c)||(d&a)",
            "(~b||u)&(~u||b)&(a||c)"
        }, StartingHornClauses = new string[]
        {
            "a",
            "a=>b",
            "a||b=>c",
            "~a||b||~c",
            "~a||~b||~c"
        }, ExpectedHornClauses = new string[]
        {
            "a",
            "a=>b",
            "(a||b)=>c",
            "(a&c)=>b",
            "(a&b&c)=>false"
        };

        [Test]
        public void TestBiconditionalElimination()
        {
            Block logic = ClauseParser.Parse("a<=>b").NextBlock,
                logic1 = ClauseParser.Parse("(a&b)<=>c").NextBlock,
                logic2 = ClauseParser.Parse("(a&b)<=>(c||d)").NextBlock;

            Assert.AreEqual(
                "((~a&~b)||(a&b))",
                RephrasingLaw.BiconditionalElimination(logic).ToString());

            Assert.AreEqual(
                "((~(a&b)&~c)||((a&b)&c))",
                RephrasingLaw.BiconditionalElimination(logic1).ToString());

            Assert.AreEqual(
                "((~(a&b)&~(c||d))||((a&b)&(c||d)))",
                RephrasingLaw.BiconditionalElimination(logic2).ToString());
        }

        [Test]
        public void TestModusPonens()
        {
            Block logic = ClauseParser.Parse("a=>b").NextBlock,
                logic1 = ClauseParser.Parse("(a&b)=>c").NextBlock,
                logic2 = ClauseParser.Parse("(a&b)=>(c||d)").NextBlock;

            Assert.AreEqual("(~a||b)", RephrasingLaw.ModusPonens(logic).ToString());
            Assert.AreEqual("(~(a&b)||c)", RephrasingLaw.ModusPonens(logic1).ToString());
            Assert.AreEqual("(~(a&b)||(c||d))", RephrasingLaw.ModusPonens(logic2).ToString());
        }

        [Test]
        public void TestTruthfulTheorems()
        {
            Block logic = ClauseParser.Parse("a||c").NextBlock,
                logic1 = ClauseParser.Parse("a&false").NextBlock,
                logic2 = ClauseParser.Parse("a||true").NextBlock;

            Assert.AreEqual("a||c", RephrasingLaw.TruthfulTheorems(logic).PreviousBlock.ToString());
            Assert.AreEqual("false", RephrasingLaw.TruthfulTheorems(logic1).PreviousBlock.ToString());
            Assert.AreEqual("true", RephrasingLaw.TruthfulTheorems(logic2).PreviousBlock.ToString());
        }

        [Test]
        public void TestDeMorgan()
        {
            Block block = ClauseParser.Parse("~(a&b)"),
                block1 = ClauseParser.Parse("(~a||~b)"),
                block2 = ClauseParser.Parse("~(~(a&c)&~b)");

            Assert.AreEqual("(~a||~b)", RephrasingLaw.DeMorgan(block).ToString());
            Assert.AreEqual("~(a&b)", RephrasingLaw.DeMorgan(block1).ToString());
            Assert.AreEqual("((a&c)||b)", RephrasingLaw.DeMorgan(block2).ToString());
        }

        [Test]
        public void TestDistribution()
        {
            Block logic = ClauseParser.Parse("a&b").NextBlock,
                logic1 = ClauseParser.Parse("a||(b&(c||d))").NextBlock,
                logic2 = ClauseParser.Parse("(a||c)&((b&d)||c)").NextBlock;

            Assert.AreEqual("a&b", RephrasingLaw.Distribution(logic).PreviousBlock.ToString());
            Assert.AreEqual(
                "((a||b)&(a||(c||d)))",
                RephrasingLaw.Distribution(logic1).PreviousBlock.ToString());

            Assert.AreEqual(
                "(((a||c)&(b&d))||((a||c)&c))",
                RephrasingLaw.Distribution(logic2).PreviousBlock.ToString());
        }


        [Test]
        public void TestAssociative()
        {
            Block clause = ClauseParser.Parse("((a&b)&c)||a"),
                clause1 = ClauseParser.Parse("(((a&b)&(a&c))&(b&d))||((m&k)&(m&g))"),
                clause2 = ClauseParser.Parse("(~(~a||b)&a)||((k&~((m&g))||~k)");

            Assert.AreEqual("((a&b)&c)||a", RephrasingLaw.Associative(clause).ToString());
            Assert.AreEqual(
                "(((a&b)&c)&d)||((m&k)&g)",
                RephrasingLaw.Associative(clause1).ToString());

            Assert.AreEqual(
                "(a&~b)||(k&~(m&g))",
                RephrasingLaw.Associative(clause2).ToString());
        }

        /// <summary>
        /// Test #1: Infinite loop - speculation: at least 1 group rephrasing leads to infinite loop!
        /// Test #2: 
        /// </summary>
        [Test]
        public void TestRephrasingNormalClauses()
        {
            int count = 0;
            Assert.DoesNotThrow(() =>
            {
                foreach (var clause in ClauseRephraser.Rephrase(new List<string>(StartingClauses)))
                    Assert.AreEqual(ExpectedClauses[count++], clause);
            });
        }

        [Test]
        public void TestRephrasingHornClauses()
        {
            int count = 0;

            Assert.DoesNotThrow(() =>
            {
                var list = ClauseRephraser.Rephrase(
                    new List<string>(StartingHornClauses),
                    RephraserType.HornClause
                );

                foreach (var clause in list)
                    Assert.AreEqual(ExpectedHornClauses[count++], clause);
            });
        }
    }
}
