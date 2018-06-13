using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Navigation
{
    public static class StateWorkFlow
    {
        // caches info on work item states
        private static readonly Dictionary<IWorkItemType, List<StateTransition>> AllTransitions = new Dictionary<IWorkItemType, List<StateTransition>>();

        /// <summary>
        /// Set the state of a Work Item.
        /// </summary>
        /// <param name="workItem">Work item to transition state of</param>
        /// <param name="state">Target state</param>
        /// <param name="commentPrefix">Added to historical comment</param>
        /// <param name="logger">Logger used to output information</param>
        /// <remarks>
        /// TFS has controls setup on State Transitions.
        /// Most templates do not allow you to go directly from a New state to a Done state.
        /// TFS Aggregator will cycle the target work item through what ever states it needs to to find the **shortest route** to the target state.
        /// (For most templates that is also the route that makes the most sense from a business perspective too.)
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarQube", "S3240:The simplest possible condition syntax should be used", Justification = "?: construct is harder to read in this case.")]
        public static void TransitionToState(IWorkItem workItem, string state, string commentPrefix, ILogEvents logger)
        {
            // Set the sourceWorkItem's state so that it is clear that it has been moved.
            string originalState = (string)workItem.Fields["System.State"].Value;

            // Try to set the state of the source work item to the "Deleted/Moved" state (whatever is defined in the file).

            // We need an open work item to set the state
            workItem.TryOpen();

            // See if we can go directly to the planned state.
            workItem.Fields["System.State"].Value = state;

            if (workItem.Fields["System.State"].Status != FieldStatus.Valid)
            {
                // Revert back to the original value and start searching for a way to our "MovedState"
                workItem.Fields["System.State"].Value = workItem.Fields["System.State"].OriginalValue;

                // If we can't then try to go from the current state to another state.  Saving each time till we get to where we are going.
                foreach (string curState in FindNextState(workItem.Type, (string)workItem.Fields["System.State"].Value, state))
                {
                    string comment;
                    if (curState == state)
                    {
                        comment = string.Format(
                            "{0}{1}  State changed to {2}",
                            commentPrefix,
                            Environment.NewLine,
                            state);
                    }
                    else
                    {
                        comment = string.Format(
                            "{0}{1}  State changed to {2} as part of move toward a state of {3}",
                            commentPrefix,
                            Environment.NewLine,
                            curState,
                            state);
                    }

                    bool success = ChangeWorkItemState(workItem, originalState, curState, comment, logger);

                    // If we could not do the incremental state change then we are done.  We will have to go back to the original...
                    if (!success)
                    {
                        break;
                    }
                }
            }
            else
            {
                // Just save it off if we can.
                string comment = commentPrefix + "\n   State changed to " + state;
                ChangeWorkItemState(workItem, originalState, state, comment, logger);
            }
        }

        private static bool ChangeWorkItemState(IWorkItem workItem, string orginalSourceState, string destState, string comment, ILogEvents logger)
        {
            // Try to save the new state.  If that fails then we also go back to the original state.
            try
            {
                workItem.TryOpen();
                workItem.Fields["System.State"].Value = destState;
                workItem.History = comment;

                logger.AttemptingToMoveWorkItemToState(workItem, orginalSourceState, destState);
                if (workItem.IsValid())
                {
                    logger.WorkItemIsValidToSave(workItem);
                }
                else
                {
                    logger.WorkItemIsInvalidInState(workItem, destState);
                }

                workItem.Save();
                return true;
            }
            catch (Exception)
            {
                // Revert back to the original value.
                workItem.Fields["System.State"].Value = orginalSourceState;
                return false;
            }
        }

        /// <summary>
        /// Used to find the next state on our way to a destination state.
        /// (Meaning if we are going from a "Not-Started" to a "Done" state,
        /// we usually have to hit a "in progress" state first.
        /// </summary>
        /// <param name="wiType">Work item type</param>
        /// <param name="fromState">source state</param>
        /// <param name="toState">target state</param>
        /// <returns></returns>
        private static IEnumerable<string> FindNextState(IWorkItemType wiType, string fromState, string toState)
        {
            var map = new Dictionary<string, string>();
            var edges = GetTransitions(wiType).ToDictionary(i => i.From, i => i.To);
            var q = new Queue<string>();
            map.Add(fromState, null);
            q.Enqueue(fromState);
            while (q.Count > 0)
            {
                var current = q.Dequeue();
                foreach (var s in edges[current])
                {
                    if (!map.ContainsKey(s))
                    {
                        map.Add(s, current);
                        if (s == toState)
                        {
                            var result = new Stack<string>();
                            var thisNode = s;
                            do
                            {
                                result.Push(thisNode);
                                thisNode = map[thisNode];
                            }
                            while (thisNode != fromState);

                            while (result.Count > 0)
                            {
                                yield return result.Pop();
                            }

                            yield break;
                        }

                        q.Enqueue(s);
                    }
                }
            }
        }

        /// <summary>
        /// Get the transitions for this <see cref="WorkItemType"/>
        /// </summary>
        private static List<StateTransition> GetTransitions(IWorkItemType workItemType)
        {
            List<StateTransition> currentTransitions;

            // See if this WorkItemType has already had it's transitions figured out.
            AllTransitions.TryGetValue(workItemType, out currentTransitions);
            if (currentTransitions != null)
            {
                return currentTransitions;
            }

            // Get this worktype type as xml
            XmlDocument workItemTypeXml = workItemType.Export(false);

            // Create a dictionary to allow us to look up the "to" state using a "from" state.
            var newTransistions = new List<StateTransition>();

            // get the transitions node.
            XmlNodeList transitionsList = workItemTypeXml.GetElementsByTagName("TRANSITIONS");

            // As there is only one transitions item we can just get the first
            XmlNode transitions = transitionsList[0];

            // Iterate all the transitions
            foreach (XmlNode transitionXML in transitions.Cast<XmlNode>())
            {
                // See if we have this from state already.
                string fromState = transitionXML.Attributes["from"].Value;
                StateTransition transition = newTransistions.Find(trans => trans.From == fromState);
                if (transition != null)
                {
                    transition.To.Add(transitionXML.Attributes["to"].Value);
                }
                else
                {
                    // If we could not find this state already then add it.

                    // save off the transition (from first so we can look up state progression.
                    newTransistions.Add(new StateTransition
                    {
                        From = transitionXML.Attributes["from"].Value,
                        To = new List<string> { transitionXML.Attributes["to"].Value }
                    });
                }
            }

            // Save off this transition so we don't do it again if it is needed.
            AllTransitions.Add(workItemType, newTransistions);

            return newTransistions;
        }
    }
}
