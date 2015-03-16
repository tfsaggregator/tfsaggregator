using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TFSAggregator.ConfigTypes;
using TFSAggregator.TfsFacade;
using TFSAggregator.TfsSpecific;

namespace TFSAggregator
{
    public class EventProcessor
    {
        IWorkItemRepository store;

        public EventProcessor() : this(new WorkItemRepository(TFSAggregatorSettings.TFSUri))
        {
        }

        public EventProcessor(IWorkItemRepository workItemStore)
        {
            this.store = workItemStore;
        }

        /// <summary>
        /// This is the one where all the magic starts.  Main() so to speak.  I will load the settings, connect to tfs and apply the aggregation rules.
        /// </summary>
        public ProcessingResult ProcessEvent(IRequestContext requestContext, INotification notification)
        {
            var result = new ProcessingResult();
            int currentAggregationId = 0;
            int workItemId = 0;
            string currentAggregationName = string.Empty;

            try
            {
                // Get the id of the work item that was just changed by the user.
                workItemId = notification.WorkItemId;
                // Download the work item so we can update it (if needed)
                var eventWorkItem = store.GetWorkItem(workItemId);
                var workItemTypeName = eventWorkItem.TypeName;
                var workItemsToSave = new List<IWorkItem>();

                if (TFSAggregatorSettings.LoggingIsEnabled)
                {
                    MiscHelpers.LogMessage(String.Format("Change detected to {0} [{1}]", workItemTypeName, workItemId));
                    MiscHelpers.LogMessage(String.Format("{0}Processing {1} AggregationItems", "  ", TFSAggregatorSettings.ConfigAggregatorItems.Count.ToString()));
                }

                // Apply the aggregation rules to the work item
                foreach (ConfigAggregatorItem configAggregatorItem in TFSAggregatorSettings.ConfigAggregatorItems)
                {
                    List<IWorkItem> sourceWorkItems = null;
                    IWorkItem targetWorkItem = null;
                    currentAggregationName = configAggregatorItem.Name;

                    var matchableWorkItemTypes = configAggregatorItem.WorkItemType.Split(';');

                    // Check to make sure that the rule applies to the work item type we have
                    if (matchableWorkItemTypes.Contains(eventWorkItem.TypeName))
                    {
                        if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}[Entry {2}] Aggregation '{3}' applies to {1} work items", "    ", workItemTypeName, currentAggregationId, currentAggregationName));

                        // Use the link type to see what the work item we are updating is
                        if (configAggregatorItem.LinkType == ConfigLinkTypeEnum.Self)
                        {
                            // We are updating the same workitem that was sent in the event.
                            sourceWorkItems = new List<IWorkItem> { eventWorkItem };
                            targetWorkItem = eventWorkItem;

                            if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}{0}Aggregation applies to SELF. ({1} {2})", "  ", workItemTypeName, workItemId));

                            // Make sure that all conditions are true before we do the aggregation
                            // If any fail then we don't do this aggregation.
                            if (!configAggregatorItem.Conditions.AreAllConditionsMet(targetWorkItem))
                            {
                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for aggregation are not met.", "    "));
                                currentAggregationId++;
                                continue;
                            }

                            if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for aggregation are met.", "    "));

                        }
                        // We are aggregating to the parent
                        else if (configAggregatorItem.LinkType == ConfigLinkTypeEnum.Parent)
                        {
                            bool parentLevelFound = true;

                            // Go up the tree till we find the level of parent that we are looking for.
                            var parentWorkItem = eventWorkItem;
                            for (int i = 0; i < configAggregatorItem.LinkLevel; i++)
                            {
                                // Load the parent from the saved list (if we have it in there) or just load it from the store.
                                IWorkItem tempWorkItem = parentWorkItem.GetParentFromListOrStore(workItemsToSave, store);
                                // 
                                if (tempWorkItem != null)
                                    parentWorkItem = tempWorkItem;
                                else
                                    parentLevelFound = false;

                            }
                            // If we did not find the correct parent then we are done with this aggregation.
                            if (!parentLevelFound)
                            {
                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}{0}Couldn't find a PARENT {2} {4} up from {3} [{1}]. This aggregation will not continue.", "  ", workItemId, configAggregatorItem.LinkLevel, workItemTypeName, configAggregatorItem.LinkLevel > 1 ? "levels" : "level"));
                                currentAggregationId++;
                                continue;
                            }

                            if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}{0}Found {1} [{2}] {3} {4} up from {5} [{6}].  Aggregation continues.", "  ", parentWorkItem.TypeName, parentWorkItem.Id, configAggregatorItem.LinkLevel, configAggregatorItem.LinkLevel > 1 ? "levels" : "level", workItemTypeName, workItemId));

                            targetWorkItem = parentWorkItem;

                            // Make sure that all conditions are true before we do the aggregation
                            // If any fail then we don't do this aggregation.
                            if (!configAggregatorItem.Conditions.AreAllConditionsMet(eventWorkItem, targetWorkItem))
                            {
                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for parent aggregation are not met", "    "));
                                currentAggregationId++;
                                continue;
                            }

                            if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for parent aggregation are met", "    "));

                            // Get the children down how ever many link levels were specified.
                            var iterateFromParents = new List<IWorkItem> { targetWorkItem };
                            for (int i = 0; i < configAggregatorItem.LinkLevel; i++)
                            {
                                var thisLevelOfKids = new List<IWorkItem>();
                                // Iterate all the parents to find the children of current set of parents
                                foreach (var iterateFromParent in iterateFromParents)
                                {
                                    thisLevelOfKids.AddRange(iterateFromParent.GetChildrenFromListOrStore(workItemsToSave, store));
                                }

                                iterateFromParents = thisLevelOfKids;
                            }

                            // remove the kids that are not the right type that we are working with
                            iterateFromParents.RemoveAll(x => !matchableWorkItemTypes.Contains(x.TypeName));
                            sourceWorkItems = iterateFromParents;
                        }
                        // We are aggregating to the children
                        else if (configAggregatorItem.LinkType == ConfigLinkTypeEnum.Children)
                        {
                            var parentWorkItem = eventWorkItem;

                            var targetWorkItemTypes = configAggregatorItem.TargetWorkItemType.Split(';');

                            // Get the children down how ever many link levels were specified.
                            // Start at the parent level and then iterate down the work item tree
                            var currentLevelWorkItems = new List<IWorkItem> { parentWorkItem };
                            for (int i = 0; i < configAggregatorItem.LinkLevel; i++)
                            {
                                var thisLevelOfKids = new List<IWorkItem>();
                                // Iterate all the parents to find the children of current set of parents
                                foreach (var iterateFromParent in currentLevelWorkItems)
                                {
                                    thisLevelOfKids.AddRange(iterateFromParent.GetChildrenFromListOrStore(workItemsToSave, store));
                                }

                                currentLevelWorkItems = thisLevelOfKids;
                            }

                            // remove the kids that are not of the type we want to change.
                            currentLevelWorkItems.RemoveAll(x => !targetWorkItemTypes.Contains(x.TypeName));
                            sourceWorkItems = currentLevelWorkItems;

                            // Make sure that all conditions are true before we do the aggregation
                            // In a copyTo operation, the conditions are evaluated per child workitem.
                            // If any fail then we don't do this aggregation.
                            foreach (var childItem in sourceWorkItems)
                            {
                                if (!configAggregatorItem.Conditions.AreAllConditionsMet(childItem, parentWorkItem))
                                {
                                    if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for parent aggregation are not met", "    "));
                                    continue;
                                }
                                if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}{0}All conditions for parent aggregation are met", "    "));
                                var changedWorkItem = Aggregator.Aggregate(eventWorkItem, null, childItem, configAggregatorItem);
                                // If we made a change then add this work item to the list of items to save.
                                if (changedWorkItem != null)
                                {
                                    // Add the changed work item to the list of work items to save.
                                    workItemsToSave.AddIfUnique(changedWorkItem);
                                }
                            }
                        }

