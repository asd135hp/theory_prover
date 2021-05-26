using System.Linq;
using System.Collections.Generic;
using Prover.Representation.Parser;

namespace Prover.Engine.Chaining
{
    public class ForwardChaining : ChainingSolver
    {
        public ForwardChaining(): base() { }

        protected override bool KBEntails(Block ask)
        {
            // copy assignment, only available in this local session only
            // it does not affect what has been recorded previously in this program session;
            // meaning this prover (and backward chaining as well) could run as many time as possible
            // in one program session without reinitializing anything
            var facts = new List<string>(HornClauses.Facts);
            var implications = new Dictionary<string, List<List<string>>>(HornClauses.Implications);
            string askSymbol = ask.GetContent(true).ToString();

            if (facts.Contains(askSymbol))
            {
                Path.Add(askSymbol);
                return true;
            }

            // forward chaining from facts
            for(int index = 0; index < facts.Count; ++index)
            {
                var fact = facts[index];
                Path.Add(fact);

                // repeatedly scanning the trimmed kb for the goal
                foreach (var (implied, requirementList) in implications)
                {
                    // in case of multiple clauses implying to the same symbol 
                    foreach(var requirements in requirementList)
                    {
                        // dig down if there is a fact amongst current requirements for implication
                        if (requirements.Contains(fact))
                        {
                            var restOfRequirements = requirements.Where((s) => s != fact);
                            bool satisfied = true;
                            foreach (var requirement in restOfRequirements)
                                if (!facts.Contains(requirement))
                                {
                                    // no point in searching if one of the requirements is not a fact
                                    satisfied = false;
                                    break;
                                }

                            // all requirements are facts -> implied symbol is also a fact
                            if (satisfied)
                            {
                                // trim kb when an implied fact found
                                facts.Add(implied);
                                implications.Remove(implied);

                                if (askSymbol == implied)
                                {
                                    Path.Add(askSymbol);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
