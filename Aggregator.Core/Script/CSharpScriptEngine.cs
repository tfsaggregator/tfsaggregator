using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public class CSharpScriptEngine : DotNetScriptEngine<Microsoft.CSharp.CSharpCodeProvider>
    {
        public CSharpScriptEngine(string scriptName, string script, IWorkItemRepository store, ILogEvents logger)
            : base(scriptName, script, store, logger)
        {
        }

        protected override string WrapScript(string script)
        {
            return @"
namespace RESERVED
{
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  public class Script_" + this.scriptName + @" : Aggregator.Core.IDotNetScript
  {
    public object RunScript(Aggregator.Core.IWorkItem self)
    {
" + script + @"
      return null;
    }
  }
}
";
        }
    }
}
