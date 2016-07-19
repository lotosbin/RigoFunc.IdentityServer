using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore.UnitTests {
    public class EntityFrameworkCoreClientStoreTests : IClassFixture<IdentityDbContextFixture> {
        private IdentityDbContextFixture _fixture;

        public EntityFrameworkCoreClientStoreTests(IdentityDbContextFixture fixture) {
            _fixture = fixture;
        }

        [Fact]
        public void FindClientByIdAsyncTest() {
            var clientStore = new EntityFrameworkCoreClientStore(_fixture.DbContext);
            var client = clientStore.FindClientByIdAsync("testclient").Result;
            Assert.NotNull(client);
        }
    }
}
