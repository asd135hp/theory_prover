using System;
using System.IO;
using System.Text.RegularExpressions;

namespace IEngine.IO
{
    public static class InputReader
    {
        public static Output GetOutput(string method, string inputFileName)
        {
            string fileContent = File.ReadAllText(inputFileName).Trim();
            if (Regex.Matches(fileContent, @"(?:[tT][eE][lL]{2}|[aA][sS][kK])+").Count != 2)
                throw new NotSupportedException(
                    "File content's format is not supported! Here is an example:\n" +
                    "TELL\na&b;b&c;c||d;\nASK\nd");

            return new Output(method, fileContent);
        }
    }
}
