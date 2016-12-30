using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Script;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core
{
    using Microsoft.CSharp;

    public class CSharpScriptEngine : DotNetScriptEngine<CSharpCodeProvider>
    {
        public CSharpScriptEngine(ILogEvents logger, bool debug, IScriptLibrary library)
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
            return "namespace " + this.Namespace + @"
{
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using Aggregator.Core;
  using Aggregator.Core.Extensions;
  using Aggregator.Core.Interfaces;
  using Aggregator.Core.Navigation;
  using Aggregator.Core.Monitoring;
  using System.Linq;
  using CoreFieldReferenceNames = Microsoft.TeamFoundation.WorkItemTracking.Client.CoreFieldReferenceNames;

  public class " + this.ClassPrefix + scriptName + @" : Aggregator.Core.Script.IDotNetScript
  {
    public object RunScript(IWorkItemExposed self, IWorkItemRepositoryExposed store, IRuleLogger logger, IScriptLibrary Library)
    {
" + script + @"
      return null;
    }
" + functions + @"
  }
}
";
        }
    }
}
