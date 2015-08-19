using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace UnitTests.Core.Mock
{
    internal class FieldMock : IFieldWrapper
    {
        private readonly WorkItemMock workItemMock;

        private object value;

        public FieldMock(WorkItemMock workItemMock, string name)
        {
            this.workItemMock = workItemMock;
            this.Name = name;
        }

        public string Name { get; set; }

        public string ReferenceName { get; set; }

        public object Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
                this.workItemMock.IsDirty = true;
            }
        }

        public FieldStatus Status { get; set; }

        public object OriginalValue { get; set; }
    }
}
