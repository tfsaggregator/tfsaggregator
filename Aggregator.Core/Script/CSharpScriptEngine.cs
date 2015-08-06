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
  using Aggregator.Core.Navigation;
  public class Script_" + scriptName + @" : Aggregator.Core.IDotNetScript
  {
    public object RunScript(Aggregator.Core.IWorkItemExposed self, Aggregator.Core.IWorkItemRepositoryExposed store)
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
