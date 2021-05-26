using Prover.Representation.Parser;

namespace Prover.Representation.Rephraser.Type
{
    internal class TruthfulTheorems : RephraserType
    {
        public TruthfulTheorems() { }

        private Block RelatedTheorems()
        {
            string firstContent = FirstBlock.GetContent(true).ToString(),
                secondContent = SecondBlock.GetContent(true).ToString();
            bool isConjunctive = Logic.IsConjunction;

            if (firstContent == secondContent)
                return FirstBlock.IsNegated != SecondBlock.IsNegated ?
                    new Block(isConjunctive ? "false" : "true") :   // complement
                    FirstBlock;                                     // idempotent

            switch (firstContent)
            {
                case "true": return isConjunctive ? SecondBlock : new Block("true");    // identity
                case "false": return isConjunctive ? new Block("false") : SecondBlock;  // annulment
            }

            return secondContent switch
            {
                "true" => isConjunctive ? FirstBlock : new Block("true"),               // identity
                "false" => isConjunctive ? new Block("false") : SecondBlock,            // annulment
                _ => ConcatStoredBlocks(),
            };
        }

        /// <summary>
        /// Truths:
        /// <para>Complement: a&~a <=> false or a||~a <=> true</para>
        /// <para>Annulment: a&false <=> false or a||true <=> true</para>
        /// <para>Idempotent: a&a <=> a or a||a <=> a</para>
        /// <para>Identity: a||false <=> a or a&true <=> a</para>
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>The block passed to this method</returns>
        public override Block Translate()
        {
            if (FirstBlock == null || Logic == null || SecondBlock == null) ThrowErrorOnFirstCheck();
            if (!Logic.IsConjunction && !Logic.IsDisjunction)
                throw new System.FormatException("Could not check for truthful theorems using any connectives" +
                    "other than AND(&) or OR(||)");

            return RelatedTheorems();
        }
    }
}
