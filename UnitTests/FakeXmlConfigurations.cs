using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TFSAggregator;
using System.Reflection;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class FakeXmlConfigurations
    {
        private string LoadTextFromEmbeddedResource(string resourceName)
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

        [TestMethod]
        public void Can_load_a_fake_xml_configuration()
        {
            var configXml = LoadTextFromEmbeddedResource("SumFieldsOnSingleWorkItem.xml");

            TFSAggregatorSettings.SettingsOverride = configXml;
            string tfsUri = TFSAggregatorSettings.TFSUri;
            Assert.AreEqual("http://fakeServer:8080/tfs/DefaultCollection", tfsUri);
        }
    }
}
