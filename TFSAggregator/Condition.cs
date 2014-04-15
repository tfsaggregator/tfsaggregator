using System;
using TFSAggregator.TfsFacade;
using TFSAggregator.TfSFacade;

namespace TFSAggregator
{
    public class Condition
    {
        public string LeftFieldName { get; set; }
        public string LeftValue { get; set; }
        public ComparisionOperator CompOperator { get; set; }
        public string RightValue { get; set; }
        public string RightFieldName { get; set; }

        private object getFieldValue(string fieldName, IWorkItem sourceItem, IWorkItem parentItem = null)
        {
            object value;
            if (fieldName.StartsWith("Source."))
            {
                value = workItemFieldValue(fieldName.Split('.')[1], sourceItem);
            }
            else if (fieldName.StartsWith("Parent."))
            {
                value = workItemFieldValue(fieldName.Split('.')[1], parentItem);
            }
            else value = workItemFieldValue(fieldName, sourceItem);
            return value;
        }

        private static object workItemFieldValue(string fieldName, IWorkItem item)
        {
            object value;
            if (fieldName.EndsWith("WorkItemType", StringComparison.InvariantCultureIgnoreCase))
                value = item.TypeName;
            else
                value = item == null ? null : item[fieldName];
            return value;
        }

        /// <summary>
        /// Perform a comparison.  If anything goes wrong then we just assume that everything checks out
        /// This is because if a value is null, we don't want to fail it.
        /// </summary>
        /// <param name="workItem"></param>
        /// <returns></returns>
        public bool Compare(IWorkItem sourceItem, IWorkItem parentItem=null)
        {
            object leftSideValue;
            object rightSideValue;
            leftSideValue = string.IsNullOrEmpty(LeftFieldName) ? LeftValue : getFieldValue(LeftFieldName, sourceItem, parentItem);
            rightSideValue = string.IsNullOrEmpty(RightFieldName) ? RightValue : getFieldValue(RightFieldName,sourceItem, parentItem);
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

        private void GetValues<T>(IWorkItem workItem, out T leftValue, out T rightValue) where T : IConvertible
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