using System.Collections.Generic;
using Prover.Representation;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace Prover.Engine
{
    public class DPLL
    {
        public DPLL(UniqueSymbols symbols) { Symbols = symbols.UniqueValues; }

        private List<string> Symbols;

        public string GetResult(KnowledgeBase kb, string ask)
        {
            var clauses = new List<List<Block>>();
            foreach(var normalClause in kb.Knowledges)
                clauses.Add(new CNFClause(normalClause).Conjunctions);

            return Main(clauses) == (ask.ToLower() == "satisfiable") ? "YES" : "NO";
        }

        private bool Main(List<List<Block>> clauses)
        {
            if (clauses.Count == 0) return true;
            var thisClause = clauses[0];
            var symbol = thisClause[0].PreviousBlock.GetContent(true).ToString();

            // need some checks here


            return Main(RemoveSymbol(clauses, new Block(symbol)))
                || Main(RemoveSymbol(clauses, new Block(symbol, true)));
        }

        private List<List<Block>> RemoveSymbol(List<List<Block>> clauses, Block symbol)
        {
            foreach(var clause in clauses)
            {
                foreach(var block in clause)
                {
                    BlockIterator.TerminableForEach(block, (currentBlock) =>
                    {

                    });
                    // 
                }
            }
            return clauses;
        }
    }
}
