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
        protected DotNetScriptEngine(IWorkItemRepository store, ILogEvents logger)
            : base(store, logger)
        {
        }

        protected abstract int LineOffset { get; }

        protected abstract string WrapScript(string scriptName, string script);

        private string[] GetAssemblyReferences()
        {
            var refList = new List<string>();

            refList.Add(Assembly.GetExecutingAssembly().Location);
            refList.Add("System.dll"); // from GAC

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

        private CompilerResults CompileCode(string[] code, bool debug = false)
        {
            var codeDomProvider = new TCodeDomProvider();

            // Setup our options
            var compilerOptions = new CompilerParameters();
            compilerOptions.GenerateExecutable = false;
            compilerOptions.GenerateInMemory = true;
            compilerOptions.IncludeDebugInformation = debug;

            // save temp files to permit debugging
            compilerOptions.TempFiles.KeepFiles = debug;

            // critical step
            compilerOptions.ReferencedAssemblies.AddRange(this.GetAssemblyReferences());

            return codeDomProvider.CompileAssemblyFromSource(compilerOptions, code);
        }

        private void RunScript(Assembly assembly, string scriptName, IWorkItem self)
        {
            // HACK name must match C# and VB.NET implementations
            var classForScript = assembly.GetType("RESERVED.Script_" + scriptName);
            if (classForScript == null)
            {
                this.logger.FailureLoadingScript(scriptName);
                return;
            }

            var interfaceForScript = classForScript.GetInterface(typeof(IDotNetScript).Name);
            if (interfaceForScript == null)
            {
                this.logger.FailureLoadingScript(scriptName);
                return;
            }

            ConstructorInfo constructor = classForScript.GetConstructor(Type.EmptyTypes);
            if (constructor == null || !constructor.IsPublic)
            {
                this.logger.FailureLoadingScript(scriptName);
                return;
            }

            // we specified that we wanted a constructor that doesn't take parameters, so don't pass parameters
            IDotNetScript scriptObject = constructor.Invoke(null) as IDotNetScript;
            if (scriptObject == null)
            {
                this.logger.FailureLoadingScript(scriptName);
                return;
            }

            System.Diagnostics.Debug.WriteLine("*** about to execute {0}", scriptName, null);
            this.logger.ScriptLogger.RuleName = scriptName;

            // Lets run our script and display its results
            object result = scriptObject.RunScript(self, this.store, this.logger.ScriptLogger);
            this.logger.ResultsFromScriptRun(scriptName, result);
        }

        private static void CleanUp(bool debug, CompilerResults compilerResult)
        {
            if (debug)
            {
                compilerResult.TempFiles.KeepFiles = false;
                compilerResult.TempFiles.Delete();
            }
        }

        private bool debug = true;

        private Dictionary<string, string> sourceCode = new Dictionary<string, string>();

        private CompilerResults compilerResult;

        public override bool Load(string scriptName, string script)
        {
            string code = this.WrapScript(scriptName, script);
            this.sourceCode.Add(scriptName, code);
            return true;
        }

        public override bool LoadCompleted()
        {
            // build a single assembly and class from multiple scripts
            this.compilerResult = this.CompileCode(this.sourceCode.Values.ToArray(), this.debug);

            // TODO find a way to get where the error is
            if (this.compilerResult.Errors.HasErrors)
            {
                foreach (CompilerError err in this.compilerResult.Errors.Cast<CompilerError>())
                {
                    this.logger.ScriptHasError("***", err.Line - this.LineOffset, err.Column, err.ErrorNumber, err.ErrorText);
                }
            }

            if (this.compilerResult.Errors.HasWarnings)
            {
                foreach (CompilerError err in this.compilerResult.Errors.Cast<CompilerError>())
                {
                    this.logger.ScriptHasWarning("***", err.Line - this.LineOffset, err.Column, err.ErrorNumber, err.ErrorText);
                }
            }

            return !this.compilerResult.Errors.HasErrors;
        }

        public override void Run(string scriptName, IWorkItem workItem)
        {
            if (!this.compilerResult.Errors.HasErrors)
            {
                AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
                try
                {
                    this.RunScript(this.compilerResult.CompiledAssembly, scriptName, workItem);
                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
                }
            }
            else
            {
                // compile errors slip away in the log, reinstate that something is wrong
                this.logger.FailureLoadingScript(scriptName);
            }

            //BUG must have a "clean up event" fired at shutdown time
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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
