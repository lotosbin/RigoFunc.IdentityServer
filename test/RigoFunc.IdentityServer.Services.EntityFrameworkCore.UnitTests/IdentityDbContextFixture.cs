using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore.UnitTests {
    public class IdentityDbContextFixture : IDisposable {
        public IdentityServerDbContext DbContext { get; set; }
        public IdentityDbContextFixture() {
            // ... initialize data in the test database ...
            var services = new ServiceCollection();
            //var connectionString = @"Server=(local);Database=identityserver;Trusted_Connection=True;";
            var connectionString = @"Server=192.168.1.13;Database=identityserver;User ID=sa;Password=Welcome123;";
            services.AddIdentityServerEntityFrameworkCoreServices(options => options.UseSqlServer(connectionString));
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<IdentityServerDbContext>();
            this.DbContext = dbContext;
            //dbContext.Database.Migrate();
            var ensureCreated = dbContext.Database.EnsureCreated();
            SeedData(dbContext);
        }

        private void SeedData(IdentityServerDbContext dbContext) {
            if (!dbContext.Scopes.Any(e => e.Name == "system")) {
                dbContext.Scopes.Add(new ScopeEntity("system"));
            }
            if (!dbContext.Clients.Any(e => e.ClientId == "testclient")) {
                dbContext.Clients.Add(new ClientEntity("testclient"));
            }
            dbContext.SaveChanges();
        }

        public void Dispose() {
            // ... clean up test data from the database ...
        }

    }
}
