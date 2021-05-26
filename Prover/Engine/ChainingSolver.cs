using System.Collections.Generic;
using Prover.Representation;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;
using Prover.Representation.Rephraser;

namespace Prover.Engine
{
    public abstract class ChainingSolver : InferenceEngine
    {
        public ChainingSolver() : base() { Path = new List<string>(); }

        protected readonly List<string> Path;
        protected HornClauses HornClauses;

        public override string Prove(KnowledgeBase kb, string ask)
        {
            HornClauses = new HornClauses(kb.Knowledges);
            base.Prove(kb, ask);
            return KBEntails(ClauseParser.Parse(ask)) ? $"YES: {string.Join(", ", Path)}" : "NO";
        }

        protected override bool KBEntails(Block ask) => false;
    }
}
