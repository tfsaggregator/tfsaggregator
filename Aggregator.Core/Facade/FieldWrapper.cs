using System;
using System.Globalization;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class FieldWrapper : IField
    {
        private readonly Field tfsField;

        public FieldWrapper(Field field)
        {
            this.tfsField = field;
        }

        public string Name
        {
            get
            {
                return this.tfsField.Name;
            }
        }

        public string ReferenceName
        {
            get
            {
                return this.tfsField.ReferenceName;
            }
        }

        public object Value
        {
            get
            {
                return this.tfsField.Value;
            }

            set
            {
                this.tfsField.Value = value;
            }
        }

        public FieldStatus Status
        {
            get { return this.tfsField.Status; }
        }

        public object OriginalValue
        {
            get { return this.tfsField.OriginalValue; }
        }

        public Type DataType
        {
            get
            {
                return this.tfsField.FieldDefinition.SystemType;
            }
        }
    }
}
