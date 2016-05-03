using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Script;

namespace Aggregator.Core
{
    using Microsoft.VisualBasic;

    public class VBNetScriptEngine : DotNetScriptEngine<VBCodeProvider>
    {
        public VBNetScriptEngine(ILogEvents logger, bool debug)
            : base(logger, debug)
        {
        }

        protected override int LinesOfCodeBeforeScript
        {
            get
            {
                return 15;
            }
        }

        protected override string WrapScript(string scriptName, string script, string functions)
        {
            return @"
Imports Microsoft.TeamFoundation.WorkItemTracking.Client
Imports Aggregator.Core
Imports Aggregator.Core.Extensions
Imports Aggregator.Core.Interfaces
Imports Aggregator.Core.Navigation
Imports Aggregator.Core.Monitoring
Imports System.Linq
Imports CoreFieldReferenceNames = Microsoft.TeamFoundation.WorkItemTracking.Client.CoreFieldReferenceNames

Namespace " + this.Namespace + @"
  Public Class " + this.ClassPrefix + scriptName + @"
    Implements Aggregator.Core.Script.IDotNetScript
  
    Public Function RunScript(ByVal self As Aggregator.Core.Interfaces.IWorkItemExposed, ByVal store As Aggregator.Core.Interfaces.IWorkItemRepositoryExposed, ByVal logger As  Aggregator.Core.Monitoring.IRuleLogger) As Object Implements Aggregator.Core.Script.IDotNetScript.RunScript
" + script + @"
      Return Nothing
    End Function
" + functions + @"
  End Class
End Namespace
";
        }
    }
}
