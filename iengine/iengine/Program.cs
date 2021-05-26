using System;
using Prover.Representation.Parser.PropositionalClause;

namespace IEngine
{
    class Program
    {
        /// <summary>
        /// Welcome to theory prover using inference engine. How to use: <methodName> <inputFileName>
        /// <para>
        /// Where:
        ///     - methodName is any one of these four: TT(Truth Table), FC(Forward Chaining),
        ///         BC(Backward Chaining) and RS(Resolution Solver - currently unstable)
        ///     - inputFileName is the file name with the same directory as this .exe's directory, containing:
        ///         + Line 1: TELL (indicator for knowledge base)
        ///         + Line 2+: Clauses like: a&b, using only these four connectives 
        ///             &(AND), ||(OR), =>(IMPLIES), <=>(BICONDITIONAL)
        ///         + After 2+: ASK (indicator for symbols used for proving)
        ///         + Finally: A single symbol
        /// </para>
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                IO.InputReader.GetOutput(args[0], args[1]).GetResult();
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception({0}) was caught: {1}\nStack trace: {2}",
                    e.GetType().Name,
                    e.Message,
                    e.StackTrace);
            }
        }
    }
}
