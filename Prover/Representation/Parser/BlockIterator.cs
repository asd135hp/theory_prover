using System;
using System.Collections.Generic;

namespace Prover.Representation.Parser
{
    public class BlockIterator
    {
        public BlockIterator(Block rootBlock)
        {
            RootBlock = rootBlock;
        }

        private readonly Block RootBlock;

        /// <summary>
        /// Get either forward iterator or backwards iterator
        /// </summary>
        /// <param name="forward">Set false to get backwards iterator</param>
        /// <returns>Forward (default) or backwards iterator</returns>
        public List<Block> GetIterator(bool forward = true)
        {
            var temp = new List<Block>();
            Block currentBlock = null;
            while (currentBlock != RootBlock)
            {
                if (currentBlock == null) currentBlock = RootBlock;
                temp.Add(currentBlock);
                currentBlock = forward ? currentBlock.NextBlock : currentBlock.PreviousBlock;
            }
            return temp;
        }

        /// <summary>
        /// Get the number of symbols, regardless connectives, in a given list of blocks
        /// (either a whole clause or a sub clause)
        /// </summary>
        /// <param name="rootBlock"></param>
        /// <param name="allNestedSymbols"></param>
        /// <returns></returns>
        public static int SymbolCount(Block rootBlock, bool allNestedSymbols = true)
        {
            int count = 0;
            ForEach(rootBlock, (block) =>
            {
                if (block.ContentType == ContentType.Normal) ++count;
                if (allNestedSymbols && block.ContentType == ContentType.Nested)
                    count += SymbolCount(rootBlock);
            });
            return count;
        }

        /// <summary>
        /// Non-terminable forward iteration
        /// </summary>
        /// <param name="eachBlockAction">Action to be taken on each accessing block</param>
        public static void ForEach(Block rootBlock, Action<Block> eachBlockAction)
        {
            Block currentBlock = null;

            while (currentBlock != rootBlock)
            {
                if (currentBlock == null) currentBlock = rootBlock;

                eachBlockAction.Invoke(currentBlock);

                // forward iteration
                currentBlock = currentBlock.NextBlock;
            }
        }

        /// <summary>
        /// Terminable forward iteration
        /// </summary>
        /// <param name="eachBlockAction">
        /// Action to be taken on each accessing block.
        /// Return false to immediately terminate the loop
        /// </param>
        /// <returns>True on iterating through all blocks, false otherwise</returns>
        public static bool TerminableForEach(Block rootBlock, Func<Block, bool> eachBlockAction)
        {
            Block currentBlock = null;

            while (currentBlock != rootBlock)
            {
                if (currentBlock == null) currentBlock = rootBlock;

                if(!eachBlockAction.Invoke(currentBlock)) return false;

                // forward iteration
                currentBlock = currentBlock.NextBlock;
            }
            return true;
        }

        /// <summary>
        /// Non-terminable backward iteration
        /// </summary>
        /// <param name="eachBlockAction">Action to be taken on each accessing block</param>
        public static void ReverseForEach(Block rootBlock, Action<Block> eachBlockAction)
        {
            Block currentBlock = null;

            while (currentBlock != rootBlock.PreviousBlock)
            {
                if (currentBlock == null) currentBlock = rootBlock.PreviousBlock;

                eachBlockAction.Invoke(currentBlock);

                // backward iteration
                currentBlock = currentBlock.PreviousBlock;
            }
        }

        /// <summary>
        /// Terminable forward iteration
        /// </summary>
        /// <param name="eachBlockAction">
        /// Action to be taken on each accessing block.
        /// Return false to immediately terminate the loop
        /// </param>
        /// <returns>True on iterating through all blocks, false otherwise</returns>
        public static bool TerminableReverseForEach(Block rootBlock, Func<Block, bool> eachBlockAction)
        {
            Block currentBlock = null;

            while (currentBlock != rootBlock.PreviousBlock)
            {
                if (currentBlock == null) currentBlock = rootBlock.PreviousBlock;

                if (!eachBlockAction.Invoke(currentBlock)) return false;

                // backward iteration
                currentBlock = currentBlock.PreviousBlock;
            }
            return true;
        }

    }
}