                        // Do the actual aggregation now
                        if (configAggregatorItem.OperationType != OperationTypeEnum.CopyTo)
                        {
                            var changedWorkItem = Aggregator.Aggregate(eventWorkItem, sourceWorkItems, targetWorkItem, configAggregatorItem);
                            // If we made a change then add this work item to the list of items to save.
                            if (changedWorkItem != null)
                            {
                                // Add the changed work item to the list of work items to save.
                                workItemsToSave.AddIfUnique(changedWorkItem);
                            }
                        }
                    }
                    else
                    {
                        if (TFSAggregatorSettings.LoggingIsEnabled) MiscHelpers.LogMessage(String.Format("{0}[Entry {2}] Aggregation '{3}' does not apply to {1} work items", "    ", workItemTypeName, currentAggregationId, currentAggregationName));
                    }

                    currentAggregationId++;
                }

                // Save any changes to the target work items.
                workItemsToSave.ForEach(x =>
                {
                    bool isValid = x.IsValid();

                    if (TFSAggregatorSettings.LoggingIsEnabled)
                    {
                        MiscHelpers.LogMessage(String.Format("{0}{0}{0}{1} [{2}] {3} valid to save. {4}",
                                                            "    ",
                                                            x.TypeName,
                                                            x.Id,
                                                            isValid ? "IS" : "IS NOT",
                                                            String.Format("\n{0}{0}{0}{0}Invalid fields: {1}", "    ", MiscHelpers.GetInvalidWorkItemFieldsList(x).ToString())));
                    }

                    if (isValid)
                    {
                        x.PartialOpen();
                        x.Save();
                    }
                });

                MiscHelpers.AddRunSeparatorToLog();
            }
            catch (Exception e)
            {
                string message = String.Format("Exception encountered processing Work Item [{2}]: {0} \nStack Trace:{1}", e.Message, e.StackTrace, workItemId);
                if (e.InnerException != null)
                {
                    message += String.Format("\n    Inner Exception: {0} \nStack Trace:{1}", e.InnerException.Message, e.InnerException.StackTrace);
                }
                MiscHelpers.LogMessage(message, true);
            }

            return result;
        }
    }
}
