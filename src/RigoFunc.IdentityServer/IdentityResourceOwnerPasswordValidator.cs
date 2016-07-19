using System.Threading.Tasks;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

namespace RigoFunc.IdentityServer {
    /// <summary>
    /// Handles validation of resource owner password credentials
    /// </summary>
    /// <typeparam name="TUser">The type of the t user.</typeparam>
    public class IdentityResourceOwnerPasswordValidator<TUser> : IResourceOwnerPasswordValidator where TUser : class {
        private readonly UserManager<TUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResourceOwnerPasswordValidator{TUser}"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        public IdentityResourceOwnerPasswordValidator(UserManager<TUser> userManager) {
            _userManager = userManager;
        }

        /// <summary>
        /// Validates the resource owner password credential
        /// </summary>
        /// <param name="userName">The user name</param>
        /// <param name="secret">The password or code</param>
        /// <param name="request">The validated token request.</param>
        /// <returns>The validation result</returns>
        public async Task<CustomGrantValidationResult> ValidateAsync(string userName, string secret, ValidatedTokenRequest request) {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null) {
                var userId = await _userManager.GetUserIdAsync(user);
                if(await _userManager.CheckPasswordAsync(user, secret)) {
                    return new CustomGrantValidationResult(userId, "password");
                }
                else {
                    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                    if(await _userManager.VerifyChangePhoneNumberTokenAsync(user, secret, phoneNumber)) {
                        return new CustomGrantValidationResult(userId, "code");
                    }
                }
            }

            return new CustomGrantValidationResult("Invalid user name or password");
        }
    }
}
