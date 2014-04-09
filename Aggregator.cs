using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSAggregator.ConfigTypes;

namespace TFSAggregator
{
    public static class Aggregator
    {
        /// <summary>
        /// Used to acutally do the aggregating.
        /// </summary>
        /// <returns>true if a change was made.  False if not</returns>
        public static bool Aggregate(IEnumerable<WorkItem> sourceWorkItems, WorkItem targetWorkItem, ConfigAggregatorItem configAggregatorItem)
        {
            if (configAggregatorItem.OperationType == OperationTypeEnum.Numeric)
            {
                return NumericAggregation(sourceWorkItems, targetWorkItem, configAggregatorItem);
            }
            if (configAggregatorItem.OperationType == OperationTypeEnum.String)
            {
                return StringAggregation(sourceWorkItems, targetWorkItem, configAggregatorItem);
            }
            if (configAggregatorItem.OperationType == OperationTypeEnum.Copy)
            {
                return CopyAggregation(sourceWorkItems, targetWorkItem, configAggregatorItem);
            }

            // This should never happen
            return false;
        }

        /// <summary>
        /// Adds up all the values that need aggregating
        /// </summary>
        /// <returns>true if a change was made.  False if not</returns>
        private static bool NumericAggregation(IEnumerable<WorkItem> sourceWorkItems, WorkItem targetWorkItem, ConfigAggregatorItem configAggregatorItem)
        {
            double aggregateValue = 0;
            // Iterate through all of the work items that we are pulling data from.
            // For link typ of "Self" this will be just one item.  For "Parent" this will be all of the co-children of the work item sent in the event.
            foreach (WorkItem sourceWorkItem in sourceWorkItems)
            {
                // Iterate through all of the TFS Fields that we are aggregating.
                foreach (ConfigItemType sourceField in configAggregatorItem.SourceItems)
                {
                    double sourceValue = sourceWorkItem.GetField(sourceField.Name, 0.0);
                    aggregateValue = configAggregatorItem.Operation.Perform(aggregateValue, sourceValue);
                }
            }

            if (aggregateValue != targetWorkItem.GetField<double>(configAggregatorItem.TargetItem.Name, 0))
            {
                targetWorkItem[configAggregatorItem.TargetItem.Name] = aggregateValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks to see if all of the source fields values are in one of the mappings.  
        /// The first mapping that is found that is a match is used.  It will apply the target value from the mapping to 
        /// the Target Field.
        /// </summary>
        /// <returns>true if a change was made.  False if not</returns>
        private static bool StringAggregation(IEnumerable<WorkItem> sourceWorkItems, WorkItem targetWorkItem, ConfigAggregatorItem configAggregatorItem)
        {
            string aggregateValue = "";
            bool aggregateFound = false;

            // Iterate through the mappings untill (or if) we find one that we match on
            foreach (Mapping mapping in configAggregatorItem.Mappings)
            {
                bool mappingMatches;
                // The default value depends on the inclusivity of the mappings.
                // If it is And then we are trying to prove that all of them match so we start with true, any mismatches will cause us to set to false.
                // If it is Or then we are trying to prove that only one them match to succeed so we start with false, any matches will cause us to set to true.
                if (mapping.Inclusive)
                    mappingMatches = true;
                else
                    mappingMatches = false;

                // Iterate through all of the work items that we are pulling data from.
                // For link typ of "Self" this will be just one item.  For "Parent" this will 
                // be all of the co-children of the work item sent in the event.
                foreach (WorkItem sourceWorkItem in sourceWorkItems)
                {
                    // Iterate through all of the TFS Fields that we are aggregating.
                    foreach (ConfigItemType sourceField in configAggregatorItem.SourceItems)
                    {
                        // Get the value of the sourceField on the sourceWorkItem
                        string sourceValue = sourceWorkItem.GetField(sourceField.Name, "");
                        
                        // Check to see if the value we have is not in the list of SourceValues
                        // If it is not then this mapping is not going to be satisfied because this source item
                        // breaks it (because we are inclusively checking (ie "And")).
                        if (!mapping.SourceValues.Contains(sourceValue) &&(mapping.Inclusive))
                        {
                            // it was not in the list.  We are done with this mapping.
                            // if we get here then this is an "And" mapping that failed.
                            mappingMatches = false;
                            break;
                        }

                        // Check to see if the value we have is in the list of SourceValues
                        // If it is, this mapping is satisfied because we are non inclusive (ie "Or")
                        if (mapping.SourceValues.Contains(sourceValue) && (!mapping.Inclusive))
                        {
                            // it was in the list.  We are done with this mapping.
                            // If we get here then this was an "Or" mapping that succeded
                            mappingMatches = true;
                            break;
                        }
                    }
                    // If this is an "And" and mapping does not match then we may as well be done with this iteration of work items.
                    if ((!mappingMatches) && (mapping.Inclusive))
                        break;

                    // If this is an "Or" and mapping does match then we need to be done.
                    if ((mappingMatches) && (!mapping.Inclusive))
                        break;
                }
                // If the mapping matched then we are done looking
                if (mappingMatches)
                {
                    aggregateValue = mapping.TargetValue;
                    aggregateFound = true;
                    break;
                }
            }

            if (aggregateFound)
            {
                // see if we need to make a change:
                if (targetWorkItem[configAggregatorItem.TargetItem.Name].ToString() != aggregateValue)
                {
                    // If this is the "State" field then we may have do so special stuff 
                    // (to get the state they want from where we are).  If not then just set the value.
                    if (configAggregatorItem.TargetItem.Name != "State")
                        targetWorkItem[configAggregatorItem.TargetItem.Name] = aggregateValue;
                    else
                        targetWorkItem.TransitionToState(aggregateValue, "TFS Aggregator: ");

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Copies a value from one field into another.
        /// No mapping configuration is required.
        /// If there are multiple source items or multiple source fields the first of each type is used
        /// </summary>
        /// <returns>true if a change was made.  False if not</returns>
        private static bool CopyAggregation(IEnumerable<WorkItem> sourceWorkItems, WorkItem targetWorkItem, ConfigAggregatorItem configAggregatorItem)
        {
            string aggregateValue = "";
            bool aggregateFound = false;

                // Iterate through all of the work items that we are pulling data from.
                // For link type of "Self" this will be just one item.
                // For "Parent" this will be all of the co-children of the work item sent in the event.
                // For "Children" this will be all of the direct children of the work item in the event.
                //TODO: Make sure children link type works
                foreach (WorkItem sourceWorkItem in sourceWorkItems)
                {
                    // Iterate through all of the TFS Fields that we are aggregating.
                    foreach (ConfigItemType sourceField in configAggregatorItem.SourceItems)
                    {
                        // Get the value of the sourceField on the sourceWorkItem
                        string sourceValue = sourceWorkItem.GetField(sourceField.Name, "");

                        // Check to see if the value we have is not in the list of SourceValues
                        // If it is not then this mapping is not going to be satisfied because this source item
                        // breaks it (because we are inclusively checking (ie "And")).
                        if (!mapping.SourceValues.Contains(sourceValue) && (mapping.Inclusive))
                        {
                            // it was not in the list.  We are done with this mapping.
                            // if we get here then this is an "And" mapping that failed.
                            mappingMatches = false;
                            break;
                        }

                        // Check to see if the value we have is in the list of SourceValues
                        // If it is, this mapping is satisfied because we are non inclusive (ie "Or")
                        if (mapping.SourceValues.Contains(sourceValue) && (!mapping.Inclusive))
                        {
                            // it was in the list.  We are done with this mapping.
                            // If we get here then this was an "Or" mapping that succeded
                            mappingMatches = true;
                            break;
                        }
                    }
                    // If this is an "And" and mapping does not match then we may as well be done with this iteration of work items.
                    if ((!mappingMatches) && (mapping.Inclusive))
                        break;

                    // If this is an "Or" and mapping does match then we need to be done.
                    if ((mappingMatches) && (!mapping.Inclusive))
                        break;
                }

            aggregateValue = mapping.TargetValue;
            aggregateFound = true;

            if (aggregateFound)
            {
                // see if we need to make a change:
                if (targetWorkItem[configAggregatorItem.TargetItem.Name].ToString() != aggregateValue)
                {
                    // If this is the "State" field then we may have do so special stuff 
                    // (to get the state they want from where we are).  If not then just set the value.
                    if (configAggregatorItem.TargetItem.Name != "State")
                        targetWorkItem[configAggregatorItem.TargetItem.Name] = aggregateValue;
                    else
                        targetWorkItem.TransitionToState(aggregateValue, "TFS Aggregator: ");

                    return true;
                }
            }

            return false;
        }

    }
}
