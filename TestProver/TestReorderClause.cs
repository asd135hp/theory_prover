using NUnit.Framework;
using Prover.Representation.Parser;
using Prover.Representation.Parser.PropositionalClause;

namespace TestProver
{
    class TestReorderClause
    {
        string[] Start = new string[]
        {
            "c||(a&b)",
            "b||c||a||g",
            "(dg||j)&(ahf||ahd)"
        }, Expected = new string[]
        {
            "c||(a&b)",
            "g||(a||(b||c))",
            "(j||dg)&(ahd||ahf)"
        };

        private static bool StringComparer(string s1, string s2)
        {
            int len1 = s1.Length, len2 = s2.Length;
            if (len1 < len2) return true;
            if (len1 > len2) return false;

            for (int i = 0; i < len1; i++)
                if (s1[i].CompareTo(s2[i]) < 0) return true;
            return false;
        }

        private static Block ReorderClause(Block clause)
        {
            var blockList = new BlockIterator(clause).GetIterator();
            for (int i = 0, len = blockList.Count; i < len; ++i)
            {
                if (blockList[i].ContentType == ContentType.Logic) continue;

                Block minBlock = null;
                string min = "";
                int cache = -1;
                for (int j = i; j < len; ++j)
                {
                    var currentBlock = blockList[j];
                    if (currentBlock.ContentType == ContentType.Nested)
                        blockList[j].SetContent(ReorderClause(currentBlock.GetContent() as Block));

                    if (currentBlock.ContentType != ContentType.Logic)
                    {
                        string symbol = currentBlock.GetContent(true).ToString();
                        if (minBlock == null || StringComparer(symbol, min))
                        {
                            min = symbol;
                            minBlock = currentBlock;
                            cache = j;
                        }
                    }
                }

                if (cache != -1 && cache != i)
                {
                    // swapping order
                    blockList[cache] = blockList[i];
                    blockList[i] = minBlock;
                    blockList[i].Swap(blockList[cache]);
                }
            }
            return blockList[0];
        }

        [Test]
        public void TestReorder()
        {
            for(int i = 0; i < Start.Length; ++i)
            {
                Assert.AreEqual(Expected[i], ReorderClause(ClauseParser.Parse(Start[i])).ToString());
            }
        }
    }
}
