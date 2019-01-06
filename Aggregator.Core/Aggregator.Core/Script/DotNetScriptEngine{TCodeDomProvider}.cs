using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core.Script
{
    /// <summary>
    /// Compiles .Net code on the fly as if scripting engine
    /// </summary>
    public abstract class DotNetScriptEngine<TCodeDomProvider> : ScriptEngine
        where TCodeDomProvider : CodeDomProvider, new()
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.")]
        protected readonly string Namespace = "RESERVED";

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.")]
        protected readonly string ClassPrefix = "Script_";

        protected DotNetScriptEngine(ILogEvents logger, bool debug, IScriptLibrary library)
            : base(logger, debug, library)
        {
        }

        /// <summary>
        /// This property counts the number of generated lines _before_ actual user script code.
        /// </summary>
        protected abstract int LinesOfCodeBeforeScript { get; }

        protected abstract string WrapScript(string scriptName, string script, string functions);

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

        private bool SyntaxChecking(string scriptName, string code)
        {
            bool pass = true;

            using (var codeDomProvider = new TCodeDomProvider())
            {
                // Setup our options
                var compilerOptions = new CompilerParameters();
                compilerOptions.GenerateExecutable = false;
                compilerOptions.GenerateInMemory = true;
                compilerOptions.IncludeDebugInformation = true;

                // critical step
                compilerOptions.ReferencedAssemblies.AddRange(this.GetAssemblyReferences());

                var syntaxResult = codeDomProvider.CompileAssemblyFromSource(compilerOptions, code);

                if (syntaxResult.Errors.HasErrors || syntaxResult.Errors.HasWarnings)
                {
                    foreach (CompilerError err in syntaxResult.Errors.Cast<CompilerError>())
                    {
                        // compiler counts as lines: usings, namespace, class declaration, etc.
                        int scriptLine = err.Line - this.LinesOfCodeBeforeScript;
                        if (err.IsWarning)
                        {
                            this.Logger.ScriptHasWarning(scriptName, scriptLine, err.Column, err.ErrorNumber, err.ErrorText);
                        }
                        else
                        {
                            this.Logger.ScriptHasError(scriptName, scriptLine, err.Column, err.ErrorNumber, err.ErrorText);
                            pass = false;
                        }
                    }
                }
            }

            return pass;
        }

        private void CompileCode(string[] code)
        {
            using (var codeDomProvider = new TCodeDomProvider())
            {
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
        }

        private void RunScript(Assembly assembly, string scriptName, IWorkItem self, IWorkItemRepository store)
        {
            var classForScript = assembly.GetType(this.Namespace + "." + this.ClassPrefix + scriptName);
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

            this.Logger.ScriptLogger.RuleName = scriptName;

            // Lets run our script and display its results
            object result = scriptObject.RunScript(self, store, this.Logger.ScriptLogger, this.Library);
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

        // a simpler pattern is , this one matches .Net identifiers
        private readonly Regex regex = new Regex(
            @"\${(?<name>[A-Za-z_]\w*)}", // @"\${(?<name>[_\p{L}\p{Nl}][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*)}",
            RegexOptions.ExplicitCapture);

        private string ReplaceMacros(string source, Dictionary<string, string> macros)
        {
            return this.regex.Replace(source, (Match m) => macros[m.Groups["name"].Value]);
        }

        public override void Load(IEnumerable<Script.ScriptSourceElement> sourceElements)
        {
            this.sourceCode.Clear();
            this.GenerateSource(sourceElements);
            this.BuildAssembly();
        }

        private void GenerateSource(IEnumerable<ScriptSourceElement> sourceElements)
        {
            var snippets = sourceElements.Where(e => e.Type == ScriptSourceElementType.Snippet).ToDictionary((e) => e.Name, (e) => e.SourceCode);

            var functionsBuilder = new StringBuilder();
            foreach (var funcElement in sourceElements.Where(e => e.Type == ScriptSourceElementType.Function))
            {
                functionsBuilder.Append(funcElement.SourceCode);
                functionsBuilder.AppendLine();
            }

            string functionsCode = functionsBuilder.ToString();

            foreach (var ruleElement in sourceElements.Where((e) => e.Type == ScriptSourceElementType.Rule))
            {
                string ruleCode = this.ReplaceMacros(ruleElement.SourceCode, snippets);

                string code = this.WrapScript(ruleElement.Name, ruleCode, functionsCode);

                bool passed = this.SyntaxChecking(ruleElement.Name, code);
                if (passed)
                {
                    this.sourceCode.Add(ruleElement.Name, code);
                }
            }
        }

        protected bool BuildAssembly()
        {
            try
            {
                // build a single assembly and class from multiple scripts
                this.CompileCode(this.sourceCode.Values.ToArray());

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
