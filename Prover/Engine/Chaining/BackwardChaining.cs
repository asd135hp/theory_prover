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

            if (!Path.Contains(symbol)) Path.Insert(0, symbol);
            if (HornClauses.IsFact(symbol)) return true;

            var relatedSymbols = HornClauses.GetClause(symbol);

            // terminable because this is the end of the chain but no facts found
            if (relatedSymbols == null) return false;

            bool result = false;
            foreach(var relatedList in relatedSymbols)
            {
                bool innerResult = false, firstValue = false;
                foreach (var s in relatedList)
                {
                    if (!firstValue)
                    {
                        firstValue = true;
                        innerResult = KBEntails(new Block(s));
                        continue;
                    }
                    innerResult &= KBEntails(new Block(s));
                }

                // one innerResult equals to true means that
                // this symbol is indeed a fact without anymore consideration
                result |= innerResult;
            }

            if (result) HornClauses.Facts.Add(symbol);

            return result;
        }
    }
}
