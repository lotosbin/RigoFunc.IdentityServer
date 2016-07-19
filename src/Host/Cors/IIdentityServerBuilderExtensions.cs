using System;
using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Hosting.Cors;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Host.Cors {
    public static class IIdentityServerBuilderExtensions {
        public static IIdentityServerBuilder FixCorsIssues(this IIdentityServerBuilder builder, Action<CorsOptions> setupAction, string[] allowedPaths) {
            var services = builder.Services;
            if (setupAction != null) {
                services.Configure(setupAction);
            }

            services.AddTransient<ICorsPolicyService, CorsPolicyService>();

            var paths = new List<string>(Constants.RoutePaths.CorsPaths);

            paths.AddRange(allowedPaths);

            // just for allow more route paths
            services.AddTransient<ICorsPolicyProvider>(provider => {
                return new PolicyProvider(
                    provider.GetRequiredService<ILogger<PolicyProvider>>(),
                    paths,
                    provider.GetRequiredService<ICorsPolicyService>());
            });
            services.AddCors();

            return builder;
        }
    }
}
