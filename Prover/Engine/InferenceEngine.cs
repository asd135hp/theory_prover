using System;
using Prover.Representation;
using Prover.Representation.Parser;

namespace Prover.Engine
{
    public abstract class InferenceEngine
    {
        public InferenceEngine() { }
        public InferenceEngine(UniqueSymbols symbols) { Symbols = symbols; }

        /// <summary>
        /// All possible symbols in given KB (no logic)
        /// </summary>
        protected UniqueSymbols Symbols;

        /// <summary>
        /// General storage of KB
        /// </summary>
        protected KnowledgeBase KB;

        /// <summary>
        /// A separate method to answer if what the engine is provided with (ask) has any proofs
        /// </summary>
        /// <param name="ask"></param>
        /// <returns></returns>
        protected abstract bool KBEntails(Block ask);

        /// <summary>
        /// Prove that what it is asked for is inferenced through the knowledge base
        /// </summary>
        /// <param name="ask"></param>
        /// <returns></returns>
        public virtual string Prove(KnowledgeBase kb, string ask)
        {
            KB = kb;
            return "NO";
        }

        /// <summary>
        /// Basic of verification for any <symbol><logic><symbol> like a&b or a<=>b
        /// </summary>
        /// <param name="first">First symbol</param>
        /// <param name="logic">Connective</param>
        /// <param name="second">Second symbol</param>
        /// <returns>Result of the simple logic term</returns>
        protected bool Verify(bool first, string logic, bool second)
        {
            return logic switch
            {
                PropositionalLogic.CONJUNCTION => first && second,                              // P & Q
                PropositionalLogic.DISJUNCTION => first || second,                              // P || Q
                PropositionalLogic.IMPLICATION => !first || second,                             // !P || Q
                PropositionalLogic.BICONDITIONAL => (!first || second) && (first || !second),   // (P => Q) & (Q => P)
                _ => throw new ArgumentException($"Unsupported propositional logic: \"{logic}\""),
            };
        }
    }
}
