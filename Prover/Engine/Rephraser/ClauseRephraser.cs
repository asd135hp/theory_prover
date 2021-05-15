using System.Collections.Generic;
using Prover.Representation.Parser;

namespace Prover.Engine.Rephraser
{
    public enum RephraserType
    {
        Normal,
        HornClause
    }

    internal static class ClauseRephraser
    {
        /// <summary>
        /// WARNING: Unless you fully know what you are doing, do not set this property normally
        /// <para>Set type of the whole rephraser class to focus only on that type of rephrasing</para>
        /// <para>Available types: Normal (only conjunctives and disjunctives) and Horn clause</para>
        /// </summary>
        public static RephraserType RephraserType = RephraserType.Normal;

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static string Rephrase(string clause) => Rephrase(ClauseParser.Parse(clause));

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static string Rephrase(Block clause) => RephraseIntoBlock(clause).ToString();

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static Block RephraseIntoBlock(string clause) => RephraseIntoBlock(ClauseParser.Parse(clause));

        #region Main rephraser

        /// <summary>
        /// Rephrase action for normal type rephrasing
        /// </summary>
        /// <param name="currentBlock"></param>
        private static bool RephraseActionToBlock(Block currentBlock)
        {
            if (currentBlock.ContentType == ContentType.Logic)
            {
                var connective = currentBlock.GetContent() as PropositionalLogic;
                if (connective.IsBiconditional)
                {
                    RephrasingLaw.BiconditionalElimination(currentBlock);
                    return false;
                }
                if (connective.IsImplication)
                {
                    RephrasingLaw.ModusPonens(currentBlock);
                    return false;
                }
                if (connective.IsConjunction || connective.IsDisjunction)
                {
                    Block firstBlock = currentBlock.PreviousBlock,
                        secondBlock = currentBlock.NextBlock;
                    ContentType firstType = firstBlock.ContentType,
                        secondType = secondBlock.ContentType;

                    // de morgan out of nested blocks (if they are negated)
                    if (firstBlock.IsNegated) RephrasingLaw.DeMorgan(firstBlock);
                    if (secondBlock.IsNegated) RephrasingLaw.DeMorgan(secondBlock);

                    // distribution does not work on a&b but a&(b||c) instead
                    if (firstType == ContentType.Nested || secondType == ContentType.Nested)
                    {
                        RephrasingLaw.Distribution(currentBlock);
                        RephrasingLaw.Associative(currentBlock.GetContent() as Block);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Basic building block of rephraser
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static Block RephraseIntoBlock(Block clause)
        {
            switch (RephraserType)
            {
                case RephraserType.HornClause:
                    return RephrasingLaw.ToHornClauseImplication(clause);
                case RephraserType.Normal:
                    BlockIterator.TerminableForEach(clause, RephraseActionToBlock);
                    break;
                default:
                    return default;
            }

            return clause;
        }

        #endregion

        /// <summary>
        /// For ordering purpose only
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>
        /// True on either a shorter string s1 or its sum of value is less than other string s2,
        /// false otherwise
        /// </returns>
        private static bool StringComparer(string s1, string s2)
        {
            int len1 = s1.Length, len2 = s2.Length;
            if (len1 < len2) return true;
            if (len1 > len2) return false;

            for(int i = 0; i < len1; i++)
                if (s1[i].CompareTo(s2[i]) < 0) return true;
            return false;
        }

        /// <summary>
        /// Reorder the whole clause, prioritize single symbols first as well as its value
        /// (a->65, b->66 => a < b)
        /// </summary>
        /// <param name="clause"></param>
        private static Block ReorderClause(Block clause)
        {
            var blockList = new BlockIterator(clause).GetIterator();
            for (int i = 0, len = blockList.Count; i < len; ++i)
            {
                if (blockList[i].ContentType == ContentType.Logic) continue;

                Block minBlock = null;
                string min = "";
                int cache = -1;
                for (int j = i; j < len; ++j)
                {
                    var currentBlock = blockList[j];
                    if (currentBlock.ContentType == ContentType.Nested)
                        blockList[j].SetContent(ReorderClause(currentBlock.GetContent() as Block));

                    if (currentBlock.ContentType != ContentType.Logic)
                    {
                        string symbol = currentBlock.GetContent(true).ToString();
                        if (minBlock == null || StringComparer(symbol, min))
                        {
                            min = symbol;
                            minBlock = currentBlock;
                            cache = j;
                        }
                    }
                }

                if (cache != -1 && cache != i)
                {
                    // swapping order
                    blockList[cache] = blockList[i];
                    blockList[i] = minBlock;
                    blockList[i].Swap(blockList[cache]);
                }
            }
            return blockList[0];
        }

        /// <summary>
        /// Generic rephraser which rephrases the whole KB into shorter clauses
        /// </summary>
        /// <param name="clauses"></param>
        /// <returns></returns>
        public static List<string> Rephrase(List<string> clauses, RephraserType type = RephraserType.Normal)
        {
            var result = new List<string>();
            RephraserType = type;
            foreach (var clause in clauses) result.Add(Rephrase(clause));
            return result;
        }
    }
}
