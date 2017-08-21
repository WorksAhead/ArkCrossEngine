using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkCrossEngine
{
    public static class CustomStringExtern
    {
        public static bool CustomEndsWith(string s, string pattarn)
        {
            int ap = s.Length - 1;
            int bp = pattarn.Length - 1;

            while (ap >= 0 && bp >= 0 && s[ap] == pattarn[bp])
            {
                ap--;
                bp--;
            }
            return bp < 0 && s.Length >= pattarn.Length;
        }

        public static bool CustomStartsWith(string s, string pattern)
        {
            int aLen = s.Length;
            int bLen = pattern.Length;
            int ap = 0; int bp = 0;

            while (ap < aLen && bp < bLen && s[ap] == pattern[bp])
            {
                ap++;
                bp++;
            }

            return bp == bLen && aLen >= bLen;
        }
    }
}
