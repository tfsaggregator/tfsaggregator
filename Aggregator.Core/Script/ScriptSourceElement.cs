using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Script
{
    public enum ScriptSourceElementType
    {
        Rule
    }

    public class ScriptSourceElement
    {
        public string Name;
        public ScriptSourceElementType Type;
        public string SourceCode;
    }
}
