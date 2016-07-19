using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore.UnitTests {
    public class EntityFrameworkCoreScopeStoreTests : IClassFixture<IdentityDbContextFixture> {
        IdentityDbContextFixture _fixture;

        public EntityFrameworkCoreScopeStoreTests(IdentityDbContextFixture fixture) {
            _fixture = fixture;
        }
        [Fact]
        public void FindScopesAsyncTest() {
            var scopeStore = new EntityFrameworkCoreScopeStore(_fixture.DbContext);
            var scopes = scopeStore.FindScopesAsync(new string[] { "system" }).Result;
            Assert.Single(scopes);
            Assert.Equal("system", scopes.Single().Name);
        }

        [Fact]
        public void GetScopesAsyncTest() {
            var scopeStore = new EntityFrameworkCoreScopeStore(_fixture.DbContext);
            {
                var scopes = scopeStore.GetScopesAsync().Result;
                Assert.NotEmpty(scopes);
            }
        }
        [Fact]
        public void GetScopesAsyncTest_publicOnly_false() {
            var scopeStore = new EntityFrameworkCoreScopeStore(_fixture.DbContext);
            {
                var scopes = scopeStore.GetScopesAsync(publicOnly: false).Result;
                Assert.NotEmpty(scopes);
            }

        }
    }
}
