using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;

namespace Aggregator.Core
{
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    public interface IDotNetScript
    {
        object RunScript(IWorkItem self);
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

        string[] GetAssemblyReferences()
        {
            string baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var refList = new List<string>();


            refList.Add(Assembly.GetExecutingAssembly().Location);
            // from GAC
            refList.Add("System.dll");
            // CAREFUL HERE and remember to AddReference and set CopyLocal=true in UnitTest project!
            var wiAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(ass => ass.GetName().Name == "Microsoft.TeamFoundation.WorkItemTracking.Client").First();

            refList.Add(new Uri(wiAssembly.CodeBase).LocalPath);

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
            compilerOptions.ReferencedAssemblies.AddRange(GetAssemblyReferences());

            CompilerResults compilerResult;
            compilerResult = codeDomProvider.CompileAssemblyFromSource(compilerOptions, code);

            if (compilerResult.Errors.HasErrors)
            {
                foreach (CompilerError err in compilerResult.Errors)
                {
                    logger.ScriptHasError("***", err.Line - this.LineOffset, err.Column, err.ErrorNumber, err.ErrorText);
                }
                return null;
            }

            if (compilerResult.Errors.HasWarnings)
            {
                foreach (CompilerError err in compilerResult.Errors)
                {
                    //TODO warning instead of errors
                    logger.ScriptHasError("***", err.Line - 8, err.Column, err.ErrorNumber, err.ErrorText);
                }
            }

            return compilerResult;
        }

        private void RunScript(Assembly assembly, string scriptName, IWorkItem self)
        {
            // Now that we have a compiled script, lets run them
            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (Type iface in type.GetInterfaces())
                {
                    if (iface == typeof(IDotNetScript))
                    {
                        ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                        if (constructor != null && constructor.IsPublic)
                        {
                            // we specified that we wanted a constructor that doesn't take parameters, so don't pass parameters
                            IDotNetScript scriptObject = constructor.Invoke(null) as IDotNetScript;
                            if (scriptObject != null)
                            {
                                //Lets run our script and display its results
                                object result = scriptObject.RunScript(self);
                                logger.ResultsFromScriptRun(scriptName, result);
                            }
                            else
                            {
                                // hmmm, for some reason it didn't create the object
                                // this shouldn't happen, as we have been doing checks all along, but we should
                                // inform the user something bad has happened, and possibly request them to send
                                // you the script so you can debug this problem
                            }
                        }
                        else
                        {
                            // and even more friendly and explain that there was no valid constructor
                            // found and that's why this script object wasn't run
                        }
                    }
                }
            }
        }

        private static void CleanUp(bool debug, CompilerResults compilerResult)
        {
            if (debug)
            {
                compilerResult.TempFiles.KeepFiles = false;
                compilerResult.TempFiles.Delete();
            }
        }

        bool debug = true;
        Dictionary<string, string> sourceCode = new Dictionary<string, string>();
        CompilerResults compilerResult;

        public override bool Load(string scriptName, string script)
        {
            string code = WrapScript(scriptName, script);
            sourceCode.Add(scriptName, code);
            return true;
        }

        public override bool LoadCompleted()
        {
            // build a single assembly and class from multiple scripts
            compilerResult = CompileCode(sourceCode.Values.ToArray(), debug);
            return !compilerResult.Errors.HasErrors;
        }

        public override void Run(string scriptName, IWorkItem workItem)
        {
            if (!compilerResult.Errors.HasErrors)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                RunScript(compilerResult.CompiledAssembly, scriptName, workItem);
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
            CleanUp(debug, compilerResult);
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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
                    return Assembly.LoadFile(file);
            }
            return null;
        }
    }
}
