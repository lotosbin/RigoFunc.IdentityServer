using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;

namespace RigoFunc.IdentityServer {
    /// <summary>
    /// The implementation of <see cref="IProfileService"/> allows IdentityServer to get identity user and profile store.
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    public class IdentityProfileService<TUser> : IProfileService
        where TUser : class {
        private readonly UserManager<TUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityProfileService{TUser}"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        public IdentityProfileService(UserManager<TUser> userManager) {
            _userManager = userManager;
        }

        /// <summary>
        /// This method is called whenever claims about the user are requested.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the operation.</returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context) {
            if (context.Subject == null)
                throw new ArgumentNullException(nameof(context.Subject));

            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            if (user == null)
                throw new ArgumentException("Invalid subject identifier");

            var claims = await GetClaimsFromUser(user);
            //if (!context.AllClaimsRequested) {
            //    var requestedClaimTypes = context.RequestedClaimTypes;
            //    if (requestedClaimTypes != null)
            //        claims = claims.Where(c => requestedClaimTypes.Contains(c.Type));
            //    else
            //        claims = claims.Take(0);
            //}
            context.IssuedClaims = claims;
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the operation.</returns>
        public async Task IsActiveAsync(IsActiveContext context) {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.Subject == null)
                throw new ArgumentNullException(nameof(context.Subject));

            context.IsActive = false;

            var subject = context.Subject;
            var user = await _userManager.FindByIdAsync(subject.GetSubjectId());
            if (user != null) {
                var security_stamp_changed = false;

                if (_userManager.SupportsUserSecurityStamp) {
                    var security_stamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
                    if (security_stamp != null) {
                        var latest_security_stamp = await _userManager.GetSecurityStampAsync(user);
                        security_stamp_changed = security_stamp != latest_security_stamp;
                    }
                }

                context.IsActive = !security_stamp_changed && !await _userManager.IsLockedOutAsync(user);
            }
        }

        private async Task<IEnumerable<Claim>> GetClaimsFromUser(TUser user) {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, await _userManager.GetUserIdAsync(user)),
                new Claim(JwtClaimTypes.Name, await _userManager.GetUserNameAsync(user))
            };

            if (_userManager.SupportsUserEmail) {
                var email = await _userManager.GetEmailAsync(user);
                if (!string.IsNullOrWhiteSpace(email)) {
                    claims.AddRange(new[]
                    {
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified,
                            await _userManager.IsEmailConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (_userManager.SupportsUserPhoneNumber) {
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (!string.IsNullOrWhiteSpace(phoneNumber)) {
                    claims.AddRange(new[]
                    {
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber),
                        new Claim(JwtClaimTypes.PhoneNumberVerified,
                            await _userManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (_userManager.SupportsUserClaim) {
                claims.AddRange(await _userManager.GetClaimsAsync(user));
            }

            if (_userManager.SupportsUserRole) {
                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
            }

            return claims;
        }
    }
}
