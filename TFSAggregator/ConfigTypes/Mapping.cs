using System.Collections.Generic;

namespace TFSAggregator.ConfigTypes
{
    public class Mapping
    {
        public Mapping()
        {
            SourceValues = new List<string>();
        }

        /// <summary>
        /// The value to set to if all of the source work items we 
        /// compare to have SourceValues 
        /// </summary>
        public string TargetValue { get; set; }

        /// <summary>
        /// If all/one the WorkItems we compare to have values in this list 
        /// then we will set the target work item to have the target value.
        /// </summary>
        public List<string> SourceValues { get; set; }

        /// <summary>
        /// If true then all of the source values have to be valid.
        /// If false then only one of the source values have to be vaild.
        /// Example for True: You would set it to true for a mapping to done 
        /// (you want all the children Done before the parent is Done).
        /// Example for False: You would set it to false for a mapping to In Progress
        /// (if even one child is in progress then the parent is In Progress).
        /// </summary>
        public bool Inclusive { get; set; }
    }
}