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
    public class IdentityServerSignInManager<TUser> : SignInManager<TUser> where TUser : class {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IdentityOptions _options;
        private HttpContext _context;

        public IdentityServerSignInManager(UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<TUser>> logger) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger) {
            _contextAccessor = contextAccessor;
            _options = optionsAccessor.Value;
        }

        internal HttpContext Context {
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

            await Context.Authentication.SignInAsync(_options.Cookies.ApplicationCookieAuthenticationScheme,
                userPrincipal,
                authenticationProperties ?? new AuthenticationProperties());
        }
    }
}
