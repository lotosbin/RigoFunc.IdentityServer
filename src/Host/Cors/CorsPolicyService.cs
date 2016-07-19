using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Host.Cors {
    public class CorsPolicyService : ICorsPolicyService {
        private readonly ILogger<CorsPolicyService> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="CorsPolicyService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CorsPolicyService(ILogger<CorsPolicyService> logger, IOptions<CorsOptions> options) {
            _logger = logger;

            var opt = options.Value;
            AllowAnyOrigin = opt.AllowAnyOrigin;
            if (opt.AllowedOrigins != null) {
                AllowedOrigins = new List<string>(opt.AllowedOrigins);
            }
        }

        /// <summary>
        /// The list allowed origins that are allowed.
        /// </summary>
        /// <value>
        /// The allowed origins.
        /// </value>
        public ICollection<string> AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether allows any origin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allows any origin; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAnyOrigin { get; set; }

        /// <summary>
        /// Determines whether the origin allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public Task<bool> IsOriginAllowedAsync(string origin) {
            if (AllowAnyOrigin) {
                _logger.LogInformation("AllowAnyOrigin true, so origin: {0} is allowed", origin);
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
