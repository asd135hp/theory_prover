using System.Collections.Generic;
using Prover.Representation.Parser;

namespace Prover.Engine.Rephraser
{
    public class HornClauses
    {
        public HornClauses()
        {
            Facts = new List<string>();
            Implications = new Dictionary<string, List<string>>();
        }

        public HornClauses(List<Block> clauses): this()
        {
            foreach (var clause in clauses)
            {
                if (clause.NextBlock == clause && clause.PreviousBlock == clause)
                {
                    Facts.Add(clause.GetContent(true).ToString());
                    continue;
                }

                // safe against nested symbols inside clause or multiple clauses pointing at 1 symbol
                var symbol = clause.PreviousBlock.GetContent(true).ToString();
                var unvisited = new List<Block> { clause };
                if (!Implications.ContainsKey(symbol))
                    Implications.Add(symbol, new List<string>());

                // recursion using while loop
                while(unvisited.Count != 0)
                {
                    BlockIterator.TerminableForEach(unvisited[0], (block) =>
                    {
                        switch (block.ContentType)
                        {
                            case ContentType.Logic:
                                if ((block.GetContent() as PropositionalLogic).IsImplication)
                                    return false;
                                break;
                            case ContentType.Normal:
                                Implications[symbol].Add(block.GetContent(true).ToString());
                                break;
                            case ContentType.Nested:
                                unvisited.Add(block.GetContent() as Block);
                                break;
                        }
                        return true;
                    });

                    unvisited.RemoveAt(0);
                }

                continue;
            }
        }

        public readonly List<string> Facts;
        public readonly Dictionary<string, List<string>> Implications;

        /// <summary>
        /// Get clause from the implied symbol
        /// </summary>
        /// <param name="implicatedSymbol"></param>
        /// <returns>Null on no implications found</returns>
        public List<string> GetClause(string impliedSymbol)
        {
            if (!Implications.ContainsKey(impliedSymbol)) return null;
            return Implications[impliedSymbol];
        }

        /// <summary>
        /// Check a symbol against a list of facts, parsed internally
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool IsFact(string symbol) => Facts.Contains(symbol);
    }
}
