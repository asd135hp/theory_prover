using System.Linq;
using System.Collections.Generic;
using Prover.Representation;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace Prover.Engine
{
    public class DPLL
    {
        public DPLL() { }

        /// <summary>
        /// The main output of this engine
        /// </summary>
        /// <param name="kb"></param>
        /// <param name="ask"></param>
        /// <returns></returns>
        public string GetResult(KnowledgeBase kb, string ask)
        {
            // could be extended to handle really big dataset with millions of symbols
            // example: List<List<Block>> contains 2 billion ^ 2 slots
            // here, it can only handle at most log2(2 billion)
            var clauses = new List<Block>();
            foreach(var normalClause in kb.Knowledges)
                clauses.AddRange(new CNFClause(normalClause).Conjunctions);

            ask = ask.ToLower();
            if (ask == "satisfiable" || ask == "unsatisfiable")
            {
                bool result = Main(clauses);
                return (ask == "satisfiable" && result)
                    || (ask == "unsatisfiable" && !result) ? "YES" : "NO";
            }
            else if (ask == "rawOutput") return Main(clauses) ? "satisfiable" : "unsatisfiable";

            throw new System.NotSupportedException(
                "Wrong type of ask argument!\n" +
                "Available ask argument: satisfiable, unsatisfiable or rawOutput");
        }

        /// <summary>
        /// Core functionality of DPLL
        /// </summary>
        /// <param name="clauses"></param>
        /// <returns></returns>
        private bool Main(List<Block> clauses)
        {
            if (clauses.Count == 0) return true;

            var unitClauses = new List<Block>(clauses.Where((clause) =>
            {
                return clause.NextBlock == clause && clause.PreviousBlock == clause;
            }));

            if (clauses.Count % 2 == 0 && unitClauses.Count == clauses.Count)
            {
                string checkingContent = "";
                int countPositive = 0, countNegative = 0;
                foreach (var clause in unitClauses)
                {
                    var content = clause.GetContent(true).ToString();
                    if (checkingContent.Length == 0) checkingContent = content;
                    else if (content != checkingContent) break;

                    if (clause.IsNegated) ++countNegative;
                    else ++countPositive;
                }
                if (countPositive != 0 && countNegative == countPositive) return false;
            }

            Block selected = unitClauses.Count == 0 ? clauses[0].PreviousBlock : unitClauses[0],
                symbol = new Block(selected.GetContent(true).ToString(), selected.IsNegated);

            return Main(RemoveSymbol(clauses, symbol))
                || Main(RemoveSymbol(clauses, symbol.SetNegation(!symbol.IsNegated)));
        }

        /// <summary>
        /// Remove any symbols inside clause, guarantee to also remove its trailing connective.
        /// <para>Eg: a||b||c into b||c if removing a or a||c if removing b</para>
        /// </summary>
        /// <param name="clauses"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private List<Block> RemoveSymbol(List<Block> clauses, Block symbol)
        {
            string rawSymbol = symbol.GetContent(true).ToString();
            for (int i = 0; i < clauses.Count; ++i)
            {
                var root = clauses[i];
                BlockIterator.TerminableForEach(root, (currentBlock) =>
                {
                    if (currentBlock.GetContent(true).ToString() == rawSymbol)
                    {
                        // remove the whole clause if it contains the exact content of the symbol
                        // passed through in this method
                        // (true in a disjunctive clause means true as a whole)
                        if (currentBlock.IsNegated == symbol.IsNegated)
                        {
                            clauses.RemoveAt(i--);
                            return false;
                        }

                        // remove only this symbol if it does not have the same negation
                        // because when this happens, the symbol within stored list turns out to be false
                        // which is removable in a sea of disjunctions
                        bool isEOL = currentBlock.NextBlock == root;
                        var unaffectedBlock = isEOL ?
                            currentBlock.PreviousBlock.PreviousBlock :
                            currentBlock.NextBlock.NextBlock;

                        // block list contains only 1 block
                        if (unaffectedBlock == currentBlock)
                        {
                            currentBlock.Isolate(true);
                            clauses.RemoveAt(i--);
                            return false;
                        }

                        // a||b into b if remove a
                        if (isEOL)
                        {
                            unaffectedBlock.RemoveBack(true);
                            unaffectedBlock.RemoveBack(true);
                        }
                        else
                        {
                            unaffectedBlock.RemoveFront(true);
                            unaffectedBlock.RemoveFront(true);
                        }

                        if (root == currentBlock)
                        {
                            clauses[i] = unaffectedBlock;
                            return false;
                        }
                    }
                    return true;
                });
            }
            return clauses;
        }
    }
}
