using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zad4_3k
{
    internal class Count
    {
        /// <summary>
        /// Любые пробелы
        /// </summary>
        public static int CountAllWhitespaces(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return text.Count(char.IsWhiteSpace);
        }
    }
}
