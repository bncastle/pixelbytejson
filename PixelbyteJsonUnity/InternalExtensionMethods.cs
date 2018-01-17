using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelbyte.JsonUnity
{
    internal static class InternalExtensionMethods
    {
        internal static bool Contains(this char[] array, char c)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == c) return true;
            return false;
        }

        internal static bool Contains(this char[] array, int c) { if (c < 0) return false; return Contains(array, (char)c); }

        internal static int CountChar(this string text, char c)
        {
            int count = 0;
            int index = 0;
            while (index < text.Length)
                if (text[index++] == c) count++;
            return count;
        }
    }
}
