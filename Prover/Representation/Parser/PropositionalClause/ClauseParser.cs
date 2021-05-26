using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Prover.Representation.Parser.PropositionalClause
{
    public static class ClauseParser
    {
        /// <summary>
        /// Special regex string that separate every components of a simple generic clause
        /// connetives and symbols are mutually exclusive here
        /// </summary>
        private static readonly string ClauseSeparationRegex = Regex.Replace(
            $@"({PropositionalLogic.ESCAPED_NEG})|(@@\d+|{PropositionalLogic.REGEX_MATCH_EXCEPT_LOGIC}+)|
            ({PropositionalLogic.REGEX_NO_NEG})",
            @"\s*",
            (_) => "");

        /// <summary>
        /// Dissecting the whole clause into smaller parts
        /// </summary>
        /// <example>
        /// Dissects clause a&~b into [a, &, ~b]
        /// </example>
        /// <param name="clause"></param>
        /// <returns>A list containing small parts, essential for parsing</returns>
        private static List<string> SimplifyClause(string clause)
        {
            var list = new List<string>();
            foreach (Match content in Regex.Matches(clause, ClauseSeparationRegex))
            {
                var groups = content.Groups;
                for (int i = 1; i < groups.Count; i++)
                {
                    string val = groups[i].Value.Trim();
                    if (val.Length != 0) list.Add(val);
                }
            }
            return list;
        }

        /// <summary>
        /// Parse a clause and replace all nested blocks in that clause
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="exchange">
        /// A lambda function for exchanging from an index into another list of blocks
        /// </param>
        /// <returns>
        /// A doubly linked list of knowledge blocks containing information of the passed clause
        /// (always the first block)
        /// </returns>
        private static Block ParseClause(string clause, Func<int, Block> exchange)
        {
            var simplifiedClause = SimplifyClause(clause);
            var baseBlock = new Block();
            var currentBlock = baseBlock;

            foreach (string symbol in simplifiedClause)
            {
                switch (symbol)
                {
                    case PropositionalLogic.CONJUNCTION:
                    case PropositionalLogic.DISJUNCTION:
                    case PropositionalLogic.IMPLICATION:
                    case PropositionalLogic.BICONDITIONAL:
                        // because the connective symbols are the ones that separate these knowledge blocks
                        // so after this part, a new block will be created
                        currentBlock = currentBlock
                            .SetContent(new PropositionalLogic(symbol))
                            .InsertBack(new Block());
                        break;
                    case PropositionalLogic.NEGATION:
                        currentBlock.SetNegation(true);
                        break;
                    default:
                        // found a nested block if any one of these two is true
                        // (one for normal one and one for negated one)
                        if (symbol.Length > 2 && symbol[0..2] == "@@")
                        {
                            // this call must succeed, or else an exception will be thrown
                            if (!int.TryParse(symbol[2..], out int index))
                                throw new InvalidCastException(
                                    "Could not cast string to integer." +
                                    "Possibly because of tampering with program's integrity or invalid input"
                                );

                            // exchange implied information using given index
                            currentBlock = currentBlock
                                .SetContent(exchange.Invoke(index))
                                .InsertBack(new Block());

                            break;
                        }

                        // in case of no nested blocks are found
                        currentBlock = currentBlock.SetContent(symbol).InsertBack(new Block());
                        break;
                }
            }

            // an useless block is pushed in at the end of the list when parsing, removal is necessary
            baseBlock.RemoveFront();
            return ApplyLogicPrecedence(baseBlock);
        }

        /// <summary>
        /// Ordered by the importance of the logic (& has higher precedence than || which is agreeable)
        /// <para>
        /// => and <=> has some dispute in their ordering, like which one is right in these ambiguous clauses?
        /// </para>
        /// <para>
        /// Eg: a => b <=> c means (a => b) <=> c or a => (b <=> c)?
        /// or even a => b => c means (a => b) => c or a => (b => c)?
        /// </para>
        /// <see cref="https://math.stackexchange.com/questions/3686952/the-precedence-of-logical-operators"/>
        /// </summary>
        private readonly static string[] OrderedLogic = new string[]
        {
            PropositionalLogic.CONJUNCTION,
            PropositionalLogic.DISJUNCTION
        };

        /// <summary>
        /// Apply logic precendence to the clause (if not, by default,
        /// the clause will be parsed from left to right, regardless of logic's importance)
        /// </summary>
        /// <param name="rootBlock"></param>
        /// <returns></returns>
        private static Block ApplyLogicPrecedence(Block rootBlock)
        {
            var logicGates = new List<Block>();

            BlockIterator.ForEach(rootBlock, (currentBlock) =>
            {
                switch (currentBlock.ContentType)
                {
                    case ContentType.Nested:
                        ApplyLogicPrecedence(currentBlock.GetContent() as Block);
                        break;
                    case ContentType.Logic:
                        logicGates.Add(currentBlock);
                        break;
                }
            });

            // the operations are spanned through recursion
            if(logicGates.Count > 1)
                foreach(var logic in OrderedLogic)
                    for(int i = 0; i < logicGates.Count && logicGates.Count != 1; ++i)
                    {
                        var logicBlock = logicGates[i];
                        if (logicBlock.GetContent(true).ToString() == logic)
                        {
                            // reposition the pointer to this block
                            if (logicBlock.PreviousBlock == rootBlock) rootBlock = logicBlock;

                            Block firstBlock = logicBlock.PreviousBlock.Isolate(),
                                secondBlock = logicBlock.NextBlock.Isolate();
                            var content = logicBlock.GetContent() as PropositionalLogic;

                            // connect a, &, b into a&b
                            firstBlock.InsertBack(new Block(content)).InsertBack(secondBlock);
                            logicBlock.SetContent(firstBlock);

                            // filter out grouped one
                            logicGates.RemoveAt(i);
                            --i;
                        }
                    }

            return rootBlock;
        }

        /// <summary>
        /// Main parser where this method will iterate through all sub-clauses (including nested ones)
        /// </summary>
        /// <param name="clause"></param>
        public static Block Parse(string clause)
        {
            var storedStartIndex = new List<int>();
            var storedNestedBlocks = new List<Block>();
            int level = 0, i = 0;
            string cloneClause = clause;

            foreach (char ch in cloneClause)
            {
                // basic setup for nested clauses
                // no nesting with []
                switch (ch)
                {
                    case '(':
                        storedStartIndex.Add(i + 1);
                        level++;
                        break;
                    case ')':
                        // get the last item in the list
                        int firstIndex = storedStartIndex.Last();
                        storedStartIndex.RemoveAt(storedStartIndex.Count - 1);

                        // push in this block of nested content
                        storedNestedBlocks.Add(
                            ParseClause(
                                cloneClause[firstIndex..i],
                                (index) => storedNestedBlocks[index]
                            )
                        );

                        // no decrease when firstIndex is 0 (out of range exception)
                        firstIndex -= firstIndex > 0 ? 1 : 0;

                        // remove content of this nested block and
                        // insert the index that this block is stored instead
                        string replacement = $"@@{storedNestedBlocks.Count - 1}";
                        cloneClause = cloneClause.Remove(firstIndex, i - firstIndex + 1)
                                                 .Insert(firstIndex, replacement);

                        // readjust index
                        i = firstIndex + replacement.Length;
                        level--;
                        continue;
                    default:
                        break;
                }
                i++;
            }

            if (level != 0)
            {
                storedNestedBlocks.Clear();
                throw new ArgumentException("Provided input contains an ill-formed nesting query");
            }

            // add to the list of possible clauses parsed ones
            var block = ParseClause(cloneClause, (index) => storedNestedBlocks[index]);
            storedNestedBlocks.Clear();
            return block;
        }

        /// <summary>
        /// Parse the given clause into a Clause object instead of primitive Block object
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static Clause ParseToClause(string clause) => new Clause(Parse(clause));

        /// <summary>
        /// Parse the given clause into a CNF Clause object instead of primitive Block object
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        public static Clause ParseToCNFClause(string clause) => new CNFClause(Parse(clause));
    }
}
