using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAggregator
{
    public static class DoubleExtensions
    {
        public const double TfsEpsylon = 1e-15d;



        public static bool SafeEquals(this double a, double b)
        {
            return SafeEquals(a, b, TfsEpsylon);
        }

        public static bool SafeEquals(this double a, double b, double epsilon)
        {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < Double.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * Double.MinValue);
            }
            else
            { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        public static int SafeCompareTo(this double a, double b)
        {
            return SafeCompareTo(a, b, TfsEpsylon);
        }

        public static int SafeCompareTo(this double a, double b, double epsilon)
        {
            if (SafeEquals(a, b))
            {
                return 0;
            }
            else
            {
                return a.CompareTo(b);
            }
        }
    }
}
