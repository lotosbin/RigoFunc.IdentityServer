using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore {
    public class EntityFrameworkCoreClientStore : IClientStore {
        private readonly IdentityServerDbContext _dbContext;

        public EntityFrameworkCoreClientStore(IdentityServerDbContext dbContext) {
            _dbContext = dbContext;
        }

        public Task<Client> FindClientByIdAsync(string clientId) {
            return Task.FromResult((Client)_dbContext.Clients.SingleOrDefault(e => e.ClientId == clientId && e.Enabled));
        }
    }
}