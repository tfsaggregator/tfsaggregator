using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    public interface IFieldCollectionWrapper
    {
        IFieldWrapper this[string name] { get; set; }
    }
}
