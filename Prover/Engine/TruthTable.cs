using System;
using Prover.Representation.TruthTable;
using Prover.Representation;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace Prover.Engine
{
    public class TruthTable : InferenceEngine
    {
        public TruthTable(UniqueSymbols symbols) : base(symbols)
        {
            TruthCount = 0;
        }

        private ulong TruthCount;

        /// <summary>
        /// Check clause (a&b) against one model (row) in truth table
        /// </summary>
        /// <param name="rootBlock"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool CheckClauseAgainstModel(Block rootBlock, Model model)
        {
            bool result = false;
            string logic = "";
            bool firstValue = true;

            BlockIterator.ForEach(rootBlock, (block) =>
            {
                switch (block.ContentType)
                {
                    case ContentType.Normal:
                        bool blockValue = model.GetTruthBlock(block.GetContent(true).ToString());

                        if (block.IsNegated) blockValue = !blockValue;

                        // if else
                        if (firstValue)
                        {
                            result = blockValue;
                            firstValue = false;
                            return;
                        }
                        result = Verify(result, logic, blockValue);

                        break;
                    case ContentType.Logic:
                        logic = block.GetContent(true).ToString();
                        break;
                    case ContentType.Nested:
                        bool nestedBlockValue = CheckClauseAgainstModel(block.GetContent() as Block, model);

                        if (block.IsNegated) nestedBlockValue = !nestedBlockValue;

                        // if else
                        if (firstValue)
                        {
                            result = nestedBlockValue;
                            firstValue = false;
                            return;
                        }
                        result = Verify(result, logic, nestedBlockValue);

                        break;
                }
            });

            return result;
        }
        
        public override string Prove(KnowledgeBase kb, string ask)
        {
            if (!Symbols.UniqueValues.Contains(ask)) return "NO";
            base.Prove(kb, ask);
            return KBEntails(ClauseParser.Parse(ask)) ? $"YES: {TruthCount}" : "NO";
        }

        protected override bool KBEntails(Block ask)
        {
            IterateThroughModels(ask.GetContent(true).ToString());
            return TruthCount > 0;
        }

        private void IterateThroughModels(string ask, Model model = null, int currentTruthBlock = 0)
        {
            if (model != null
            && model.Row.Count == Symbols.UniqueValues.Count
            && model.GetTruthBlock(ask))
            {
                bool result = false, firstValue = true;
                foreach (var knowledge in KB.Knowledges)
                {
                    bool value = CheckClauseAgainstModel(knowledge, model);

                    if (firstValue)
                    {
                        result = value;
                        firstValue = false;
                        continue;
                    }

                    // clauses are separated by ";", which is actually an AND operation
                    result = Verify(result, PropositionalLogic.CONJUNCTION, value);
                }
                if (result) ++TruthCount;
            }

            if (currentTruthBlock == Symbols.UniqueValues.Count) return;

            Model trueModel = new Model(0),
                falseModel = new Model(1);

            if (model != null)
                foreach (var truthBlock in model.Row)
                {
                    trueModel.AddTruthBlock(truthBlock.Content, truthBlock.IsTrue);
                    falseModel.AddTruthBlock(truthBlock.Content, truthBlock.IsTrue);
                }

            trueModel.AddTruthBlock(Symbols.UniqueValues[currentTruthBlock], true);
            falseModel.AddTruthBlock(Symbols.UniqueValues[currentTruthBlock], false);

            ++currentTruthBlock;
            IterateThroughModels(ask, trueModel, currentTruthBlock);
            IterateThroughModels(ask, falseModel, currentTruthBlock);
        }

        /// <summary>
        /// A last minute addition to this object. It is NOT supposed to belong here!
        /// </summary>
        /// <returns></returns>
        internal void IterateThroughModels(
            Action<Model> actionOnTrueResult,
            Action<Model> actionOnFalseResult,
            Block rootBlock,
            Model model = null,
            int currentTruthBlock = 0)
        {
            if (model != null && model.Row.Count == Symbols.UniqueValues.Count)
            {
                if (CheckClauseAgainstModel(rootBlock, model)) actionOnTrueResult?.Invoke(model);
                else actionOnFalseResult?.Invoke(model);
            }

            if (currentTruthBlock == Symbols.UniqueValues.Count) return;

            Model trueModel = new Model(0),
                falseModel = new Model(1);

            if (model != null)
                foreach (var truthBlock in model.Row)
                {
                    trueModel.AddTruthBlock(truthBlock.Content, truthBlock.IsTrue);
                    falseModel.AddTruthBlock(truthBlock.Content, truthBlock.IsTrue);
                }

            trueModel.AddTruthBlock(Symbols.UniqueValues[currentTruthBlock], true);
            falseModel.AddTruthBlock(Symbols.UniqueValues[currentTruthBlock], false);

            ++currentTruthBlock;
            IterateThroughModels(
                actionOnTrueResult, actionOnFalseResult,
                rootBlock, trueModel, currentTruthBlock);

            IterateThroughModels(
                actionOnTrueResult, actionOnFalseResult,
                rootBlock, falseModel, currentTruthBlock);
        }
    }
}
