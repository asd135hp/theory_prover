using System.Collections.Generic;
using Prover.Representation.TruthTable;
using Prover.Representation.Rephraser.Type;

namespace Prover.Representation.Parser.PropositionalClause
{
    public class CNFClause : Clause
    {
        public CNFClause(string clause) : this(ClauseParser.Parse(clause)) { }

        public CNFClause(Block rootBlock): base(rootBlock)
        {
            RootBlock = rootBlock;
            Conjunctions = new List<Block>();
            ToCNF(RootBlock);
        }

        public List<Block> Conjunctions { get; private set; }

        #region Unstable/Unusable
        /// <summary>
        /// Since this clause is full of disjunctions, might as well flatten them
        /// </summary>
        private void Flatten()
        {
            bool anyChanges = true;
            while (anyChanges)
            {
                anyChanges = false;
                BlockIterator.TerminableForEach(RootBlock, (currentBlock) =>
                {
                    if (currentBlock.ContentType == ContentType.Nested)
                    {
                        Block next = currentBlock.NextBlock,
                            current = currentBlock.Isolate(true).GetContent() as Block;
                        anyChanges = true;

                        next.ExtendFront(current);
                        if (currentBlock.Equals(RootBlock)) RootBlock = current;

                        return false;
                    }

                    return true;
                });
            }
        }

        /// <summary>
        /// Indirectly call for Translate of RephraserType object
        /// </summary>
        /// <param name="rootBlock"></param>
        /// <param name="type"></param>
        /// <param name="firstBlock"></param>
        /// <param name="logic"></param>
        /// <param name="secondBlock"></param>
        /// <returns></returns>
        private Block Translation(
            ref Block rootBlock,
            RephraserType type,
            Block firstBlock,
            PropositionalLogic logic = null,
            Block secondBlock = null)
        {
            var result = type
                .AddLeftBlock(firstBlock)
                .AddLogic(logic)
                .AddRightBlock(secondBlock)
                .Translate();

            // memory of rootBlock is altered after this translation call
            // so this assignment is a must
            if (rootBlock.Equals(firstBlock)) rootBlock = result;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="rootBlock"></param>
        /// <param name="currentBlock"></param>
        private void TranslateToCNF(
            PropositionalLogic logic,
            ref Block rootBlock,
            ref Block currentBlock)
        {
            RephraserType type = null;
            if (logic.IsBiconditional) type = new BiconditionalElimination();
            else if (logic.IsImplication) type = new ModusPonens();
            else if (logic.IsDisjunction) type = new Distribution();

            if (type == null) throw new System.Exception("Obligatory exception");
            System.Console.WriteLine("{0}, {1}", currentBlock.GetContent(true).ToString(), currentBlock.GetHashCode());

            var unAffectedBlock = currentBlock.NextBlock.NextBlock;
            var newList = Translation(
                ref rootBlock,
                type,
                currentBlock.PreviousBlock,
                logic,
                currentBlock.NextBlock);

            currentBlock = unAffectedBlock.Equals(currentBlock) ?
                newList :
                unAffectedBlock.ExtendFront(newList);

            System.Console.WriteLine("{0}, {1}", currentBlock.GetContent(true).ToString(), currentBlock.GetHashCode());
        }

        /// <summary>
        /// Main method to rephrase the whole clause to CNF
        /// </summary>
        private Block ToCNF(Block list, int dummy)
        {
            Block rootBlock = list,
                currentBlock = null;

            while (currentBlock != rootBlock)
            {
                if (currentBlock == null) currentBlock = rootBlock;
                switch (currentBlock.ContentType)
                {
                    case ContentType.Nested:
                        var nested = currentBlock.GetContent() as Block;
                        if (currentBlock.IsNegated) Translation(ref rootBlock, new DeMorgan(), nested);
                        currentBlock.SetContent(ToCNF(nested, 0));
                        break;
                    case ContentType.Logic:
                        var connective = currentBlock.GetContent() as PropositionalLogic;
                        if (connective.IsBiconditional || connective.IsImplication)
                        {
                            TranslateToCNF(connective, ref rootBlock, ref currentBlock);
                            break;
                        }
                        if (connective.IsDisjunction)
                        {
                            Block firstBlock = currentBlock.PreviousBlock,
                                secondBlock = currentBlock.NextBlock;

                            // de morgan out of nested blocks (if they are negated)
                            if (firstBlock.IsNegated) Translation(ref rootBlock, new DeMorgan(), firstBlock);
                            if (secondBlock.IsNegated) Translation(ref rootBlock, new DeMorgan(), secondBlock);

                            // distribution does not work on a&b but a&(b||c) instead
                            if (firstBlock.ContentType == ContentType.Nested
                            || secondBlock.ContentType == ContentType.Nested)
                                TranslateToCNF(connective, ref rootBlock, ref currentBlock);

                            break;
                        }
                        break;
                }

                // forward iteration
                currentBlock = currentBlock.NextBlock;
            }

            return rootBlock;
        }
        #endregion

        #region To CNF using Truth table

        private Block GenerateCNFClause(Model model)
        {
            Block rootBlock = null;
            foreach(var truthBlock in model.Row)
            {
                if (truthBlock.Content.Length == 0) break;
                if (rootBlock == null)
                {
                    rootBlock = new Block(truthBlock.Content, truthBlock.IsTrue);
                    continue;
                }
                rootBlock.InsertBack(new Block(PropositionalLogic.Disjunction))
                    .InsertBack(new Block(truthBlock.Content, truthBlock.IsTrue));
            }
            return rootBlock;
        }

        /// <summary>
        /// Easy to implement and no errors but super slow
        /// </summary>
        /// <see cref="https://www.youtube.com/watch?v=tpdDlsg4Cws"/>
        /// <see cref="https://math.stackexchange.com/questions/3549712/how-to-compute-cnf-from-truth-table"/>
        /// <param name="rootBlock"></param>
        /// <returns></returns>
        private void ToCNF(Block rootBlock)
        {
            var result = new Engine.TruthTable(new UniqueSymbols(rootBlock)).CheckClause(rootBlock);
            ulong count = 0;
            while (true)
            {
                if (!result.ContainsKey(count)) break;
                var model = result[count++];

                // we only care about any models that generate a false value
                if (!model.GetTruthBlock("")) Conjunctions.Add(GenerateCNFClause(model));
            }
        }

        #endregion

        public override string ToString() => RootBlock.ToString();
    }
}
