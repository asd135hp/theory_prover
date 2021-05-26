using System;

namespace Prover.Representation.Parser.PropositionalClause
{
    /// <summary>
    /// Doubly Linked List manager object
    /// </summary>
    public class Clause
    {
        protected Clause() { }

        public Clause(string clause) : this(ClauseParser.Parse(clause)) { }

        public Clause(Block rootBlock)
        {
            RootBlock = rootBlock;
            Length = Traverse((_0, _1) => { });
        }

        protected Block RootBlock;
        public int Length { get; private set; }

        public Block GetClause() => RootBlock;

        /// <summary>
        /// Traverse through the list, regardless of parentheses
        /// </summary>
        protected int Traverse(Action<Block, int> action, Block rootBlock = null, int index = 0)
        {
            BlockIterator.ForEach(rootBlock ?? RootBlock, (currentBlock) =>
            {
                if (currentBlock.ContentType == ContentType.Nested)
                {
                    index = Traverse(action, currentBlock.GetContent() as Block, index);
                    return;
                }
                action.Invoke(currentBlock, index++);
            });
            return index;
        }

        public override string ToString() => RootBlock.ToString();
    }
}
