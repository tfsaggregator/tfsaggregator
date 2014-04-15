using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TFSAggregator;

namespace UnitTests
{
    internal static class TestHelpers
    {
        private static string LoadTextFromEmbeddedResource(string resourceName)
        {
            try
            {
                var thisAssembly = Assembly.GetAssembly(typeof(FakeXmlConfigurations));
                var stream = thisAssembly.GetManifestResourceStream("UnitTests.ConfigurationsForTests." + resourceName);
                var textStream = new StreamReader(stream);
                return textStream.ReadToEnd();
            }
            catch
            {
                Assert.Fail("Couldn't load embedded resource " + resourceName);
                return "";
            }
        }

        public static void SetConfigResourceFile(string fileName)
        {
            var configXml = LoadTextFromEmbeddedResource("SumFieldsOnSingleWorkItem.xml");
            TFSAggregatorSettings.SettingsOverride = configXml;
        }
    }
}
