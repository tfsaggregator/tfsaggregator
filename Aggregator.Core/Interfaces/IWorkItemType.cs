using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Aggregator.Core
{
    public interface IWorkItemType
    {
        string Name { get; }
        XmlDocument Export(bool includeGlobalListsFlag);
    }
}
