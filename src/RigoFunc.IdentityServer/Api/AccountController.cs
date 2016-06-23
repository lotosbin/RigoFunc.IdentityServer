using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RigoFunc.OAuth;

namespace RigoFunc.IdentityServer.Api {
    [Route("api/[controller]")]
    public class AccountController {
        private readonly IAccountService _service;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService service, ILogger<AccountController> logger) {
            _service = service;
            _logger = logger;
        }

        [HttpPost("[action]")]
        public async Task<IResponse> Register([FromBody]RegisterInputModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }

            return await _service.RegisterAsync(model);
        }

        [HttpPost("[action]")]
        public async Task<bool> SendCode([FromBody]SendCodeInputModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }

            return await _service.SendCodeAsync(model);
        }

        [HttpPost("[action]")]
        public async Task<IResponse> Login([FromBody]LoginInputModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }

            return await _service.LoginAsync(model);
        }

        [HttpPost("[action]")]
        public async Task<IResponse> VerifyCode([FromBody]VerifyCodeInputModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }

            return await _service.VerifyCodeAsync(model);
        }

        [HttpPost("[action]")]
        public async Task<bool> ChangePassword([FromBody]ChangePasswordModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }

            return await _service.ChangePasswordAsync(model);
        }

        [HttpPost("[action]")]
        public async Task<IResponse> ResetPassword([FromBody]ResetPasswordModel model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }

            return await _service.ResetPasswordAsync(model);
        }

        [HttpPost("[action]")]
        public async Task<bool> Update([FromBody]OAuthUser model) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }

            return await _service.UpdateAsync(model);
        }
    }
}
