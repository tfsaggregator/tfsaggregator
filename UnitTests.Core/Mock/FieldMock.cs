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
        public FieldMock(string name)
        {
            
        }

        public string Name { get; set; }

        public string ReferenceName { get; set; }

        public object Value { get; set; }

        public FieldStatus Status { get; set; }

        public object OriginalValue { get; set; }
    }
}
