using System;
using System.Collections.Generic;

namespace Aggregator.Core.Navigation
{
    internal class StateTransition
    {
        // The state we are moving from.
        public string From { get; set; }

        // All the To states for this from
        public List<string> To { get; set; }

        public override string ToString()
        {
            return string.Format(
                "{0} -> {{{1}}}",
                this.From,
                string.Join(",", this.To));
        }
    }
}
