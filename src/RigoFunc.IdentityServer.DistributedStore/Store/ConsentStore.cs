using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace RigoFunc.IdentityServer.DistributedStore {
    public class ConsentStore : IConsentStore {
        private readonly IDistributedCache _cache;
        private readonly IDataSerializer<Consent> _serializer;
        private DistributedCacheEntryOptions _dceo;
        public ConsentStore(IDistributedCache cache, IDataSerializer<Consent> serializer) {
            _cache = cache;
            _serializer = serializer;
            _dceo = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(15));
        }

        public Task<IEnumerable<Consent>> LoadAllAsync(string subject) {
            throw new NotImplementedException();
        }

        public async Task<Consent> LoadAsync(string subject, string client) {
            var key = $"{subject}_{client}";
            var data = await _cache.GetAsync(key);
            if (data == null) {
                return null;
            }
            return _serializer.Deserialize(data);
        }

        public async Task RevokeAsync(string subject, string client) {
            var key = $"{subject}_{client}";
            await _cache.RemoveAsync(key);
        }

        public async Task UpdateAsync(Consent consent) {
            string key = $"{consent.Subject}_{consent.ClientId}";
            var data = _serializer.Serialize(consent);
            await _cache.RemoveAsync(key);

            await _cache.SetAsync(key, data, _dceo);
        }
    }
}
