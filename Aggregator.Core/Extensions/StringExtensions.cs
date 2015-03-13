using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public static class StringExtensions
    {
        public static bool SameAs(this string lhs, string rhs)
        {
            return lhs == "*" || rhs == "*" || string.Compare(lhs, rhs, true) == 0;
        }
    }
}
