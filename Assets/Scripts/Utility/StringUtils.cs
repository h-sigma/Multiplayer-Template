using System;
using UnityEngine.Assertions;

namespace Utility
{
    public static class StringUtils
    {
        public static int InsertAfterAll(ref string insertInto, string after, string insertion)
        {
            Assert.IsFalse(insertion.Contains(after), "insertion.Contains(after)");

            int counts    = 0;
            int prevIndex = -1;
            int indexOf   = insertInto.IndexOf(after, prevIndex + 1, StringComparison.Ordinal);
            while (indexOf != -1)
            {
                insertInto = insertInto.Insert(indexOf + 1, insertion);

                counts++;
                prevIndex = indexOf;
                indexOf   = insertInto.IndexOf(after, prevIndex + 1, StringComparison.Ordinal);
            }

            return counts;
        }

        public static int InsertBeforeAll(ref string insertInto, string after, string insertion)
        {
            Assert.IsFalse(insertion.Contains(after), "insertion.Contains(after)");

            int counts    = 0;
            int prevIndex = -1;
            int indexOf   = insertInto.IndexOf(after, prevIndex + 1, StringComparison.Ordinal);
            while (indexOf != -1)
            {
                insertInto = insertInto.Insert(indexOf, insertion);

                counts++;
                prevIndex = indexOf + insertion.Length;
                indexOf   = insertInto.IndexOf(after, prevIndex + 1, StringComparison.Ordinal);
            }

            return counts;
        }

        public static string JsonMultilineFormat(string json)
        {
            var result = json;
            var leftb  = StringUtils.InsertAfterAll(ref result, "{", "\n");
            var comma  = StringUtils.InsertAfterAll(ref result, ",", "\n");
            var rightb = StringUtils.InsertBeforeAll(ref result, "}", "\n");
            return result;
        }
    }
}