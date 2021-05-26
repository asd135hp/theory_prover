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
            ulong modelCount = MaxSize = (ulong)Math.Pow(2, uniqueSymbols.Count);

            // populate truth table's models
            Models = new Dictionary<ulong, Model>();
            for(int i = 0; i < uniqueSymbols.Count; i++)
            {
                modelCount /= 2;
                bool value = false;
                for(ulong j = 0; j < MaxSize; j++)
                {
                    if (i == 0) Models[j] = new Model(j);

                    if (j % modelCount == 0) value = !value; 
                    Models[j].AddTruthBlock(uniqueSymbols[i], value);
                }
            }

            TruthCount = 0;
        }

        private ulong TruthCount;
        private readonly ulong MaxSize;
        private readonly Dictionary<ulong, Model> Models;

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
        /// </summary>
        /// <param name="rootBlock"></param>
        /// <returns></returns>
        internal Dictionary<ulong, Model> CheckClause(Block rootBlock)
        {
            for(ulong i = 0; i < MaxSize; ++i)
            {
                var model = Models[i];
                model.AddTruthBlock("", CheckClauseAgainstModel(rootBlock, model));
            }
            return Models;
        }

        public override string Prove(KnowledgeBase kb, string ask)
        {
            if (!Symbols.UniqueValues.Contains(ask)) return "NO";
            base.Prove(kb, ask);
            return KBEntails(ClauseParser.Parse(ask)) ? $"YES: {TruthCount}" : "NO";
        }

        protected override bool KBEntails(Block ask)
        {
            bool result = false, concatKB = false, firstValue, current;
            string kbName = "";

            for(ulong i = 0; i < MaxSize; ++i)
            {
                var model = Models[i];
                current = false;
                firstValue = true;

                foreach(var knowledge in KB.Knowledges)
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
                    if(!result) result = true;
                    ++TruthCount;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Debugging method
        /// </summary>
        /// <returns>Representing number of truth blocks generated in each model</returns>
        public override string ToString()
        {
            string result = $"There are {MaxSize} models in total: [\n";
            for(ulong i = 0; i < MaxSize; ++i)
            {
                var model = Models[i];
                result += $"{model.Row.Count}:";
                foreach(var block in model.Row) result += (block.IsTrue ? 'T' : 'F');
                result += ",\n";
            }
            return $"{result.TrimEnd(',', '\n').Trim()}]";
        }
    }
}
