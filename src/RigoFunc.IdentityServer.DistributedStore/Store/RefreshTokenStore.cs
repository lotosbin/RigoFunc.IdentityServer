using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace RigoFunc.IdentityServer.DistributedStore {
    public class RefreshTokenStore : IRefreshTokenStore {
        private readonly IDistributedCache _cache;
        private readonly IDataSerializer<RefreshToken> _serializer;
        private DistributedCacheEntryOptions _dceo;

        public RefreshTokenStore(IDistributedCache cache, IDataSerializer<RefreshToken> serializer) {
            _cache = cache;
            _serializer = serializer;
            _dceo = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(15));
        }

        public async Task<RefreshToken> GetAsync(string key) {
            var data = await _cache.GetAsync(key);
            if (data == null) {
                return null;
            }
            return _serializer.Deserialize(data);
        }

        public async Task RemoveAsync(string key) {
            await _cache.RemoveAsync(key);
        }

        public async Task StoreAsync(string key, RefreshToken value) {
            var data = _serializer.Serialize(value);

            await _cache.SetAsync(key, data, _dceo);
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject) {
            throw new NotImplementedException();
        }

        public Task RevokeAsync(string subject, string client) {
            throw new NotImplementedException();
        }
    }
}
