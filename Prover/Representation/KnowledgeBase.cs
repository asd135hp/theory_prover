using System.Collections.Generic;
using System.Text.RegularExpressions;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace Prover.Representation
{
    /// <summary>
    /// This KB must be able to parse every single generic clause using given connectives
    /// </summary>
    public class KnowledgeBase
    {
        public KnowledgeBase(string tell)
        {
            // accessing a clause without any whitespaces
            string noWhitespaceKB = Regex.Replace(tell, @"\s+", (_) => "");

            Knowledges = new List<Block>();

            // clauses in tell are separated by ';'
            foreach (var clause in noWhitespaceKB.Split(';'))
                if(clause.Length != 0) Knowledges.Add(ClauseParser.Parse(clause));
        }

        /// <summary>
        /// A list of knowledges
        /// </summary>
        public List<Block> Knowledges { get; private set; }

        /// <summary>
        /// Debugging method
        /// </summary>
        public override string ToString()
        {
            string result = "";
            int count = 0;
            foreach (var knowledge in Knowledges)
            {
                result += knowledge.ToString() + (++count != Knowledges.Count ? "\n" : "");
            }
            return result;
        }
    }
}
