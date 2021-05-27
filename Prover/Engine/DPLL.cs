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
        public virtual string GetResult(KnowledgeBase kb, string ask)
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
                    || (ask == "unsatisfiable" && !result) ? $"YES: {Path}" : "NO";
            }
            else if (ask == "rawoutput") return Main(clauses) ? $"SATISFIABLE: {Path}" : "UNSATISFIABLE";

            throw new System.NotSupportedException(
                "Wrong type of ask argument!\n" +
                "Available ask argument: satisfiable, unsatisfiable or rawOutput");
        }

        private string Path;

        /// <summary>
        /// Core functionality of DPLL
        /// </summary>
        /// <see cref="https://www.cs.cmu.edu/~15414/f17/lectures/10-dpll.pdf"/>
        /// <see cref="https://www.cs.miami.edu/home/geoff/Courses/CSC648-12S/Content/DPLL.shtml"/>
        /// <see cref="https://www.inf.ufpr.br/dpasqualin/d3-dpll/"/>
        /// <param name="clauses"></param>
        /// <returns></returns>
        private bool Main(List<Block> clauses, string path = "")
        {
            List<Block> unitClauses;
            do
            {
                // is there an empty clause or not
                if (clauses.Contains(null)) return false;
                
                // if clauses list is empty, this loop will do nothing and automatically break
                unitClauses = new List<Block>(clauses.Where((clause) =>
                {
                    return clause.NextBlock == clause && clause.PreviousBlock == clause;
                }));

                foreach (var unitClause in unitClauses)
                {
                    path = (path.Length == 0 ? "" : $"{path}, ") + unitClause;
                    RemoveSymbol(clauses, unitClause, false);
                }

            } while (unitClauses.Count != 0);

            // store path when it is true
            if (clauses.Count == 0)
            {
                Path = path;
                return true;
            }
            
            // is there an empty clause or not
            if (clauses.Contains(null)) return false;

            Block selected = clauses[0],
                symbol = new Block(selected.GetContent(true).ToString(), selected.IsNegated);
            path = (path.Length == 0 ? "" : $"{path}, ") + symbol;

            return Main(RemoveSymbol(clauses, symbol), path)
                || Main(RemoveSymbol(clauses, symbol.SetNegation(!symbol.IsNegated)), path);
        }

        /// <summary>
        /// Remove any symbols inside clause, guarantee to also remove its trailing connective.
        /// <para>Eg: a||b||c into b||c if removing a or a||c if removing b</para>
        /// </summary>
        /// <param name="clauses"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private List<Block> RemoveSymbol(List<Block> clauses, Block symbol, bool newList = true)
        {
            // must specify a fresh copy on demand here because blocks are also objects
            // that have memory addresses
            var result = !newList ? clauses :
                                    clauses.Select((b) => ClauseParser.Parse(b.ToString())).ToList();

            string rawSymbol = symbol.GetContent(true).ToString();
            for (int i = 0; i < result.Count; ++i)
            {
                var root = result[i];
                if (root == null) continue;

                // remove either a symbol or the whole rootBlock
                BlockIterator.TerminableForEach(root, (currentBlock) =>
                {
                    if (currentBlock.GetContent(true).ToString() == rawSymbol)
                    {
                        // remove the whole clause if it contains the exact content of the symbol
                        // passed through in this method
                        // (true in a disjunctive clause means true as a whole)
                        if (currentBlock.IsNegated == symbol.IsNegated)
                        {
                            result.RemoveAt(i--);
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
                            result[i] = null;
                            return false;
                        }

                        if (isEOL)
                        {
                            // a||b into b if remove a
                            unaffectedBlock.RemoveBack(true);
                            unaffectedBlock.RemoveBack(true);
                        }
                        else
                        {
                            // a||b||c into a||c if remove b
                            unaffectedBlock.RemoveFront(true);
                            unaffectedBlock.RemoveFront(true);
                        }

                        if (root == currentBlock)
                        {
                            result[i] = unaffectedBlock;
                            return false;
                        }
                    }
                    return true;
                });
            }

            return result;
        }
    }
}
