namespace Prover.Representation.Parser
{
    public enum ContentType
    {
        Normal,
        Nested,
        Logic,
        None
    }

    /// <summary>
    /// Doubly Linked List like data structure, representing blocks of clauses
    /// <para>One block represents any one of the following: ~a, a, &, (a&~b)</para>
    /// </summary>
    public class Block
    {
        public Block(object content = null, bool isNegated = false)
        {
            SetContent(content);
            IsNegated = isNegated;
            PreviousBlock = NextBlock = this;
        }

        #region Getters/Setters

        public bool IsNegated { get; private set; }
        private object Content;

        public Block PreviousBlock { get; private set; }
        public Block NextBlock { get; private set; }

        /// <summary>
        /// Set negation (~) to either true or false
        /// </summary>
        /// <param name="isNegated"></param>
        /// <returns>This block for method chaining</returns>
        public Block SetNegation(bool isNegated)
        {
            IsNegated = isNegated;
            return this;
        }

        /// <summary>
        /// Set content, could pass nested ones or propositional logic
        /// </summary>
        /// <param name="content">Any types of content</param>
        /// <returns>This block for method chaining</returns>
        /// <exception cref="System.ArgumentException">
        /// Wrong content - or wrong type of symbol passed through this method
        /// </exception>
        public Block SetContent(object content)
        {
            if (content == null || content is string || content is Block || content is PropositionalLogic)
            {
                ContentType = content == null ? ContentType.None :
                    (content is Block ? ContentType.Nested :
                        (content is PropositionalLogic ? ContentType.Logic : ContentType.Normal));

                Content = content;
                return this;
            }

            throw new System.ArgumentException(
                "Could not read symbol." +
                "Symbol must be either one of these followings: string, Block, PropositionalLogic"
            );
        }

        /// <summary>
        /// Get content, either a nested one or normal ones
        /// </summary>
        /// <returns>
        /// If toString is true, a stringified clause is returned.
        /// Else, a raw content object is returned
        /// </returns>
        public object GetContent(bool toString = false)
        {
            // raw content object
            if (!toString) return Content;
            
            // to string of nested content
            if (Content is Block) return $"({Content as Block})";

            // dependent on object type, no throwing but returning an empty string instead
            // which could be dangerous...
            return (Content as PropositionalLogic ?? Content)?.ToString() ?? "";
        }

        /// <summary>
        /// Get content type, either a normal one or a nested one for current block
        /// </summary>
        /// <returns></returns>
        public ContentType ContentType { get; private set; }

        #endregion

        #region Insertion

        /// <summary>
        /// Insert a block before this block
        /// </summary>
        /// <param name="block"></param>
        /// <returns>Newly inserted block</returns>
        public Block InsertFront(Block block)
        {
            if (block == this) return this;

            // chaining for new block
            block.PreviousBlock = PreviousBlock;
            block.NextBlock = this;

            // rechaining for this block
            PreviousBlock.NextBlock = block;
            PreviousBlock = block;
            return block;
        }

        /// <summary>
        /// Insert a block after this block
        /// </summary>
        /// <param name="block"></param>
        /// <returns>Newly inserted block</returns>
        public Block InsertBack(Block block)
        {
            if (block == this) return this;

            // chaining for new block
            block.PreviousBlock = this;
            block.NextBlock = NextBlock;

            // rechaining for this block
            NextBlock.PreviousBlock = block;
            NextBlock = block;

            return block;
        }

        #endregion

        #region Removal

        /// <summary>
        /// Isolate this block from the list
        /// </summary>
        /// <returns>Isloated block</returns>
        public Block Isolate(bool fullIsolation = false)
        {
            Block prev = PreviousBlock, next = NextBlock;
            prev.NextBlock = next;
            next.PreviousBlock = prev;

            PreviousBlock = NextBlock = fullIsolation ? null : this;

            return this;
        }

        /// <summary>
        /// Remove the front block (or this block if the list contains only one element)
        /// </summary>
        /// <returns>Removed block</returns>
        public Block RemoveFront(bool fullIsolation = false) => PreviousBlock.Isolate(fullIsolation);

        /// <summary>
        /// Remove the back block (or this block if the list contains only one element)
        /// </summary>
        /// <returns>Removed block</returns>
        public Block RemoveBack(bool fullIsolation = false) => NextBlock.Isolate(fullIsolation);

        #endregion

        #region Swapper

        /// <summary>
        /// Swap this block to other block in the same list (or other list).
        /// Method's returning value could be ambiguous if you are not careful!
        /// <para>It is recommended to not use the returning value of this method for method chaining</para>
        /// </summary>
        /// <param name="otherBlock"></param>
        /// <returns>Swapped block, representing current index in the list</returns>
        public Block Swap(Block otherBlock)
        {
            Block thisNext = NextBlock, thisPrev = PreviousBlock,
                otherNext = otherBlock.NextBlock, otherPrev = otherBlock.PreviousBlock;

            if(PreviousBlock == otherBlock)
            {
                PreviousBlock = otherPrev;
                NextBlock = thisNext.PreviousBlock = otherBlock;
                otherBlock.PreviousBlock = otherPrev.NextBlock = this;
                otherBlock.NextBlock = thisNext;
                return otherBlock;
            }

            if(NextBlock == otherBlock)
            {
                NextBlock = otherNext;
                PreviousBlock = thisPrev.NextBlock = otherBlock;
                otherBlock.PreviousBlock = thisPrev;
                otherBlock.NextBlock = otherNext.PreviousBlock = this;
                return otherBlock;
            }

            thisNext.PreviousBlock = thisPrev.NextBlock = otherBlock;
            otherNext.PreviousBlock = otherPrev.NextBlock = this;

            PreviousBlock = otherPrev;
            NextBlock = otherNext;

            otherBlock.PreviousBlock = thisPrev;
            otherBlock.NextBlock = thisNext;

            return otherBlock;
        }

        #endregion

        #region Extension
        
        /// <summary>
        /// Extend current list to the front of this list's head
        /// </summary>
        /// <param name="newList"></param>
        /// <returns>The first block of the new list</returns>
        public Block ExtendFront(Block newList)
        {
            Block thisPrev = PreviousBlock,
                otherBegin = newList, otherEnd = newList.PreviousBlock;

            PreviousBlock = otherEnd;
            thisPrev.NextBlock = otherBegin;
            otherBegin.PreviousBlock = thisPrev;
            otherEnd.NextBlock = this;

            return newList;
        }

        /// <summary>
        /// Extend current list to the back of this list's head
        /// </summary>
        /// <param name="newList"></param>
        /// <returns>The first block of the new list</returns>
        public Block ExtendBack(Block newList)
        {
            Block thisNext = NextBlock,
                otherBegin = newList, otherEnd = newList.PreviousBlock;

            NextBlock = otherBegin;
            thisNext.PreviousBlock = otherEnd;
            otherBegin.PreviousBlock = this;
            otherEnd.NextBlock = thisNext;

            return newList;
        }

        #endregion

        /// <summary>
        /// Represent the whole linked list as a string again
        /// </summary>
        /// <returns>A string in form of ~a& to represent a certain part of the list</returns>
        public override string ToString()
        {
            string result = "";

            // forward iterate through the list from this block onwards
            BlockIterator.ForEach(this, (currentBlock) =>
            {
                // one block of content is pushed into the result (no whitespaces in between)
                result +=
                    (currentBlock.IsNegated ? PropositionalLogic.NEGATION : "") +
                    currentBlock.GetContent(true);
            });

            return result;
        }
    }
}
