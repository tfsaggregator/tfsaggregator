using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator
{
    public class Condition
    {
        public string LeftFieldName { get; set; }
        public string LeftValue { get; set; }
        public ComparisionOperator CompOperator { get; set; }
        public string RightValue { get; set; }
        public string RightFieldName { get; set; }

        /// <summary>
        /// Perform a comparison.  If anything goes wrong then we just assume that everything checks out
        /// This is because if a value is null, we don't want to fail it.
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns></returns>
        public bool Compare(WorkItem workItem)
        {
            object leftSideValue;
            object rightSideValue;
            leftSideValue = string.IsNullOrEmpty(LeftFieldName) ? LeftValue : workItem[LeftFieldName];
            rightSideValue = string.IsNullOrEmpty(RightFieldName) ? RightValue : workItem[RightFieldName];
            try
            {
                // Null is a bit of a special case.
                if (rightSideValue as String == "$NULL$" && leftSideValue == null)
                    return true;
                if (leftSideValue as string == "$NULL$" && rightSideValue == null)
                    return true;

                // Get the type of the work item.
                Type leftType = leftSideValue.GetType();

                // Compare each type.
                if (leftType == typeof(string))
                {
                    return CompOperator.Compare((string)leftSideValue, (string)rightSideValue);
                }
                if (leftType == typeof(int))
                {
                    return CompOperator.Compare((int)leftSideValue, (int)rightSideValue);
                }
                if (leftType == typeof(double))
                {
                    //double leftValue, rightValue;
                    //GetValues(workItem, out leftValue, out rightValue);
                    return CompOperator.Compare((double)leftSideValue, (double)rightSideValue);
                }
                if (leftType == typeof(DateTime))
                {
                    //DateTime leftValue, rightValue;
                    //GetValues(workItem, out leftValue, out rightValue);
                    return CompOperator.Compare((DateTime)leftSideValue, (DateTime)rightSideValue);
                }

                // No other types are supported
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private void GetValues<T>(WorkItem workItem, out T leftValue, out T rightValue) where T : IConvertible
        {
            leftValue = (T)workItem[LeftFieldName];
            if (RightValue != null)
                rightValue = ExpandMacro<T>(RightValue);
            else
                rightValue = (T)workItem[RightFieldName];
        }

        private T ExpandMacro<T>(String macro) where T : IConvertible
        {
            if (macro == "$NOW$")
                return (T)Convert.ChangeType(DateTime.Now, typeof(T));

            return (T)Convert.ChangeType(macro, typeof(T));
        }
    }
}