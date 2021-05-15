namespace Prover.Representation.TruthTable
{
    internal class TruthBlock
    {
        public TruthBlock(string content, bool isTrue)
        {
            Content = content;
            IsTrue = isTrue;
        }

        public string Content { get; private set; }
        public bool IsTrue { get; private set; }
    }
}
