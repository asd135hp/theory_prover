using System;
using System.Collections.Generic;
using Prover.Representation.Parser;

namespace Prover.Engine.Rephraser
{
    internal static class RephrasingLaw
    {
        #region Elimination

        /// <summary>
        /// a->b <=> (~a||b)
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>The block passed to this method</returns>
        public static Block ModusPonens(Block logicBlock)
        {
            Block firstBlock = logicBlock.PreviousBlock,
                secondBlock = logicBlock.NextBlock;
            var connective = logicBlock.GetContent() as PropositionalLogic;

            // elimination
            if (connective.IsImplication)
            {
                firstBlock.Isolate().SetNegation(!firstBlock.IsNegated)
                    .InsertBack(new Block(PropositionalLogic.Disjunction))
                    .InsertBack(secondBlock.Isolate());

                return logicBlock.SetContent(firstBlock);
            }

            // reverse elimination
            // ~a||b into a=>b and a||~b into b=>a
            // accepting either one of them is negated, not both true or both false => XOR is the best choice
            if (connective.IsDisjunction && (firstBlock.IsNegated ^ secondBlock.IsNegated))
            {
                firstBlock = firstBlock.IsNegated ? firstBlock : secondBlock;
                secondBlock = firstBlock.IsNegated ? secondBlock : firstBlock;

                firstBlock.Isolate().SetNegation(!firstBlock.IsNegated)
                    .InsertBack(new Block(PropositionalLogic.Implication))
                    .InsertBack(secondBlock.Isolate());

                return logicBlock.SetContent(firstBlock);
            }

            return logicBlock;
        }

        /// <summary>
        /// a<=>b into (a=>b)&(b=>a) into (~a&~b)||(a&b)
        /// Warning: This method is one directional, do not use this to convert a logic block into <=>
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>The block passed to this method</returns>
        public static Block BiconditionalElimination(Block logicBlock)
        {
            if (!((logicBlock.GetContent() as PropositionalLogic)?.IsBiconditional ?? false))
                return logicBlock;

            Block firstBlock = logicBlock.RemoveFront(),
                secondBlock = logicBlock.RemoveBack(),
                cloneFirstBlock = new Block(firstBlock.GetContent(true).ToString(), firstBlock.IsNegated),
                cloneSecondBlock = new Block(secondBlock.GetContent(true).ToString(), secondBlock.IsNegated);

            // (~a&~b)
            firstBlock.SetNegation(!firstBlock.IsNegated)
                .InsertBack(new Block(PropositionalLogic.Conjunction))
                .InsertBack(secondBlock.SetNegation(!secondBlock.IsNegated));

            // (a&b)
            cloneFirstBlock
                .InsertBack(new Block(PropositionalLogic.Conjunction))
                .InsertBack(cloneSecondBlock);

            // ((~a&~b)||(a&b))
            var nested = new Block(firstBlock);
            nested.InsertBack(new Block(PropositionalLogic.Disjunction))
                  .InsertBack(new Block(cloneFirstBlock));

            return logicBlock.SetContent(nested);
        }

        #endregion

        #region Conjunction/Disjunction conversions

