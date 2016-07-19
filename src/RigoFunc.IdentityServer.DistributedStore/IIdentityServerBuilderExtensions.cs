using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RigoFunc.IdentityServer.DistributedStore;

namespace Microsoft.Extensions.DependencyInjection {
    /// <summary>
    /// Contains extension methods to <see cref="IIdentityServerBuilder"/> for configuring identity server.
    /// </summary>
    public static class IIdentityServerBuilderExtensions {
        public static IIdentityServerBuilder AddDistributedStores(this IIdentityServerBuilder builder) {
            var services = builder.Services;

            services.TryAddSingleton<IDataSerializer<Token>, TokenSerializer>();
            services.TryAddSingleton<IDataSerializer<Consent>, ConsentSerializer>();
            services.TryAddSingleton<IDataSerializer<RefreshToken>, RefreshTokenSerializer>();
            services.TryAddSingleton<IDataSerializer<AuthorizationCode>, AuthorizationCodeSerializer>();

            services.AddSingleton<IAuthorizationCodeStore, AuthorizationCodeStore>();
            services.AddSingleton<IRefreshTokenStore, RefreshTokenStore>();
            services.AddSingleton<ITokenHandleStore, TokenHandleStore>();
            services.AddSingleton<IConsentStore, ConsentStore>();

            return builder;
        }
    }
}
