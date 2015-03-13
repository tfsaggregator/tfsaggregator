using Aggregator.Core;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Core
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;



    using UnitTests.Core.Mock;

    [TestClass]
    public class Basics
    {
        [TestMethod]
        public void Can_load_a_fake_xml_configuration()
        {
            var settings = TestHelpers.LoadConfigFromResourceFile("NoOp.policies");
            var level = settings.LogLevel;
            Assert.AreEqual(LogLevel.Diagnostic, level);
        }
    }
}
