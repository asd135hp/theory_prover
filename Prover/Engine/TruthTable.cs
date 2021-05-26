using System;
using System.Collections.Generic;
using Prover.Representation.TruthTable;
using Prover.Representation;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace Prover.Engine
{
    public class TruthTable : InferenceEngine
    {
        public TruthTable(UniqueSymbols symbols): base(symbols)
        {
            var uniqueSymbols = symbols.UniqueValues;
            int symbolsCount = uniqueSymbols.Count;
            Models = new Dictionary<ulong, Dictionary<ulong, Model>>();

            if (symbolsCount <= 64)
            {
                ulong modelCount = MaxSize = (ulong)Math.Pow(2, symbolsCount);
                Models[0] = new Dictionary<ulong, Model>();
                MaxPartitions = 1;

                // populate truth table's models
                for (int i = 0; i < symbolsCount; i++)
                {
                    modelCount /= 2;
                    bool value = false;
                    for (ulong j = 0; j < MaxSize; j++)
                    {
                        if (i == 0) Models[0][j] = new Model(j);

                        if (j % modelCount == 0) value = !value;
                        Models[0][j].AddTruthBlock(uniqueSymbols[i], value);
                    }
                }
            }
            else if (symbolsCount <= 128)
            {
                // the truth table is too big to verify its validity!
                // initialize partition size
                MaxPartitions = (ulong)Math.Pow(2, symbolsCount - 64);
                for (ulong k = 0; k < MaxPartitions; ++k) Models[k] = new Dictionary<ulong, Model>();

                // initialize models
                double modelCount = MaxPartitions;
                for (int i = 0; i < symbolsCount; ++i)
                {
                    modelCount /= 2.0;
                    bool value = false;
                    if (modelCount >= 1.0)
                    {
                        for (double j = 0; j < MaxPartitions; ++j)
                        {
                            var model = Models[(ulong)j];
                            for (ulong k = 0; k < ulong.MaxValue; ++k)
                            {
                                if (i == 0) model[k] = new Model(k);

                                if (j % modelCount == 0) value = !value;
                                model[k].AddTruthBlock(uniqueSymbols[i], value);
                            }
                        }
                        continue;
                    }

                    // for modelCount = 1.0 or now it is 0.5
                    ulong switchingValue = (ulong)(ulong.MaxValue * modelCount);
                    for (double j = 0; j < MaxPartitions; ++j)
                    {
                        var model = Models[(ulong)j];
                        for (ulong k = 0; k < ulong.MaxValue; ++k)
                        {
                            if (i == 0) model[k] = new Model(k);

                            if (k % switchingValue == 0) value = !value;
                            model[k].AddTruthBlock(uniqueSymbols[i], value);
                        }
                    }
                }
            }

            TruthCount = 0;
        }

        private ulong TruthCount;
        private readonly ulong MaxPartitions;
        private readonly ulong MaxSize;
        private readonly Dictionary<ulong, Dictionary<ulong, Model>> Models;

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

        /// <summary>
        /// A last minute addition to this object. It is NOT supposed to belong here!
        /// However, here is a real question:
        /// what is the odd of an user inputting 64 unique symbols into one single clause?
        /// </summary>
        /// <param name="rootBlock"></param>
        /// <returns></returns>
        internal Dictionary<ulong, Model> CheckClause(Block rootBlock)
        {
            if (Symbols.UniqueValues.Count > 64)
                throw new NotSupportedException("Unfortunately, you have hit the odd of " +
                    "inputting more than 64 unique symbols into one clause.\n" +
                    "Please consider decrease the amount of unique symbols " +
                    "that any of your clauses have to a maximum of 64 unique symbols!");

            var modelFor64UniqueSymbols = Models[0];
            for(ulong i = 0; i < MaxSize; ++i)
            {
                var model = modelFor64UniqueSymbols[i];
                model.AddTruthBlock("", CheckClauseAgainstModel(rootBlock, model));
            }
            return modelFor64UniqueSymbols;
        }

        public override string Prove(KnowledgeBase kb, string ask)
        {
            if (!Symbols.UniqueValues.Contains(ask)) return "NO";
            base.Prove(kb, ask);
            return KBEntails(ClauseParser.Parse(ask)) ? $"YES: {TruthCount}" : "NO";
        }

        protected override bool KBEntails(Block ask)
        {
            if(Symbols.UniqueValues.Count > 128)
                throw new NotSupportedException("Could not do a truth table with more than 128 unique symbols!");

            // start entailment
            bool result = false, concatKB = false, firstValue, current;
            string kbName = "";

            for(ulong i = 0; i < MaxPartitions; ++i)
                for (ulong j = 0; j < MaxSize; ++j)
                {
                    var model = Models[i][j];
                    current = false;
                    firstValue = true;

                    foreach (var knowledge in KB.Knowledges)
                    {
                        bool value = CheckClauseAgainstModel(knowledge, model);

                        if (!concatKB) kbName += knowledge.ToString() + PropositionalLogic.CONJUNCTION;
                        if (firstValue)
                        {
                            current = value;
                            firstValue = false;
                            continue;
                        }

                        // clauses are separated by ";", which is actually an AND operation
                        current = Verify(current, PropositionalLogic.CONJUNCTION, value);
                    }

                    concatKB = true;
                    model.AddTruthBlock(kbName, current);

                    if (current && model.GetTruthBlock(ask.GetContent(true).ToString()))
                    {
                        if (!result) result = true;
                        ++TruthCount;
                    }
                }

            return result;
        }
    }
}
