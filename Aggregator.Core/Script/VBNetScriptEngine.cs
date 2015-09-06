using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Script;

namespace Aggregator.Core
{
    using Microsoft.VisualBasic;

    public class VBNetScriptEngine : DotNetScriptEngine<VBCodeProvider>
    {
        public VBNetScriptEngine(IWorkItemRepository store, ILogEvents logger)
            : base(store, logger)
        {
        }

        protected override int LineOffset
        {
            get
            {
                return 11;
            }
        }

        protected override string WrapScript(string scriptName, string script)
        {
            return @"
Imports Microsoft.TeamFoundation.WorkItemTracking.Client
Imports Aggregator.Core
Imports Aggregator.Core.Extensions
Imports Aggregator.Core.Interfaces
Imports Aggregator.Core.Navigation
Imports Aggregator.Core.Monitoring

Namespace RESERVED
  Public Class Script_" + scriptName + @"
    Implements Aggregator.Core.Script.IDotNetScript
  
    Public Function RunScript(ByVal self As Aggregator.Core.Interfaces.IWorkItemExposed, ByVal store As Aggregator.Core.Interfaces.IWorkItemRepositoryExposed, ByVal logger As  Aggregator.Core.Monitoring.IRuleLogger) As Object Implements Aggregator.Core.Script.IDotNetScript.RunScript
" + script + @"
      Return Nothing
    End Function
  End Class
End Namespace
";
        }
    }
}
