using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Script;

namespace Aggregator.Core
{
    using Microsoft.VisualBasic;

#pragma warning disable S101 // Types should be named in camel case
    public class VBNetScriptEngine : DotNetScriptEngine<VBCodeProvider>
#pragma warning restore S101 // Types should be named in camel case
    {
        public VBNetScriptEngine(ILogEvents logger, bool debug, IScriptLibrary library)
            : base(logger, debug, library)
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
  
    Public Function RunScript(ByVal self As IWorkItemExposed, ByVal store As IWorkItemRepositoryExposed, ByVal logger As  IRuleLogger, ByVal Library As IScriptLibrary) As Object Implements Aggregator.Core.Script.IDotNetScript.RunScript
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
