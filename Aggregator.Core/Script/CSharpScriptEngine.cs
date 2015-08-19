using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;
using Aggregator.Core.Script;

namespace Aggregator.Core
{
    using Microsoft.CSharp;

    public class CSharpScriptEngine : DotNetScriptEngine<CSharpCodeProvider>
    {
        public CSharpScriptEngine(IWorkItemRepository store, ILogEvents logger)
            : base(store, logger)
        {
        }

        protected override int LineOffset { get { return 10; } }

        protected override string WrapScript(string scriptName, string script)
        {
            return @"
namespace RESERVED
{
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using Aggregator.Core;
  using Aggregator.Core.Extensions;
  using Aggregator.Core.Interfaces;
  using Aggregator.Core.Navigation;
  using System.Linq;

  public class Script_" + scriptName + @" : Aggregator.Core.Script.IDotNetScript
  {
    public object RunScript(Aggregator.Core.Interfaces.IWorkItemExposed self, Aggregator.Core.Interfaces.IWorkItemRepositoryExposed store)
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
