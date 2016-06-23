using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using RigoFunc.IdentityServer.Api;

namespace RigoFunc.IdentityServer {
    public class IdentityCorsPolicyService : ICorsPolicyService {
        private readonly ILogger<IdentityCorsPolicyService> _logger;

        public IdentityCorsPolicyService(ILogger<IdentityCorsPolicyService> logger) {
            _logger = logger;

            // allow Api route paths
            AllowedOrigins = ApiConstants.RoutePaths;
        }

        /// <summary>
        /// The list allowed origins that are allowed.
        /// </summary>
        /// <value>
        /// The allowed origins.
        /// </value>
        public ICollection<string> AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all origins are allowed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allow all; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAll { get; set; }

        /// <summary>
        /// Determines whether the origin allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public Task<bool> IsOriginAllowedAsync(string origin) {
            if (AllowAll) {
                _logger.LogInformation("AllowAll true, so origin: {0} is allowed", origin);
                return Task.FromResult(true);
            }

            if (AllowedOrigins != null) {
                if (AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)) {
                    _logger.LogInformation("AllowedOrigins configured and origin {0} is allowed", origin);
                    return Task.FromResult(true);
                }
                else {
                    _logger.LogInformation("AllowedOrigins configured and origin {0} is not allowed", origin);
                }
            }

            _logger.LogInformation("Exiting; origin {0} is not allowed", origin);

            return Task.FromResult(false);
        }
    }
}
