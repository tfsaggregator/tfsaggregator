using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public static class IWorkItemExposedExtensions
    {
        /// <summary>
        /// Used to convert a field to a number.  If anything goes wrong then the default value is returned.
        /// </summary>
        /// <param name="workItem"></param>
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
    }
}
