using Prover.Representation.Parser;

namespace Prover.Representation.Rephraser.Type
{
    internal class ModusPonens : RephraserType
    {
        public ModusPonens()
        {
            Logic = PropositionalLogic.Implication;
        }

        /// <summary>
        /// a->b <=> (~a||b)
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>The block passed to this method</returns>
        public override Block Translate()
        {
            if (FirstBlock == null || SecondBlock == null) ThrowErrorOnFirstCheck();

            // elimination
            if (Logic.IsImplication)
            {
                FirstBlock.SetNegation(!FirstBlock.IsNegated)
                    .InsertBack(new Block(PropositionalLogic.Disjunction))
                    .InsertBack(SecondBlock);

                return FirstBlock;
            }

            // reverse elimination
            // ~a||b into a=>b and a||~b into b=>a
            // accepting either one of them is negated, not both true or both false => XOR is the best choice
            if (Logic.IsDisjunction && (FirstBlock.IsNegated ^ SecondBlock.IsNegated))
            {
                FirstBlock = FirstBlock.IsNegated ? FirstBlock : SecondBlock;
                SecondBlock = FirstBlock.IsNegated ? SecondBlock : FirstBlock;

                FirstBlock.SetNegation(!FirstBlock.IsNegated)
                    .InsertBack(new Block(PropositionalLogic.Implication))
                    .InsertBack(SecondBlock);

                return FirstBlock;
            }

            return ConcatStoredBlocks();
        }
    }
}
