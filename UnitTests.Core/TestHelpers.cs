using Aggregator.Core.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core
{
    internal static class TestHelpers
    {
        private static string LoadTextFromEmbeddedResource(string resourceName)
        {
            try
            {
                var thisAssembly = Assembly.GetAssembly(typeof(Basics));
                var stream = thisAssembly.GetManifestResourceStream("UnitTests.Core.ConfigurationsForTests." + resourceName);
                var textStream = new StreamReader(stream);
                return textStream.ReadToEnd();
            }
            catch
            {
                Assert.Fail("Couldn't load embedded resource " + resourceName);
                return "";
            }
        }

        public static TFSAggregatorSettings LoadConfigFromResourceFile(string fileName, Aggregator.Core.ILogEvents logger)
        {
            var configXml = LoadTextFromEmbeddedResource(fileName);
            return TFSAggregatorSettings.LoadXml(configXml, logger);
        }

        public static void LoadAndRun(this Aggregator.Core.ScriptEngine engine, string scriptName, string script, Aggregator.Core.IWorkItem workItem)
        {
            engine.Load(scriptName, script);
            engine.LoadCompleted();
            engine.Run(scriptName, workItem);
        }
    }
}
