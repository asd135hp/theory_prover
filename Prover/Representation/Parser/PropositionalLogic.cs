using System;

namespace Prover.Representation.Parser
{
    public class PropositionalLogic
    {
        // logic related strings
        public const string
            NEGATION = "~",
            CONJUNCTION = "&",
            DISJUNCTION = "||",
            IMPLICATION = "=>",
            BICONDITIONAL = "<=>",
            REGEX_NO_NEG = @"\&|\|\||\<?\=\>",
            REGEX_MATCH_EXCEPT_LOGIC = @"[^\&\|\<\=\>]", // a little dangerous because there are null characters
            ESCAPED_NEG = @"\~";

        public PropositionalLogic(string symbol)
        {
            // don't know if negation should be excluded or not
            if (symbol.Length != 0
            &&  symbol != NEGATION
            &&  symbol != CONJUNCTION
            &&  symbol != DISJUNCTION
            &&  symbol != IMPLICATION
            &&  symbol != BICONDITIONAL)
                throw new ArgumentException($"Unsupported preposition logic (connective): {symbol}");

            _content = symbol;
        }

        public static PropositionalLogic Conjunction => new PropositionalLogic(CONJUNCTION);
        public static PropositionalLogic Disjunction => new PropositionalLogic(DISJUNCTION);
        public static PropositionalLogic Implication => new PropositionalLogic(IMPLICATION);
        public static PropositionalLogic Biconditional => new PropositionalLogic(BICONDITIONAL);

        private string _content;

        public bool IsConjunction => _content == CONJUNCTION;
        public bool IsDisjunction => _content == DISJUNCTION;
        public bool IsImplication => _content == IMPLICATION;
        public bool IsBiconditional => _content == BICONDITIONAL;

        /// <summary>
        /// Only invert '&' to '||' and vice-versa
        /// </summary>
        public string Invert()
        {
            if (IsConjunction) return _content = DISJUNCTION;
            if (IsDisjunction) return _content = CONJUNCTION;

            throw new InvalidOperationException($"Could not invert this connective logic: {_content}");
        }

        public override string ToString() => _content;
    }
}
