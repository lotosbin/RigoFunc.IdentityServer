using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore {
    public class IdentityServerDbContext : DbContext {
        public IdentityServerDbContext(DbContextOptions<IdentityServerDbContext> options) : base(options) {

        }
        public DbSet<ClientEntity> Clients { get; set; }
        public DbSet<ScopeEntity> Scopes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }
        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            //client
            var client = builder.Entity<ClientEntity>();
            client.ToTable("Clients").HasKey(e => e.ClientId);
            client.Ignore(e => e.Claims);
            client.Ignore(e => e.AllowedCorsOrigins);
            client.Ignore(e => e.AllowedGrantTypes);
            client.Ignore(e => e.AllowedScopes);
            client.Ignore(e => e.ClientSecrets);
            client.Ignore(e => e.IdentityProviderRestrictions);
            client.Ignore(e => e.PostLogoutRedirectUris);
            client.Ignore(e => e.RedirectUris);

            //scope
            var scope = builder.Entity<ScopeEntity>();
            scope.ToTable("Scopes").HasKey(e => e.Name);
            scope.Ignore(e => e.Claims);
            scope.Ignore(e => e.ScopeSecrets);
        }
    }
}
