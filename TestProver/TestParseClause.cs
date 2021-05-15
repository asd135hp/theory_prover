using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TestAssignment2
{
    public class TestParseClauses
    {
        private static readonly string Connectives = @"(\~?[\w\d\-]+)(\&|\<\=\>|(?<!\<)\=\>|\|\|)?";

        public List<string> Parse(string clause)
        {
            var list = new List<string>();
            foreach(Match content in Regex.Matches(clause, Connectives))
            {
                var groups = content.Groups;
                for(int i = 1; i < groups.Count; i++)
                {
                    string val = groups[i].Value;
                    if (val.Length != 0) list.Add(val);
                }
            }
            return list;
        }

        [Test]
        public void TestParseClause()
        {
            var list = Parse("~a&b||c<=>d");
            foreach (var str in list) System.Console.WriteLine(str);
            Assert.AreEqual(list, new List<string> { "~a", "&", "b", "||", "c", "<=>", "d" });
        }
    }
}