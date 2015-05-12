using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Navigation
{
    class StateTransition
    {
        // The state we are moving from.
        public String From { get; set; }
        // All the To states for this from
        public List<String> To { get; set; }

        public override string ToString()
        {
            return string.Format(
                "{0} -> {{{1}}}",
                this.From,
                string.Join(",",this.To)
                );
        }
    }
}
