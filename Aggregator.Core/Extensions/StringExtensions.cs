using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public static class StringExtensions
    {
        /// <summary>
        /// Case insensitive comparison plus '*' wildcard matching
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns></returns>
        public static bool SameAs(this string lhs, string rhs)
        {
            return lhs == "*" || rhs == "*" || string.Compare(lhs, rhs, true) == 0;
        }
    }
}
