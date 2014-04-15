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
        [TestMethod]
        public void Can_load_a_fake_xml_configuration()
        {
            TestHelpers.SetConfigResourceFile("SumFieldsOnSingleWorkItem.xml");
            string tfsUri = TFSAggregatorSettings.TFSUri;
            Assert.AreEqual("http://fakeServer:8080/tfs/DefaultCollection", tfsUri);
        }
    }
}
