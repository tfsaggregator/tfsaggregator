using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    public class VBNetScriptEngine : DotNetScriptEngine<Microsoft.VisualBasic.VBCodeProvider>
    {
        public VBNetScriptEngine(IWorkItemRepository store, ILogEvents logger)
            : base(store, logger)
        {
        }

        protected override int LineOffset { get { return 10; } }

        protected override string WrapScript(string scriptName, string script)
        {
            return @"
Imports Microsoft.TeamFoundation.WorkItemTracking.Client
Imports Aggregator.Core

Namespace RESERVED
  Public Class Script_" + scriptName + @"
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
