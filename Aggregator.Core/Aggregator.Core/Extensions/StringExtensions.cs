using System;

namespace Aggregator.Core.Extensions
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
            return lhs == "*" || rhs == "*" || string.Equals(lhs, rhs, StringComparison.OrdinalIgnoreCase);
        }

        public static string Truncate(this string value, int maxLength)
        {
            return string.IsNullOrEmpty(value) ? value : value.Substring(0, Math.Min(value.Length, maxLength));
        }
    }
}
