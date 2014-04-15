using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator
{
    public enum ComparisionOperator
    {
        LessThan,
        GreaterThan,
        LessThanOrEqualTo,
        GreaterThanOrEqualTo,
        EqualTo,
        NotEqualTo
    }

    public static class ComparisonHelper
    {
        // Only equal is supported for strings.
        public static bool Compare(this ComparisionOperator oper, string leftSide, string rightSide)
        {
            switch (oper)
            {
                case ComparisionOperator.EqualTo:
                    return leftSide == rightSide;
                case ComparisionOperator.NotEqualTo:
                    return leftSide != rightSide;
                default:
                    return leftSide == rightSide;
            }
        }

        public static bool Compare<T>(this ComparisionOperator oper, T leftSide, T rightSide) where T : IComparable<T>
        {
            switch (oper)
            {
                case ComparisionOperator.LessThan:
                    return leftSide.CompareTo(rightSide) < 0;
                case ComparisionOperator.GreaterThan:
                    return leftSide.CompareTo(rightSide) > 0;
                case ComparisionOperator.LessThanOrEqualTo:
                    return leftSide.CompareTo(rightSide) <= 0;
                case ComparisionOperator.GreaterThanOrEqualTo:
                    return leftSide.CompareTo(rightSide) >= 0;
                case ComparisionOperator.EqualTo:
                    return leftSide.CompareTo(rightSide) == 0;
                case ComparisionOperator.NotEqualTo:
                    return leftSide.CompareTo(rightSide) != 0;
                default:
                    throw new ArgumentOutOfRangeException("oper");
            }
        }
    }

}