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

        public object Value { get; set; }

        public FieldStatus Status { get; set; }

        public object OriginalValue { get; set; }
    }
}
