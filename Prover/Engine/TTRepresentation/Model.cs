using System;
using System.Collections.Generic;

namespace Prover.Engine.TTRepresentation
{
    internal class Model
    {
        public Model(ulong id)
        {
            ID = id;
            Row = new List<TruthBlock>();
        }

        public void AddTruthBlock(string content, bool isTrue)
            => Row.Add(new TruthBlock(content, isTrue));

        public bool GetTruthBlock(string content)
        {
            foreach (var block in Row)
                if (content == block.Content) return block.IsTrue;

            throw new ArgumentException("Unrecognized symbol when checking against the truth table!");
        }

        public ulong ID { get; private set; }
        public List<TruthBlock> Row { get; private set; }

        /// <summary>
        /// Debugging method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = $"Model #{ID}: ";
            foreach (var block in Row) result += block.IsTrue ? 'T' : 'F';
            return result;
        }
    }
}
