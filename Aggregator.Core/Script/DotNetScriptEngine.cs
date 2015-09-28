using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Script
{
    public interface IDotNetScript
    {
        object RunScript(IWorkItemExposed self, IWorkItemRepositoryExposed store, IRuleLogger scriptLogger);
    }

    /// <summary>
    /// Compiles .Net code on the fly as if scripting engine
    /// </summary>
    public abstract class DotNetScriptEngine<TCodeDomProvider> : ScriptEngine
        where TCodeDomProvider : CodeDomProvider, new()
    {
        protected DotNetScriptEngine(ILogEvents logger, bool debug)
            : base(logger, debug)
        {
        }

        protected abstract int LineOffset { get; }

        protected abstract string WrapScript(string scriptName, string script);

        private string[] GetAssemblyReferences()
        {
            var refList = new List<string>();

            refList.Add(Assembly.GetExecutingAssembly().Location);
            refList.Add("System.dll"); // from GAC
            refList.Add("System.Core.dll");

            // CAREFUL HERE and remember to AddReference and set CopyLocal=true in UnitTest project!
            var wiAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(ass => ass.GetName().Name == "Microsoft.TeamFoundation.WorkItemTracking.Client");

            if (wiAssembly != null)
            {
                refList.Add(new Uri(wiAssembly.CodeBase).LocalPath);
            }
            else
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string asmPath = Path.Combine(dir, "Microsoft.TeamFoundation.WorkItemTracking.Client.dll");
                refList.Add(asmPath);
            }

            return refList.ToArray();
        }

        private void CompileCode(string[] code)
        {
            var codeDomProvider = new TCodeDomProvider();

            // Setup our options
            var compilerOptions = new CompilerParameters();
            compilerOptions.GenerateExecutable = false;
            compilerOptions.GenerateInMemory = true;
            compilerOptions.IncludeDebugInformation = this.Debug;

            // save temp files to permit debugging
            compilerOptions.TempFiles.KeepFiles = this.Debug;

            // critical step
            compilerOptions.ReferencedAssemblies.AddRange(this.GetAssemblyReferences());

            this.compilerResult = codeDomProvider.CompileAssemblyFromSource(compilerOptions, code);
        }

        private void RunScript(Assembly assembly, string scriptName, IWorkItem self, IWorkItemRepository store)
        {
            // HACK name must match C# and VB.NET implementations
            var classForScript = assembly.GetType("RESERVED.Script_" + scriptName);
            if (classForScript == null)
            {
                this.Logger.FailureLoadingScript(scriptName);
                return;
            }

            var interfaceForScript = classForScript.GetInterface(typeof(IDotNetScript).Name);
            if (interfaceForScript == null)
            {
                this.Logger.FailureLoadingScript(scriptName);
                return;
            }

            ConstructorInfo constructor = classForScript.GetConstructor(Type.EmptyTypes);
            if (constructor == null || !constructor.IsPublic)
            {
                this.Logger.FailureLoadingScript(scriptName);
                return;
            }

            // we specified that we wanted a constructor that doesn't take parameters, so don't pass parameters
            IDotNetScript scriptObject = constructor.Invoke(null) as IDotNetScript;
            if (scriptObject == null)
            {
                this.Logger.FailureLoadingScript(scriptName);
                return;
            }

            System.Diagnostics.Debug.WriteLine("*** about to execute {0}", scriptName, null);
            this.Logger.ScriptLogger.RuleName = scriptName;

            // Lets run our script and display its results
            object result = scriptObject.RunScript(self, store, this.Logger.ScriptLogger);
            this.Logger.ResultsFromScriptRun(scriptName, result);
        }

        private void CleanUp()
        {
            if (this.Debug)
            {
                this.compilerResult.TempFiles.KeepFiles = false;
                this.compilerResult.TempFiles.Delete();
            }
        }

        private readonly Dictionary<string, string> sourceCode = new Dictionary<string, string>();

        private CompilerResults compilerResult;

        public override bool Load(string scriptName, string script)
        {
            string code = this.WrapScript(scriptName, script);
            this.sourceCode.Add(scriptName, code);
            return true;
        }

        public override bool LoadCompleted()
        {
            try
            {
                // build a single assembly and class from multiple scripts
                this.CompileCode(this.sourceCode.Values.ToArray());

                // TODO find a way to get where the error is
                if (this.compilerResult.Errors.HasErrors)
                {
                    foreach (CompilerError err in this.compilerResult.Errors.Cast<CompilerError>())
                    {
                        this.Logger.ScriptHasError("***", err.Line - this.LineOffset, err.Column, err.ErrorNumber, err.ErrorText);
                    }
                }

                if (this.compilerResult.Errors.HasWarnings)
                {
                    foreach (CompilerError err in this.compilerResult.Errors.Cast<CompilerError>())
                    {
                        this.Logger.ScriptHasWarning("***", err.Line - this.LineOffset, err.Column, err.ErrorNumber, err.ErrorText);
                    }
                }

                return !this.compilerResult.Errors.HasErrors;
            }
            finally
            {
                this.CleanUp();
            }
        }

        public override void Run(string scriptName, IWorkItem workItem, IWorkItemRepository store)
        {
            if (!this.compilerResult.Errors.HasErrors)
            {
                ResolveEventHandler resolveAssemblies = (s, e) => ResolveAssembly(e);
                try
                {
                    AppDomain.CurrentDomain.AssemblyResolve += resolveAssemblies;
                    this.RunScript(this.compilerResult.CompiledAssembly, scriptName, workItem, store);
                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= resolveAssemblies;
                }
            }
            else
            {
                // compile errors slip away in the log, reinstate that something is wrong
                this.Logger.FailureLoadingScript(scriptName);
            }

            // BUG: must have a "clean up event" fired at shutdown time
        }

        private static Assembly ResolveAssembly(ResolveEventArgs args)
        {
            Match m = Regex.Match(
                args.Name,
                "^[a-z0-9]+(\\.[a-z0-9]+)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var loadedAssembly = AppDomain.CurrentDomain
                .GetAssemblies().FirstOrDefault(ass => string.Equals(ass.GetName().Name, m.Value, StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }
            else
            {
                var dir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                var file = Path.Combine(dir, args.Name + ".dll");
                if (File.Exists(file))
                {
                    return Assembly.LoadFile(file);
                }
            }

            return null;
        }
    }
}
