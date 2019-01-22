using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaltMalKurzControl.Helpers
{
    public static class Extensions
    {
        public static string FirstWord(this string str) => str.Contains(" ") ? str.Remove(str.IndexOf(" ")) : str;
    }
}
