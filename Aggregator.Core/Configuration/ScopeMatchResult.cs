using System.Collections.Generic;

namespace Aggregator.Core.Configuration
{
    public class ScopeMatchResult
    {
        public bool Success { get; set; }

        private readonly List<string> arguments = new List<string>();

        public void Add(string argument)
        {
            this.arguments.Add(argument);
        }

        public void AddRange(IEnumerable<string> collection)
        {
            this.arguments.AddRange(collection);
        }

        public string Arguments
        {
            get
            {
                return string.Join(", ", this.arguments);
            }
        }
    }
}
