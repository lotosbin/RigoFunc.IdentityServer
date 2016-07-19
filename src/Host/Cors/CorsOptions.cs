using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Host.Cors {
    public class CorsOptions {
        public static readonly string[] RoutePaths = new string[] {
            // can this by reflection rather than hard code?
            "api/account/register",
            "api/account/sendcode",
            "api/account/login",
            "api/account/verifycode",
            "api/account/changepassword",
            "api/account/resetpassword",
            "api/account/update",
            "api/weixin/bind",
            "api/weixin/login"
        };

        /// <summary>
        /// Gets or sets the allowed paths.
        /// </summary>
        /// <value>The allowed paths.</value>
        public string [] AllowedPaths { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether allows any origin.
        /// </summary>
        /// <value>
        ///   <c>true</c> if allows any origin; otherwise, <c>false</c>.
        /// </value>
        public bool AllowAnyOrigin { get; set; }
        /// <summary>
        /// Gets or sets the allowed origins.
        /// </summary>
        /// <value>The allowed origins.</value>
        public string[] AllowedOrigins { get; set; }
    }
}
