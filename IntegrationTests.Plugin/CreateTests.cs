using System;
using Xunit;

namespace IntegrationTests.Plugin
{
    [Collection("TFS collection")]
    public class CreateTests
    {
        TfsFixture fixture;

        public CreateTests(TfsFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestMethod1()
        {
        }
    }
}
