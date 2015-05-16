namespace Aggregator.Core
{
    using System.Xml;

    public interface IWorkItemType
    {
        string Name { get; }
        XmlDocument Export(bool includeGlobalListsFlag);
    }
}
