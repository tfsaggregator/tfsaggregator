using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public class VBNetScriptEngine : DotNetScriptEngine<Microsoft.VisualBasic.VBCodeProvider>
    {
        public VBNetScriptEngine(string scriptName, string script, IWorkItemRepository store, ILogEvents logger)
            : base(scriptName, script, store, logger)
        {
        }

        protected override string WrapScript(string script)
        {
            return @"
Imports Microsoft.TeamFoundation.WorkItemTracking.Client

Namespace RESERVED
  Public Class Script_" + this.scriptName + @"
    Implements Aggregator.Core.IDotNetScript
  
    Public Function RunScript(ByVal self As Aggregator.Core.IWorkItem) As Object Implements Aggregator.Core.IDotNetScript.RunScript
" + script + @"
      Return Nothing
    End Function
  End Class
End Namespace
";
        }
    }
}
