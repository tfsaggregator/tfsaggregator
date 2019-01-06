using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Aggregator.WebHooks.Utils
{
    public static class StringExtensions
    {
        // source http://extensionmethod.net/csharp/string/withvar

        /// <summary>
        /// Replace placeholders in string using <typeparamref name="T"/> properties.
        /// </summary>
        /// <typeparam name="T">Type with properties</typeparam>
        /// <param name="str">A composite format string (equal string.Format's format)</param>
        /// <param name="arg">class or anonymouse type</param>
        /// <returns></returns>
        /// <example>"{a}, {a:000}, {b}".WithVar(new {a, b});</example>
        public static string WithVar<T>(this string str, T arg)
            where T : class
        {
            var type = typeof(T);
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!(member is FieldInfo || member is PropertyInfo))
                {
                    continue;
                }

                var pattern = @"\{" + member.Name + @"(\:.*?)?\}";
                var alreadyMatch = new HashSet<string>();
                foreach (Match m in Regex.Matches(str, pattern).OfType<Match>())
                {
                    if (alreadyMatch.Contains(m.Value))
                    {
                        continue;
                    }
                    else
                    {
                        alreadyMatch.Add(m.Value);
                    }

                    string oldValue = m.Value;
                    string newValue = null;
                    string format = "{0" + m.Groups[1].Value + "}";

                    if (member is FieldInfo)
                    {
                        newValue = format.With(((FieldInfo)member).GetValue(arg));
                    }

                    if (member is PropertyInfo)
                    {
                        newValue = format.With(((PropertyInfo)member).GetValue(arg));
                    }

                    if (newValue != null)
                    {
#pragma warning disable S1226 // Method parameters and caught exceptions should not be reassigned
                        str = str.Replace(oldValue, newValue);
#pragma warning restore S1226 // Method parameters and caught exceptions should not be reassigned
                    }
                }
            }

            return str;
        }

        public static string With(this string str, params object[] param)
        {
            return string.Format(str, param);
        }
    }
}