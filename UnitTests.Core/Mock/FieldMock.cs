using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTests.Core.Mock
{
    using Aggregator.Core;

    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    class FieldMock : IFieldWrapper
    {
        private WorkItemMock workItemMock;

        public FieldMock(WorkItemMock workItemMock, string name)
        {
            this.workItemMock = workItemMock;
            this.Name = name;
        }

        public string Name { get; set; }

        public string ReferenceName { get; set; }

        private object _value;
        public object Value
        {
            get { return _value; }
            set { _value = value; this.workItemMock.IsDirty = true; }
        }

        public FieldStatus Status { get; set; }

        public object OriginalValue { get; set; }
    }
}
