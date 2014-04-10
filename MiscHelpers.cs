using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFSAggregator
{
    public static class MiscHelpers
    {
        static string runSeparator = new string('-', 80);

        public static void AddIfUnique(this List<WorkItem> workItemList, WorkItem workItem)
        {
            if (workItemList.Where(x=>x.Id == workItem.Id).Count() == 0)
                workItemList.Add(workItem);
        }

        public static T LoadAttribute<T>(this XElement xmlElement, string attributeName, T defaultValue) where T : IConvertible
        {
            try
            {
                return (T)Convert.ChangeType(xmlElement.Attribute(attributeName).Value, typeof(T));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Used to load an attribute from xml.  If nothing is there then the default value is returned.
        /// </summary>
        public static TEnumType LoadEnumAttribute<TEnumType>(this XElement xmlElement, string attributeName, TEnumType defaultValue) where TEnumType : struct
        {
            try
            {
                TEnumType outEnum;
                // try and see if we have a valid value
                bool success = Enum.TryParse(xmlElement.Attribute(attributeName).Value, true, out outEnum);

                // If we do then use it.  If not then return the default
                return success ? outEnum : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Evaluate all conditions.  All must be true to perform the aggregation.
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="workItem"></param>
        /// <returns></returns>
        public static bool AreAllConditionsMet(this List<Condition> conditions, WorkItem workItem)
        {
            bool areAllTrue = true;
            foreach (Condition condition in conditions)
            {
                if (!condition.Compare(workItem))
                {
                    areAllTrue = false;
                    break;
                }
            }
            return areAllTrue;
        }

        /// <summary>
        /// Evaluate all conditions.  All must be true to perform the aggregation.
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="workItem"></param>
        /// <returns></returns>
        public static bool AreAllConditionsMet(this List<Condition> conditions, WorkItem sourceItem, WorkItem parentItem)
        {
            bool areAllTrue = true;
            foreach (Condition condition in conditions)
            {
                if (!condition.Compare(sourceItem, parentItem))
                {
                    areAllTrue = false;
                    break;
                }
            }
            return areAllTrue;
        }

        public static void LogMessage(string message, bool isError)
        {
            if (TFSAggregatorSettings.LoggingIsEnabled)
            {
                string[] lines = message.Split('\n');
                foreach(string line in lines)
                    Trace.WriteLine(String.Format("TFSAggregator: {0}", line));

            }

            if (isError)
                EventLog.WriteEntry(TFSAggregatorSettings.EventLogSource, String.Format("TFSAggregator: {0}", message), EventLogEntryType.Error);
        }


        public static void LogMessage(string message)
        {
            LogMessage(message, false);
        }

        public static void AddRunSeparatorToLog()
        {

            LogMessage(runSeparator);
        }

        public enum LoggingLevel
        {
            None,
            Diagnostic
        }

        public static StringBuilder GetInvalidWorkItemFieldsList(WorkItem wi)
        {
            StringBuilder sb = new StringBuilder();
            if (wi.IsValid())
            {
                sb.AppendLine("None");
            }
            else
            {
                foreach (string s in wi.Validate())
                    sb.AppendLine(s);
            }

            return sb;
        }
    }
}
