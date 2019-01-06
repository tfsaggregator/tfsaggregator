namespace Aggregator.Core.Configuration
{
    /// <summary>
    /// Represents a Rule of <see cref="TFSAggregatorSettings"/>.
    /// </summary>
    public class Rule : ScriptElement
    {
        public string Name { get; set; }

        public RuleScope[] Scope { get; set; }
    }
}
