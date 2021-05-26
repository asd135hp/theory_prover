using Prover.Representation.Parser;

namespace Prover.Representation.Rephraser.Type
{
    internal class Distribution : RephraserType
    {
        public Distribution()
        {
        }

        /// <summary>
        /// Distribute content to nested block using given logic (under assumption that nested block
        /// contains no more nested contents)
        /// <para>E.g: a&(b||c) <=> (a&b)||(a&c)</para>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="logic"></param>
        /// <param name="nested"></param>
        /// <returns></returns>
        private Block Distribute(bool isNegated, string content, Block nested)
        {
            // with a||(b&c)
            // isNegated = false, content = a, logic = ||, nested = b&c
            BlockIterator.ForEach(nested, (currentBlock) =>
            {
                if (currentBlock.ContentType != ContentType.Logic)
                {
                    var block = new Block(content, isNegated);                              // a (or ~a)
                    block.InsertBack(new Block(Logic))                                      // connective
                         .InsertBack(new Block(currentBlock.GetContent(true).ToString()));  // b or c in (b||c)

                    currentBlock.SetContent(block);
                }
            });
            return nested;
        }

        /// <summary>
        /// Only a||(b&c) <=> (a||b)&(a||c) is available to prioritize CNF over DNF
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>A block containing nested contents, which are distributed from original one</returns>
        public override Block Translate()
        {
            if (FirstBlock == null || Logic == null || SecondBlock == null) ThrowErrorOnFirstCheck();

            bool isFirstNormal = FirstBlock.ContentType == ContentType.Normal,
                isSecondNormal = SecondBlock.ContentType == ContentType.Normal;

            // from left to right instead of expanding from the least to the most symbols
            // no point in taking a&b or similars into consideration
            if (!isFirstNormal || !isSecondNormal)
            {
                var logic = Logic.ToString();

                // swap places: (a&b)||c into c||(a&b) locally
                if (isSecondNormal) (FirstBlock, SecondBlock) = (SecondBlock, FirstBlock);

                // a&(b||c) like
                if (FirstBlock.ContentType == ContentType.Normal)
                    return Distribute(
                        FirstBlock.IsNegated,
                        FirstBlock.GetContent(true).ToString(),
                        SecondBlock.GetContent() as Block);

                // (a||b)&(c||d) like
                if (FirstBlock.ContentType == ContentType.Nested)
                {
                    BlockIterator.ForEach(FirstBlock.GetContent() as Block, (currentBlock) =>
                    {
                        if (currentBlock.ContentType == ContentType.Normal)
                            Distribute(
                                currentBlock.IsNegated,
                                currentBlock.GetContent(true).ToString(),
                                SecondBlock.GetContent() as Block
                            );
                    });
                    return FirstBlock.GetContent() as Block;
                }
            }

            return ConcatStoredBlocks();
        }
    }
}
