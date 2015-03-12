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
    public interface IDotNetScript
    {
        object RunScript(IWorkItem self, IWorkItem parent);
    }

    /// <summary>
    /// Compiles .Net code on the fly as if scripting engine
    /// </summary>
    public abstract class DotNetScriptEngine<TCodeDomProvider> : ScriptEngine
        where TCodeDomProvider : CodeDomProvider, new()
    {
        protected DotNetScriptEngine(string scriptName, string script, IWorkItemRepository store, ILogEvents logger)
            : base(scriptName, script, store, logger)
        {
        }

        protected abstract string WrapScript(string script);

        string[] GetAssemblyReferences()
        {
            string baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var refList = new List<string>();


            refList.Add(Assembly.GetExecutingAssembly().Location);
            // from GAC
            refList.Add("System.dll");
            // CAREFUL HERE and remember to AddReference and set CopyLocal=true in UnitTest project!
            refList.Add(System.IO.Path.Combine(baseDir, "Microsoft.TeamFoundation.WorkItemTracking.Client.dll"));

            return refList.ToArray();
        }

        private Assembly CompileCode(string code)
        {
            var codeDomProvider = new TCodeDomProvider();

            // Setup our options
            var compilerOptions = new CompilerParameters();
            compilerOptions.GenerateExecutable = false;
            compilerOptions.GenerateInMemory = true;//debugging only!!!
            compilerOptions.IncludeDebugInformation = true;
            // critical step
            compilerOptions.ReferencedAssemblies.AddRange(GetAssemblyReferences());

            CompilerResults compilerResult;
            compilerResult = codeDomProvider.CompileAssemblyFromSource(compilerOptions, code);

            if (compilerResult.Errors.HasErrors)
            {
                foreach (CompilerError err in compilerResult.Errors)
                {
                    logger.ScriptHasError(this.scriptName, err.Line - 9, err.Column, err.ErrorNumber, err.ErrorText);
                }
                return null;
            }

            if (compilerResult.Errors.HasWarnings)
            {
                foreach (CompilerError err in compilerResult.Errors)
                {
                    //TODO warning instead of errors
                    logger.ScriptHasError(this.scriptName, err.Line - 8, err.Column, err.ErrorNumber, err.ErrorText);
                }
            }

            return compilerResult.CompiledAssembly;
        }

        private void RunScript(Assembly assembly, IWorkItem self, IWorkItem parent)
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
                                object result = scriptObject.RunScript(self, parent);
                                logger.ResultsFromScriptRun(this.scriptName, result);
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

        override public void Run(IWorkItem workItem)
        {
            // TODO we can build a single assembly and class from multiple scripts
            // a method for each script
            string code = WrapScript(this.script);
            var asm = CompileCode(code);
            if (asm != null)
            {
                RunScript(asm, workItem, workItem.Parent);
            }
        }
    }
}
