using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    public interface IWorkItem
    {
        IFieldCollectionWrapper Fields { get; }
        TType GetField<TType>(string fieldName, TType defaultValue);
        string History { get; set; }
        int Id { get; }
        bool IsValid();
        void PartialOpen();
        void Save();
        object this[string name] { get; set; }
        void TryOpen();
        string TypeName { get; }
        ArrayList Validate();
        WorkItemLinkCollection WorkItemLinks { get; }
        // navigation helper
        IWorkItem Parent { get; }
    }
}
