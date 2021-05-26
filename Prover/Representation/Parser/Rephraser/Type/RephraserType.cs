using System;
using Prover.Representation.Parser;

namespace Prover.Representation.Rephraser.Type
{
    internal abstract class RephraserType
    {
        public RephraserType()
        {

        }

        protected Block FirstBlock, SecondBlock;
        protected PropositionalLogic Logic;

        public virtual RephraserType AddLeftBlock(Block leftBlock)
        {
            FirstBlock = leftBlock.Isolate();
            return this;
        }
        public virtual RephraserType AddLogic(PropositionalLogic logic)
        {
            Logic = logic;
            return this;
        }
        public virtual RephraserType AddRightBlock(Block rightBlock)
        {
            SecondBlock = rightBlock.Isolate();
            return this;
        }

        protected void ThrowErrorOnFirstCheck()
            => throw new FormatException("Could not start translating without none of the following:" +
                "left block, logic or right block");

        protected Block ConcatStoredBlocks()
        {
            FirstBlock.InsertBack(new Block(Logic)).InsertBack(SecondBlock);
            return FirstBlock;
        }

        public abstract Block Translate();
    }
}
