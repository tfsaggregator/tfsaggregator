using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests.Plugin
{
    [CollectionDefinition("TFS collection")]
    public class TfsCollection : ICollectionFixture<TfsFixture>
    {
    }
}
