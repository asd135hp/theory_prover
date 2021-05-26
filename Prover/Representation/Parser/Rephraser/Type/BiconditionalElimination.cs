using Prover.Representation.Parser;

namespace Prover.Representation.Rephraser.Type
{
    internal class BiconditionalElimination : RephraserType
    {
        public BiconditionalElimination()
        {
            Logic = PropositionalLogic.Biconditional;
        }

        /// <summary>
        /// a<=>b into (a=>b)&(b=>a) into ((~a||b)&(a||~b)) (Prioritize CNF over DNF)
        /// Warning: This method is one directional, do not use this to convert a logic block into <=>
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>The block passed to this method</returns>
        public override Block Translate()
        {
            if (FirstBlock == null || SecondBlock == null) ThrowErrorOnFirstCheck();
            if (!Logic.IsBiconditional) return ConcatStoredBlocks();

            Block cloneFirstBlock = new Block(FirstBlock.GetContent(true).ToString(), FirstBlock.IsNegated),
                cloneSecondBlock = new Block(SecondBlock.GetContent(true).ToString(), SecondBlock.IsNegated);

            // (~a||b)
            FirstBlock.SetNegation(!FirstBlock.IsNegated)
                .InsertBack(new Block(PropositionalLogic.Disjunction))
                .InsertBack(SecondBlock);

            // (a||~b)
            cloneFirstBlock
                .InsertBack(new Block(PropositionalLogic.Disjunction))
                .InsertBack(cloneSecondBlock.SetNegation(!cloneFirstBlock.IsNegated));

            // ((~a||b)&(a||~b))
            var nested = new Block(FirstBlock);
            nested.InsertBack(new Block(PropositionalLogic.Conjunction))
                  .InsertBack(new Block(cloneFirstBlock));

            return nested;
        }
    }
}
