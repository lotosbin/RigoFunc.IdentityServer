using System;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RigoFunc.IdentityServer.Services.EntityFrameworkCore;

namespace RigoFunc.IdentityServer {
    public static class IdentityServerServiceCollectionExtensions {
        public static void AddIdentityServerEntityFrameworkCoreServices(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction = null) {
            services.AddDbContext<IdentityServerDbContext>(optionsAction);
            services.AddTransient<IScopeStore, EntityFrameworkCoreScopeStore>();
            services.AddTransient<IClientStore, EntityFrameworkCoreClientStore>();
        }

    }
}
