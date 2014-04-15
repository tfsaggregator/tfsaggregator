using System;
using System.Collections.Generic;

namespace TFSAggregator
{
    public class Transition
    {
        // The state we are moving from.
        public String From { get; set; }
        // All the To states for this from
        public List<String> To { get; set; }
    }
}