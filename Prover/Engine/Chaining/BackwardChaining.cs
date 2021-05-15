using Prover.Representation.Parser;

namespace Prover.Engine.Chaining
{
    public class BackwardChaining : ChainingSolver
    {
        public BackwardChaining() : base() { }

        protected override bool KBEntails(Block ask)
        {
            // only true if input symbols are Horn clauses but in disjunctive form
            var symbol = ask.GetContent(true).ToString();
            Path.Insert(0, symbol);
            if (HornClauses.IsFact(symbol)) return true;

            var relatedSymbols = HornClauses.GetClause(symbol);
            bool result = false, firstValue = false;

            // terminable because this is the end of the chain but no facts found
            if (relatedSymbols == null) return false;

            foreach (var s in relatedSymbols)
            {
                if (!firstValue)
                {
                    firstValue = true;
                    result = KBEntails(new Block(s));
                    break;
                }
                result &= KBEntails(new Block(s));
            }

            return result;
        }
    }
}
