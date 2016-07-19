using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Contrib.Store.Redis;
using IdentityServer4.Models;
using IdentityServer4.Services;
using StackExchange.Redis;

namespace RigoFunc.IdentityServer.Services.Redis {
    public class RedisAuthorizationCodeStore : BaseTokenStore<AuthorizationCode>, IAuthorizationCodeStore {
        private readonly IDatabase _db;

        public RedisAuthorizationCodeStore(IClientStore clientStore, IScopeStore scopeStore, RedisStoreOptions options)
            : this(clientStore, scopeStore, options.Configuration, options.Db) {

        }

        internal RedisAuthorizationCodeStore(IClientStore clientStore, IScopeStore scopeStore, string config, int db = 0) : base(clientStore, scopeStore) {
            var connectionMultiplexer = RedisConnectionMultiplexerStore.GetConnectionMultiplexer(config);
            _db = connectionMultiplexer.GetDatabase(db);
        }
        public async Task StoreAsync(string key, AuthorizationCode value) {
            var json = ToJson(value);
            await _db.StringSetAsync(key, json);
        }

        public async Task<AuthorizationCode> GetAsync(string key) {
            var json = await _db.StringGetAsync(key);
            return FromJson(json);
        }

        public async Task RemoveAsync(string key) {
            await _db.KeyDeleteAsync(key);
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject) {
            throw new NotImplementedException();
        }

        public Task RevokeAsync(string subject, string client) {
            throw new NotImplementedException();
        }
    }
}
