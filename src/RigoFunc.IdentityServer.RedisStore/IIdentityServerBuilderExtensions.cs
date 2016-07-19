using System;
using IdentityServer4.Services;
using Microsoft.Extensions.DependencyInjection;
using RigoFunc.IdentityServer.Services.Redis;

namespace RigoFunc.IdentityServer {
    /// <summary>
    /// Contains extension methods to <see cref="IIdentityServerBuilder"/> for configuring identity server.
    /// </summary>
    public static class IIdentityServerBuilderExtensions {
        public static IIdentityServerBuilder AddRedisTransientStores(this IIdentityServerBuilder builder, Action<RedisStoreOptions> options = null) {
            var redisStoreOptions = new RedisStoreOptions();
            options?.Invoke(redisStoreOptions);

            var services = builder.Services;

            services.AddSingleton(redisStoreOptions);
            services.AddSingleton<IAuthorizationCodeStore, RedisAuthorizationCodeStore>();
            services.AddSingleton<IRefreshTokenStore, RedisRefreshTokenStore>();
            services.AddSingleton<ITokenHandleStore, RedisTokenHandleStore>();
            services.AddSingleton<IConsentStore, RedisConsentStore>();

            return builder;
        }
    }
}
