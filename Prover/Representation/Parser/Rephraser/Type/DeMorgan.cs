using Prover.Representation.Parser;

namespace Prover.Representation.Rephraser.Type
{
    internal class DeMorgan : RephraserType
    {
        public DeMorgan()
        {
        }

        /// <summary>
        /// From ~(a||b) to ~a&~b and ~(a&b) to ~a||~b (vice-versa)
        /// </summary>
        /// <param name="blockWithNestedContent"></param>
        /// <returns>The block passed to this method</returns>
        public override Block Translate()
        {
            if (FirstBlock == null && (FirstBlock = SecondBlock) == null)
                ThrowErrorOnFirstCheck();

            Block blockWithNestedContent = FirstBlock;

            if (blockWithNestedContent.ContentType == ContentType.Nested)
            {
                blockWithNestedContent.SetNegation(!blockWithNestedContent.IsNegated);
                BlockIterator.ForEach(blockWithNestedContent.GetContent() as Block, (currentBlock) =>
                {
                    switch (currentBlock.ContentType)
                    {
                        case ContentType.Logic:
                            (currentBlock.GetContent() as PropositionalLogic).Invert();
                            break;
                        default:
                            currentBlock.SetNegation(!currentBlock.IsNegated);
                            break;
                    }
                });
            }
            return blockWithNestedContent;
        }
    }
}
