using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Facade
{
    public class ProjectPropertyWrapper : IProjectProperty
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
