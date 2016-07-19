using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RigoFunc.IdentityServer {
    /// <summary>
    /// Class IdentityServerSignInManager.{CC2D43FA-BBC4-448A-9D0B-7B57ADF2655}
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    public class IdentityServerSignInManager<TUser> : SignInManager<TUser> where TUser : class {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IdentityOptions _options;
        private HttpContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityServerSignInManager{TUser}" /> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="contextAccessor">The context accessor.</param>
        /// <param name="claimsFactory">The claims factory.</param>
        /// <param name="optionsAccessor">The options accessor.</param>
        /// <param name="logger">The logger.</param>
        public IdentityServerSignInManager(UserManager<TUser> userManager,
                    IHttpContextAccessor contextAccessor,
                    IUserClaimsPrincipalFactory<TUser> claimsFactory,
                    IOptions<IdentityOptions> optionsAccessor,
                    ILogger<SignInManager<TUser>> logger) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger) {
            _contextAccessor = contextAccessor;
            _options = optionsAccessor.Value;
        }

        /// <summary>
        /// Gets or sets the HTTP context.
        /// </summary>
        /// <value>The HTTP context.</value>
        internal HttpContext HttpContext {
            get {
                var context = _context ?? _contextAccessor?.HttpContext;
                if (context == null) {
                    throw new InvalidOperationException("HttpContext must not be null.");
                }
                return context;
            }
            set {
                _context = value;
            }
        }

        /// <summary>
        /// Signs in the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to sign-in.</param>
        /// <param name="authenticationProperties">Properties applied to the login and authentication cookie.</param>
        /// <param name="authenticationMethod">Name of the method used to authenticate the user.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async override Task SignInAsync(TUser user, AuthenticationProperties authenticationProperties, string authenticationMethod = null) {
            var userPrincipal = await CreateUserPrincipalAsync(user);

            // Review: should we guard against CreateUserPrincipal returning null?
            userPrincipal.Identities.First().AddClaims(new[]
            {
                new Claim(JwtClaimTypes.IdentityProvider, authenticationMethod ?? Constants.BuiltInIdentityProvider),
                new Claim(JwtClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString())
            });

            if (authenticationMethod != null) {
                userPrincipal.Identities.First().AddClaim(new Claim(ClaimTypes.AuthenticationMethod, authenticationMethod));
            }

            await HttpContext.Authentication.SignInAsync(_options.Cookies.ApplicationCookieAuthenticationScheme,
                userPrincipal,
                authenticationProperties ?? new AuthenticationProperties());
        }
    }
}
