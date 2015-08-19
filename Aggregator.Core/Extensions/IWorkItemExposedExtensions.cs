using System;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Navigation;

namespace Aggregator.Core.Extensions
{
    public static class IWorkItemExposedExtensions
    {
        /// <summary>
        /// Used to convert a field to a number.  If anything goes wrong then the default value is returned.
        /// </summary>
        /// <param name="self">The work item to get the field data from</param>
        /// <param name="fieldName">The name of the field to be retrieved</param>
        /// <param name="defaultValue">Value to be returned if something goes wrong.</param>
        /// <returns></returns>
        public static TType GetField<TType>(this IWorkItemExposed self, string fieldName, TType defaultValue)
        {
            try
            {
                TType convertedValue = (TType)self[fieldName];
                return convertedValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static bool HasParent(this IWorkItemExposed self)
        {
            return self.HasRelation(WorkItemImplementationBase.ParentRelationship);
        }

        public static bool HasChildren(this IWorkItemExposed self)
        {
            return self.HasRelation(WorkItemImplementationBase.ChildRelationship);
        }

        // fluent API for GetRelatives
        public static FluentQuery WhereTypeIs(this IWorkItemExposed wi, string workItemType)
        {
            return new FluentQuery(wi).WhereTypeIs(workItemType);
        }

        public static FluentQuery AtMost(this IWorkItemExposed wi, int levels)
        {
            return new FluentQuery(wi).AtMost(levels);
        }

        public static FluentQuery FollowingLinks(this IWorkItemExposed wi, string linkType)
        {
            return new FluentQuery(wi).FollowingLinks(linkType);
        }
    }
}
