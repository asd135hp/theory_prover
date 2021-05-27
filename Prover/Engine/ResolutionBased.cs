using System.Collections.Generic;
using Prover.Representation;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace Prover.Engine
{
    public class ResolutionBasedSolver: DPLL
    {
        public ResolutionBasedSolver() { }

        public override string GetResult(KnowledgeBase kb, string ask)
        {
            var askSymbol = ClauseParser.Parse(ask);
            var clauses = new List<Block> { askSymbol.SetNegation(!askSymbol.IsNegated) };
            foreach (var normalClause in kb.Knowledges)
                clauses.AddRange(new CNFClause(normalClause).Conjunctions);

            kb.Knowledges.Clear();
            kb.Knowledges.AddRange(clauses);

            return base.GetResult(kb, "unsatisfiable").Trim().Trim(':');
        }
    }
}
