using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Contrib.Store.Redis;
using IdentityServer4.Models;
using IdentityServer4.Services;
using StackExchange.Redis;

namespace RigoFunc.IdentityServer.Services.Redis {
    public class RedisConsentStore : BaseTokenStore<Consent>, IConsentStore {
        private readonly IDatabase _db;
        public RedisConsentStore(IClientStore clientStore, IScopeStore scopeStore, RedisStoreOptions options)
            : this(clientStore, scopeStore, options.Configuration, options.Db) {

        }

        internal RedisConsentStore(IClientStore clientStore, IScopeStore scopeStore, string config, int db = 0) : base(clientStore, scopeStore) {
            var connectionMultiplexer = RedisConnectionMultiplexerStore.GetConnectionMultiplexer(config);
            _db = connectionMultiplexer.GetDatabase(db);
        }
        public Task<IEnumerable<Consent>> LoadAllAsync(string subject) {
            throw new NotImplementedException();
        }

        public async Task RevokeAsync(string subject, string client) {
            string key = $"{subject}_{client}";
            await _db.KeyDeleteAsync(key);
        }

        public async Task<Consent> LoadAsync(string subject, string client) {
            string key = $"{subject}_{client}";
            var json = await _db.StringGetAsync(key);
            var token = FromJson(json);
            return token;
        }

        public async Task UpdateAsync(Consent consent) {
            string key = $"{consent.Subject}_{consent.ClientId}";
            var json = ToJson(consent);
            await _db.StringSetAsync(key, json);
        }
    }
}
