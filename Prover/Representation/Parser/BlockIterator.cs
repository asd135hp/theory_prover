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
        public static void TerminableForEach(Block rootBlock, Func<Block, bool> eachBlockAction)
        {
            Block currentBlock = null;

            while (currentBlock != rootBlock)
            {
                if (currentBlock == null) currentBlock = rootBlock;

                if(!eachBlockAction.Invoke(currentBlock)) return;

                // forward iteration
                currentBlock = currentBlock.NextBlock;
            }
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
        public static void TerminableReverseForEach(Block rootBlock, Func<Block, bool> eachBlockAction)
        {
            Block currentBlock = null;

            while (currentBlock != rootBlock.PreviousBlock)
            {
                if (currentBlock == null) currentBlock = rootBlock.PreviousBlock;

                if (!eachBlockAction.Invoke(currentBlock)) return;

                // backward iteration
                currentBlock = currentBlock.PreviousBlock;
            }
        }

    }
}
