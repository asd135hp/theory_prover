using System.Collections.Generic;
using Prover.Representation.Parser;

namespace Prover.Representation
{
    /// <summary>
    /// Represents all unique values that knowledge base / a clause has
    /// </summary>
    public class UniqueSymbols
    {
        public UniqueSymbols(Block rootBlock)
        {
            UniqueValues = new List<string>();
            IterateThroughBlockLists(rootBlock);
        }

        public UniqueSymbols(KnowledgeBase kb)
        {
            UniqueValues = new List<string>();

            foreach (var blockList in kb.Knowledges) IterateThroughBlockLists(blockList);
        }

        private void IterateThroughBlockLists(Block block)
        {
            BlockIterator.ForEach(block, (currentBlock) =>
            {
                // recursion for removal of deep nesting
                object content = currentBlock.GetContent();
                if (content is Block) {
                    IterateThroughBlockLists(content as Block);
                    return;
                }

                // we do not want any logic in here
                if (content is PropositionalLogic) return;

                // definitely a string, containing a symbol
                string value = content as string;
                if (!UniqueValues.Contains(value)) UniqueValues.Add(value);
            });
        }

        public List<string> UniqueValues { get; private set; }
    }
}
