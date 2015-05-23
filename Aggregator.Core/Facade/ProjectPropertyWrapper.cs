namespace Aggregator.Core.Facade
{
    using Aggregator.Core.Interfaces;

    public class ProjectPropertyWrapper : IProjectPropertyWrapper
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
