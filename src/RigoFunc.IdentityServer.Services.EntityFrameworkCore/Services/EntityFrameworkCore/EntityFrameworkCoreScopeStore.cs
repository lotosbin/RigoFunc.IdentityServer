using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace RigoFunc.IdentityServer.Services.EntityFrameworkCore {
    public class EntityFrameworkCoreScopeStore : IScopeStore {
        private readonly IdentityServerDbContext _dbContext;

        public EntityFrameworkCoreScopeStore(IdentityServerDbContext dbContext) {
            _dbContext = dbContext;
        }

        public Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames) {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var scopes = _dbContext.Scopes.Where(s => scopeNames.ToList().Contains(s.Name));

            return Task.FromResult<IEnumerable<Scope>>(scopes.ToList());
        }

        public Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true) {
            var expression = publicOnly
                ? (Expression<Func<ScopeEntity, bool>>)(s => s.ShowInDiscoveryDocument)
                : (s => true);
            IEnumerable<Scope> scopes = _dbContext.Scopes.Where(expression).AsEnumerable();
            return Task.FromResult(scopes);
        }
    }
}
