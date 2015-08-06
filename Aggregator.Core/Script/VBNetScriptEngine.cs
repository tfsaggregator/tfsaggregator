namespace Aggregator.Core
{
    using Microsoft.VisualBasic;

    public class VBNetScriptEngine : DotNetScriptEngine<VBCodeProvider>
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
  
    Public Function RunScript(ByVal self As Aggregator.Core.IWorkItemExposed, ByVal store As Aggregator.Core.IWorkItemRepositoryExposed) As Object Implements Aggregator.Core.IDotNetScript.RunScript
" + script + @"
      Return Nothing
    End Function
  End Class
End Namespace
";
        }
    }
}
