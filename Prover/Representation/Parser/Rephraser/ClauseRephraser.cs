using System.Collections.Generic;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace Prover.Representation.Rephraser
{
    public enum ClauseRephraserType
    {
        CNF,
        DNF
    }

    internal class ClauseRephraser
    {
        public ClauseRephraser(ClauseRephraserType type = ClauseRephraserType.CNF)
        {
            RephraserType = type;
        }

        private ClauseRephraserType RephraserType;

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public string Rephrase(string clause) => Rephrase(ClauseParser.Parse(clause));

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public string Rephrase(Block clause) => RephraseIntoBlock(clause).ToString();

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public Block RephraseIntoBlock(string clause) => RephraseIntoBlock(ClauseParser.Parse(clause));

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public Block RephraseIntoBlock(Block clause) => RephraseIntoClause(clause).GetClause();

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public Clause RephraseIntoClause(Block clause)
        {
            if (RephraserType == ClauseRephraserType.CNF) return new CNFClause(clause);
            throw new System.NotSupportedException("Rephrase to DNF is not yet supported!");
        }

        /// <summary>
        /// Generic rephraser which rephrases the whole KB into shorter clauses
        /// </summary>
        /// <param name="clauses"></param>
        /// <returns></returns>
        public List<string> Rephrase(List<string> clauses)
        {
            var result = new List<string>();
            foreach (var clause in clauses) result.Add(Rephrase(clause));
            return result;
        }
    }
}
