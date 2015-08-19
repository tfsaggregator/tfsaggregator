using System.Xml;

namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemType
    {
        string Name { get; }

        XmlDocument Export(bool includeGlobalListsFlag);
    }
}