        /// <summary>
        /// From ~(a||b) to ~a&~b and ~(a&b) to ~a||~b (vice-versa)
        /// </summary>
        /// <param name="blockWithNestedContent"></param>
        /// <returns>The block passed to this method</returns>
        public static Block DeMorgan(Block blockWithNestedContent)
        {
            if(blockWithNestedContent.ContentType == ContentType.Nested)
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

        #region Distribution

        /// <summary>
        /// Distribute content to nested block using given logic (under assumption that nested block
        /// contains no more nested contents)
        /// <para>E.g: a&(b||c) <=> (a&b)||(a&c)</para>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="logic"></param>
        /// <param name="nested"></param>
        /// <returns></returns>
        private static Block Distribute(bool isNegated, string content, string logic, Block nested)
        {
            BlockIterator.ForEach(nested, (currentBlock) =>
            {
                if (currentBlock.ContentType == ContentType.Normal)
                {
                    var block = new Block(content, isNegated);                              // a (or ~a)
                    block.InsertBack(new Block(new PropositionalLogic(logic)))              // connective
                         .InsertBack(new Block(currentBlock.GetContent(true).ToString()));  // b or c in (b||c)

                    currentBlock.SetContent(block);
                }
            });
            return nested;
        }

        /// <summary>
        /// a&(b||c) <=> (a&b)||(a&c) or a||(b&c) <=> (a||b)&(a||c)
        /// <para>
        /// Constrain: the logic block must be either & or || for the conversion to take place
        /// as well as no too deep nests taken place
        /// </para>
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>A block containing nested contents, which are distributed from original one</returns>
        public static Block Distribution(Block logicBlock)
        {
            Block firstBlock = logicBlock.PreviousBlock,
                secondBlock = logicBlock.NextBlock;

            // from left to right instead of expanding from the least to the most symbols
            if(secondBlock.ContentType == ContentType.Nested)
            {
                string logic = logicBlock.GetContent(true).ToString();
                firstBlock.Isolate(true);
                secondBlock.Isolate(true);

                // a&(b||c) like
                if(firstBlock.ContentType == ContentType.Normal)
                    return logicBlock.SetContent(
                        Distribute(
                            firstBlock.IsNegated,
                            firstBlock.GetContent(true).ToString(),
                            logic,
                            secondBlock.GetContent() as Block));

                // (a||b)&(c||d) like
                if(firstBlock.ContentType == ContentType.Nested)
                {
                    BlockIterator.ForEach(firstBlock.GetContent() as Block, (currentBlock) =>
                    {
                        if (currentBlock.ContentType == ContentType.Normal)
                            currentBlock.SetContent(
                                Distribute(
                                    currentBlock.IsNegated,
                                    currentBlock.GetContent(true).ToString(),
                                    logic,
                                    secondBlock.GetContent() as Block
                                )
                            );
                    });
                    return logicBlock.SetContent(firstBlock.GetContent() as Block);
                }
            }
            return logicBlock;
        }

        #endregion

        #endregion

        #region Truth conversions

        /// <summary>
        /// Truths:
        /// <para>Complement: a&~a <=> false or a||~a <=> true</para>
        /// <para>Annulment: a&false <=> false or a||true <=> true</para>
        /// <para>Idempotent: a&a <=> a or a||a <=> a</para>
        /// <para>Identity: a||false <=> a or a&true <=> a</para>
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>The block passed to this method</returns>
        public static Block TruthfulTheorems(Block logicBlock)
        {
            Block firstBlock = logicBlock.PreviousBlock,
                secondBlock = logicBlock.NextBlock;
            string firstContent = firstBlock.GetContent(true).ToString(),
                secondContent = secondBlock.GetContent(true).ToString();

            bool foundFirst = true;
            PropositionalLogic connective = logicBlock.GetContent() as PropositionalLogic;

            if (connective.IsConjunction)
            {
                if (firstContent == secondContent)
                {
                    // complement
                    if (firstBlock.IsNegated != secondBlock.IsNegated) logicBlock.SetContent("false");
                    // idempotent
                    else logicBlock.SetNegation(firstBlock.IsNegated).SetContent(firstContent);
                }

                switch (firstContent)
                {
                    case "true":
                        // identity
                        logicBlock.SetNegation(secondBlock.IsNegated).SetContent(secondContent);
                        break;
                    case "false":
                        // annulment
                        logicBlock.SetContent("false");
                        break;
                    default:
                        foundFirst = false;
                        break;
                }

                if (!foundFirst)
                {
                    switch (secondContent)
                    {
                        case "true":
                            // identity
                            logicBlock.SetNegation(firstBlock.IsNegated).SetContent(firstContent);
                            break;
                        case "false":
                            // annulment
                            logicBlock.SetContent("false");
                            break;
                    }
                }
            }
            else if (connective.IsDisjunction)
            {
                if (firstContent == secondContent)
                {
                    // complement
                    if (firstBlock.IsNegated != secondBlock.IsNegated) logicBlock.SetContent("true");
                    // idempotent
                    else logicBlock.SetNegation(firstBlock.IsNegated).SetContent(firstContent);
                }

                switch (firstContent)
                {
                    case "true":
                        // annulment
                        logicBlock.SetContent("true");
                        break;
                    case "false":
                        // identity
                        logicBlock.SetNegation(secondBlock.IsNegated).SetContent(secondContent);
                        break;
                    default:
                        foundFirst = false;
                        break;
                }

                if (!foundFirst)
                {
                    switch (secondContent)
                    {
                        case "true":
                            // annulment
                            logicBlock.SetContent("true");
                            break;
                        case "false":
                            // identity
                            logicBlock.SetNegation(firstBlock.IsNegated).SetContent(firstContent);
                            break;
                    }
                }
            }

            if (logicBlock.ContentType != ContentType.Logic)
            {
                firstBlock.Isolate(true);
                secondBlock.Isolate(true);
            }

            return logicBlock;
        }

        #endregion

        #region Association

        /// <summary>
        /// Associate two same blocks together (and calls idempotent law too)
        /// </summary>
        /// <param name="storedValues"></param>
        /// <returns></returns>
        private static Block Associates(Dictionary<string, List<Block>> storedValues)
        {
            Block newBlock = null;
            foreach(var (symbol, blocks) in storedValues)
            {
                if (storedValues.Count < 2) continue;

                // guarantees to be the same connective across storedValues for each block
                // a&b&~a&b&~a
                Block storedBlock = null;
                foreach(var block in blocks)
                {
                    if(storedBlock == null)
                    {
                        storedBlock = block;
                        continue;
                    }

                    // ..&a
                    if(block.PreviousBlock.ContentType == ContentType.Logic)
                    {
                        // swap positions: a&b&a into a&a&b
                        storedBlock.NextBlock.NextBlock.Swap(block);

                        // convert into single block using theorems
                        // ignoring true and false constants
                        storedBlock = TruthfulTheorems(storedBlock.NextBlock);
                    }
                }
            }
            return newBlock;
        }

        /// <summary>
        /// Prioritize same symbol grouping for idempotent theorem
        /// </summary>
        /// <param name="logicBlock"></param>
        /// <returns>The block passed to this method</returns>
        public static Block Associative(Block rootBlock)
        {
            var categories = new Dictionary<string, List<Block>>();
            string currentLogic = "";
            Block newBlock = null;
            BlockIterator.ForEach(rootBlock, (currentBlock) =>
            {
                string content = currentBlock.GetContent(true).ToString();
                switch (currentBlock.ContentType)
                {
                    case ContentType.Logic:
                        if(content != currentLogic)
                        {
                            if (newBlock == null) newBlock = Associates(categories);
                            else newBlock.InsertBack(new Block(new PropositionalLogic(currentLogic)))
                                         .InsertBack(Associates(categories));

                            // refresh logic cache
                            currentLogic = content;
                        }
                        break;
                    case ContentType.Normal:
                    case ContentType.Nested:
                        if (categories.ContainsKey(content))
                        {
                            categories[content] = new List<Block> { currentBlock };
                            break;
                        }

                        categories[content].Add(currentBlock);
                        break;
                }
            });

            return rootBlock;
        }

        #endregion

        #region Convert to Horn clause's implication form

        /// <summary>
        /// Nested block (or parentheses) removal. This method takes about O(2n) per run
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private static Block NestedBlockRemoval(Block block)
        {
            // remove some connectives for Horn clause disjunctive form: => and <=>
            BlockIterator.ForEach(block, (currentBlock) =>
            {
                if (currentBlock.ContentType == ContentType.Logic)
                {
                    var logic = currentBlock.GetContent() as PropositionalLogic;
                    if (logic.IsImplication) ModusPonens(currentBlock);
                    if (logic.IsBiconditional) BiconditionalElimination(currentBlock);
                }
            });

            // De morgan where possible
            BlockIterator.ForEach(block, (currentBlock) =>
            {
                // parentheses removal 
                if(currentBlock.ContentType == ContentType.Nested)
                {
                    // flatten the nested block
                    currentBlock.PreviousBlock.ExtendBack(
                        NestedBlockRemoval(currentBlock.GetContent() as Block)
                    );
                    // remove current block for correct display
                    currentBlock.Isolate(true);
                }
            });

            return block.IsNegated ? DeMorgan(block) : block;
        }

        /// <summary>
        /// Check all the content of the clause passed to this method, including nested ones
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="positiveCount"></param>
        /// <returns></returns>
        private static (Block, int) CheckHornClause(Block clause, int positiveCount = 0)
        {
            bool allDisjunction = true;
            Block onlyPositive = null;

            // check the content of the clause after flattened down the whole clause
            BlockIterator.ForEach(clause, (currentBlock) =>
            {
                switch (currentBlock.ContentType)
                {
                    case ContentType.Logic:
                        if (!(currentBlock.GetContent() as PropositionalLogic).IsDisjunction)
                        {
                            allDisjunction = false;
                            break;
                        }
                        break;
                    case ContentType.Normal:
                        if (!currentBlock.IsNegated)
                        {
                            if (onlyPositive == null)
                            {
                                onlyPositive = currentBlock;
                                break;
                            }
                            ++positiveCount;
                        }
                        break;
                    case ContentType.Nested:
                        var t = CheckHornClause(currentBlock.GetContent() as Block, positiveCount);
                        onlyPositive = t.Item1;
                        positiveCount += t.Item2;
                        break;
                }
            });

            // ~a||b||c||d||~e||~f is wrong
            if (positiveCount > 1) throw new ArgumentException(
                 $"Provided clause: {clause} contains too many positives" +
                 "that is against Horn clause's definition");

            // ~a||~b||c||(e&d) is wrong (must be in disjunctive form) - debatable
            if (!allDisjunction) throw new ArgumentException(
                $"Provided clause: {clause} contains non disjunction inside this clause's disjunctive form");

            return (onlyPositive, positiveCount);
        }

        /// <summary>
        /// Convert the whole clause into disjunctive form and then into implication form
        /// </summary>
        /// <param name="clause"></param>
        /// <returns>New clause that has been properly filtered into a Horn clause</returns>
        public static Block ToHornClauseImplication(Block clause)
        {
            // this is a fact, no point in converting
            if (clause.NextBlock == clause && clause.PreviousBlock == clause) return clause;

            string error = $"Provided clause: {clause}";
            bool allConjunction = true, tooManyImpliedSymbols = false;
            int implications = 0;

            // remove all nested blocks
            BlockIterator.ForEach(clause, (currentBlock) =>
            {
                switch (currentBlock.ContentType)
                {
                    case ContentType.Logic:
                        var logic = currentBlock.GetContent() as PropositionalLogic;
                        if (logic.IsDisjunction) allConjunction = false;
                        if (logic.IsImplication) ++implications;
                        break;
                    case ContentType.Nested:
                        if (implications > 0)
                        {
                            // this is guaranteed because =>'s precedence is less than & and ||
                            tooManyImpliedSymbols = true;
                            break;
                        }
                        NestedBlockRemoval(clause.GetContent() as Block);
                        break;
                }
            });

            // clause contains 1 implication, no need to check for disjunctive form anymore
            if(implications == 1)
            {
                // a||b=>c and a=>b&c are both wrong
                if(!allConjunction || tooManyImpliedSymbols)
                    throw new ArgumentException(
                        $"{error} contains too many implied symbols or its normal form " +
                        "contains disjunction, which is against Horn clause's definition");
                return clause;
            }

            // a=>b=>c is wrong
            if (implications > 1) throw new ArgumentException($"{error} contains too many implications");

            Block onlyPositive = CheckHornClause(clause).Item1;

            // found no positives: ~a||~b||~c into a&b&c=>false
            if (onlyPositive == null)
                return DeMorgan(new Block(clause, true))
                    .InsertBack(new Block(PropositionalLogic.Implication))
                    .InsertBack(new Block("false"));

            // found the positive: ~a||b||~c into (a&c)=>b
            Block disjunction = onlyPositive.RemoveFront(true);
            onlyPositive.Isolate();

            // de morgan the whole clause except for the only positive
            return DeMorgan(new Block(clause, true))
                .InsertBack(new Block(PropositionalLogic.Implication))
                .InsertBack(onlyPositive);
        }

        #endregion
    }
}
