using Prover.Representation;
using Prover.Representation.Parser;
using System;

namespace Prover.Engine
{
    public class ResolutionSolver : InferenceEngine
    {
        public ResolutionSolver() : base() { }

        public override string Prove(KnowledgeBase kb, string ask)
        {
            base.Prove(kb, ask);

            return "NO";
        }


        protected override bool KBEntails(Block ask)
        {
            throw new NotImplementedException();
        }
    }
}
