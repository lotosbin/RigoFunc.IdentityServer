using System.Threading.Tasks;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

namespace RigoFunc.IdentityServer {
    public class IdentityResourceOwnerPasswordValidator<TUser> : IResourceOwnerPasswordValidator where TUser : class {
        private readonly UserManager<TUser> _userManager;

        public IdentityResourceOwnerPasswordValidator(UserManager<TUser> userManager) {
            _userManager = userManager;
        }

        public async Task<CustomGrantValidationResult> ValidateAsync(string userName, string password, ValidatedTokenRequest request) {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null && await _userManager.CheckPasswordAsync(user, password)) {
                var userId = await _userManager.GetUserIdAsync(user);
                return new CustomGrantValidationResult(userId, "password");
            }

            return new CustomGrantValidationResult("Invalid user name or password");
        }
    }
}
