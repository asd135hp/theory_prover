using NUnit.Framework;
using System.Collections.Generic;
using Prover.Representation.Rephraser;
using Prover.Representation.Rephraser.Type;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace TestProver
{
    public class TestClauseRephrase
    {
        private readonly string[] StartingClauses = new string[]
        {
            "a",
            "a=>b",
            "a&(b||c)",
            "(b<=>u)&(a||c)",
            "(d=>g&c)||(d&a)"
        }, ExpectedClauses = new string[]
        {
            "a",
            "(~a||b)",
            "a&(b||c)",
            "((~b||u)&(~u||b))&(a||c)",
            "()"
        };

        [Test]
        public void TestBiconditionalElimination()
        {
            RephraserType r1 = new BiconditionalElimination(),
                r2 = new BiconditionalElimination(),
                r3 = new BiconditionalElimination();

            r1.AddLeftBlock(new Block("a")).AddRightBlock(new Block("b"));
            r2.AddLeftBlock(ClauseParser.Parse("(a&b)")).AddRightBlock(new Block("c"));
            r3.AddLeftBlock(ClauseParser.Parse("(a&b)")).AddRightBlock(ClauseParser.Parse("(c||d)"));

            Assert.AreEqual("(~a||b)&(a||~b)", r1.Translate().ToString());
            Assert.AreEqual("(~(a&b)||c)&((a&b)||~c)", r2.Translate().ToString());
            Assert.AreEqual("(~(a&b)||(c||d))&((a&b)||~(c||d))", r3.Translate().ToString());
        }

        [Test]
        public void TestModusPonens()
        {
            RephraserType r1 = new ModusPonens(),
                r2 = new ModusPonens(),
                r3 = new ModusPonens();

            r1.AddLeftBlock(new Block("a")).AddRightBlock(new Block("b"));
            r2.AddLeftBlock(ClauseParser.Parse("(a&b)")).AddRightBlock(new Block("c"));
            r3.AddLeftBlock(ClauseParser.Parse("(a&b)")).AddRightBlock(ClauseParser.Parse("(c||d)"));

            Assert.AreEqual("~a||b", r1.Translate().ToString());
            Assert.AreEqual("~(a&b)||c", r2.Translate().ToString());
            Assert.AreEqual("~(a&b)||(c||d)", r3.Translate().ToString());
        }

        [Test]
        public void TestTruthfulTheorems()
        {
            RephraserType r1 = new TruthfulTheorems(),
                r2 = new TruthfulTheorems(),
                r3 = new TruthfulTheorems();

            r1.AddLeftBlock(new Block("a"))
                .AddLogic(PropositionalLogic.Disjunction)
                .AddRightBlock(new Block("c"));

            r2.AddLeftBlock(new Block("a"))
                .AddLogic(PropositionalLogic.Conjunction)
                .AddRightBlock(new Block("false"));

            r3.AddLeftBlock(ClauseParser.Parse("a"))
                .AddLogic(PropositionalLogic.Disjunction)
                .AddRightBlock(ClauseParser.Parse("true"));

            Assert.AreEqual("a||c", r1.Translate().ToString());
            Assert.AreEqual("false", r2.Translate().ToString());
            Assert.AreEqual("true", r3.Translate().ToString());
        }

        [Test]
        public void TestDeMorgan()
        {
            RephraserType r1 = new DeMorgan(),
                r2 = new DeMorgan(),
                r3 = new DeMorgan();

            r1.AddLeftBlock(ClauseParser.Parse("~(a&b)"));
            r2.AddLeftBlock(ClauseParser.Parse("(~a||~b)"));
            r3.AddRightBlock(ClauseParser.Parse("~(~(a&c)&~b)"));

            Assert.AreEqual("(~a||~b)", r1.Translate().ToString());
            Assert.AreEqual("~(a&b)", r2.Translate().ToString());
            Assert.AreEqual("((a&c)||b)", r3.Translate().ToString());
        }

        [Test]
        public void TestDistribution()
        {
            RephraserType r1 = new Distribution(),
                r2 = new Distribution(),
                r3 = new Distribution();

            r1.AddLeftBlock(new Block("a"))
                .AddLogic(PropositionalLogic.Conjunction)
                .AddRightBlock(new Block("b"));

            r2.AddLeftBlock(new Block("a"))
                .AddLogic(PropositionalLogic.Disjunction)
                .AddRightBlock(ClauseParser.Parse("(b&(c||d))"));

            r3.AddLeftBlock(ClauseParser.Parse("(b&d)"))
                .AddLogic(PropositionalLogic.Disjunction)
                .AddRightBlock(new Block("c"));

            Assert.AreEqual("a&b", r1.Translate().ToString());
            Assert.AreEqual("(a||b)&(a||(c||d))", r2.Translate().ToString());
            Assert.AreEqual("(c||b)&(c||d)", r3.Translate().ToString());
        }

        /// <summary>
        /// Test #1: Infinite loop - speculation: at least 1 group rephrasing leads to infinite loop!
        /// Test #2: 
        /// </summary>
        [Test]
        [Ignore("")]
        public void TestRephrasingNormalClauses()
        {
            int count = 0;
            Assert.DoesNotThrow(() =>
            {
                foreach (var clause in new ClauseRephraser().Rephrase(new List<string>(StartingClauses)))
                    Assert.AreEqual(ExpectedClauses[count++], clause);
            });
        }

        [Test]
        [Ignore("")]
        public void TestGuaranteeCNF()
        {
            Assert.DoesNotThrow(() =>
            {
                foreach (var clause in StartingClauses)
                {
                    var block = new ClauseRephraser().RephraseIntoBlock(clause);
                    BlockIterator.ForEach(block, (currentBlock) =>
                    {
                        // all normal clauses contain only disjunction
                        // conjunction only appears when concatenating two or more clauses
                        if (currentBlock.ContentType == ContentType.Logic)
                            Assert.AreEqual(
                                (currentBlock.GetContent() as PropositionalLogic).IsDisjunction,
                                true
                            );
                    });
                }
            });

        }
    }
}
