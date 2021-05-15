using System;
using Prover.Engine;
using Prover.Engine.Chaining;
using Prover.Representation;

namespace IEngine.IO
{
    public class Output
    {
        public Output(string method, string content)
        {
            ProcessContent(content);
            GetInferenceEngine(method);
        }

        private InferenceEngine _engine;
        private KnowledgeBase _kb;
        private string _ask;

        /// <summary>
        /// Raw content:
        /// TELL
        /// a&b;b&c;...
        /// ASK
        /// d
        /// </summary>
        /// <param name="content"></param>
        private void ProcessContent(string content)
        {
            string tell = "", ask = "";
            bool foundTell = false, foundAsk = false;

            foreach (string subContent in content.Split(Environment.NewLine))
            {
                if (subContent.Length == 0) continue;

                if (subContent.Length == 4 && subContent.ToLower() == "tell")
                {
                    foundTell = true;
                    continue;
                }
                if (subContent.Length == 3 && subContent.ToLower() == "ask")
                {
                    foundAsk = true;
                    continue;
                }

                if (foundTell && !foundAsk) tell += subContent;
                if (foundAsk) ask += subContent;
            }

            if(!foundTell || !foundAsk)
                throw new NotSupportedException(
                    "File content's format is not supported! Here is an example:\n" +
                    "TELL\na&b;b&c;c||d;\nASK\nd");

            _kb = new KnowledgeBase(tell);
            _ask = ask;
        }

        private void GetInferenceEngine(string method)
        {
            _engine = method.ToLower() switch
            {
                "tt" => new TruthTable(new UniqueSymbols(_kb)),
                "fc" => new ForwardChaining(),
                "bc" => new BackwardChaining(),
                "rs" => new ResolutionSolver(),
                _ => throw new NotSupportedException(
                    $"The method named \"{method}\" is not supported by this program!")
            };
        }

        public void GetResult() => Console.WriteLine(_engine.Prove(_kb, _ask));
    }
}
