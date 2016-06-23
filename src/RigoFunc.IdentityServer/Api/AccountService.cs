using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RigoFunc.ApiCore.Services;
using RigoFunc.OAuth;
using RigoFunc.Utils;

namespace RigoFunc.IdentityServer.Api {
    /// <summary>
    /// Represents the default implementation of the <see cref="IAccountService"/> interface.
    /// </summary>
    public class AccountService<TUser, TKey> : IAccountService
        where TUser : IdentityUser<TKey>, new() where TKey : IEquatable<TKey> {
        private readonly UserManager<TUser> _userManager;
        private readonly SignInManager<TUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly HttpContext _httpContext;
        private readonly AccountApiOptions _options;
        private const string DefaultSecurityStamp = "022a9e42-9509-4aa6-8a0a-c34a1f405c61";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService{TUser, TKey}"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="emailSender">The email sender.</param>
        /// <param name="smsSender">The SMS sender.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="contextAccessor">The context accessor.</param>
        /// <param name="options">The options.</param>
        public AccountService(UserManager<TUser> userManager,
            SignInManager<TUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor contextAccessor,
            IOptions<AccountApiOptions> options) {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger("AccountService"); ;
            _httpContext = contextAccessor.HttpContext;
        }

        /// <summary>
        /// Changes the password for the specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the change operation.</returns>
        public async Task<bool> ChangePasswordAsync(ChangePasswordModel model) {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null) {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");

                    return true;
                }

                _logger.LogError(result.ToString());
            }

            return false;
        }

        /// <summary>
        /// Logins with the specified model asynchronous.
        /// </summary>
        /// <param name="model">The login model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the login operation.</returns>
        public async Task<IResponse> LoginAsync(LoginInputModel model) {
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded) {
                _logger.LogInformation(1, "User logged in.");
                return await RequestTokenAsync(model.UserName, model.Password);
            }

            _logger.LogError(result.ToString());

            throw new ArgumentException(string.Format(Resources.PhoneNumberOrPasswordError, model.UserName, model.Password));
        }

        /// <summary>
        /// Registers a new user asynchronous.
        /// </summary>
        /// <param name="model">The register model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the register operation. Task result contains the register response.</returns>
        public async Task<IResponse> RegisterAsync(RegisterInputModel model) {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user != null) {
                throw new ArgumentException(string.Format(Resources.PhoneNumberHadBeenRegister, model.PhoneNumber));
            }

            if (!ValidateCode(model.Code, model.PhoneNumber)) {
                throw new ArgumentException(string.Format(Resources.VerifyCodeFailed, model.Code));
            }

            user = new TUser { UserName = model.UserName ?? model.PhoneNumber, PhoneNumber = model.PhoneNumber };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);

                return await RequestTokenAsync(user.UserName, model.Password);
            }

            _logger.LogError(result.ToString());

            throw new ArgumentException(string.Format(Resources.RegisterNewUserFailed, model.PhoneNumber, model.Code));
        }

        /// <summary>
        /// Resets the password for specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the reset operation.</returns>
        public async Task<IResponse> ResetPasswordAsync(ResetPasswordModel model) {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null) {
                throw new ArgumentNullException(string.Format(Resources.NotFoundUserByPhoneNumber, model.PhoneNumber));
            }

            if (!await _userManager.VerifyChangePhoneNumberTokenAsync(user, model.Code, model.PhoneNumber)) {
                throw new ArgumentException(string.Format(Resources.VerifyCodeFailed, model.Code));
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);

                return await RequestTokenAsync(user.UserName, model.Password);
            }

            _logger.LogError(result.ToString());

            throw new ArgumentNullException(string.Format(Resources.PasswordResetFailed, model.PhoneNumber));
        }

        /// <summary>
        /// Sends the specified code asynchronous.
        /// </summary>
        /// <param name="model">The send code model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the send operation.</returns>
        public async Task<bool> SendCodeAsync(SendCodeInputModel model) {
            string code;
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null) {
                code = GenerateCode(model.PhoneNumber);
            }
            else {
                code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            }

            SendSmsResult result;
            if (string.IsNullOrWhiteSpace(_options.SendCodeTemplate)) {
                result = await _smsSender.SendSmsAsync(model.PhoneNumber, code);
            }
            else {
                result = await _smsSender.SendSmsAsync(_options.SendCodeTemplate, model.PhoneNumber, Tuple.Create("code", code));
            }

            if (!result.IsSuccessSend) {
                throw new ArgumentException(string.Format(Resources.SendCodeFailed, model.PhoneNumber, result.ErrorMessage));
            }

            return true;
        }

        /// <summary>
        /// Updates the specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the reset operation.</returns>
        public async Task<bool> UpdateAsync(OAuthUser model) {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null) {
                throw new ArgumentException(string.Format(Resources.NotFoundUserById, model.Id));
            }

            var result = await _userManager.AddClaimsAsync(user, model.ToClaims());
            if (result.Succeeded) {
                return true;
            }

            _logger.LogError(result.ToString());

            throw new ArgumentNullException(Resources.UpdateUserFailed);
        }

        /// <summary>
        /// Verifies the specified code asynchronous.
        /// </summary>
        /// <param name="model">The verify code model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the verify operation.</returns>
        public async Task<IResponse> VerifyCodeAsync(VerifyCodeInputModel model) {
            var password = $"{GenericUtil.UniqueKey()}@520";
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null) {
                if (!ValidateCode(model.Code, model.PhoneNumber)) {
                    throw new ArgumentException(string.Format(Resources.VerifyCodeFailed, model.Code));
                }

                user = new TUser { UserName = model.PhoneNumber, PhoneNumber = model.PhoneNumber };
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    SendSmsResult smsResult;
                    if (string.IsNullOrWhiteSpace(_options.SendPasswordTemplate)) {
                        smsResult = await _smsSender.SendSmsAsync(model.PhoneNumber, password);
                    }
                    else {
                        smsResult = await _smsSender.SendSmsAsync(_options.SendPasswordTemplate, model.PhoneNumber, Tuple.Create("password", password));
                    }
                    if (!smsResult.IsSuccessSend) {
                        throw new ArgumentException(string.Format(Resources.SendCodeFailed, model.PhoneNumber, smsResult.ErrorMessage));
                    }

                    _logger.LogInformation(3, "User changed their password successfully.");

                    return await RequestTokenAsync(user.UserName, password);
                }

                _logger.LogError(result.ToString());

                throw new ArgumentException(string.Format(Resources.RegisterNewUserFailed, model.PhoneNumber, model.Code));
            }
            else {
                if (!await _userManager.VerifyChangePhoneNumberTokenAsync(user, model.Code, model.PhoneNumber)) {
                    throw new ArgumentException($"cannot verify the code: {model.Code} for the phone: {model.PhoneNumber}");
                }

                // TODO: bypass the password login
                // because we doesn't known the password.
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, code, password);
                if (result.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return await RequestTokenAsync(user.UserName, password);
                }

                _logger.LogError(result.ToString());

                throw new ArgumentNullException(string.Format(Resources.LoginFailedWithCodeAndPhone, model.Code, model.PhoneNumber));
            }
        }

        private async Task<IResponse> RequestTokenAsync(string userName, string password) {
            var endpoint = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host.Value}/connect/token";

            _logger.LogInformation($"token_endpoint: {endpoint}");

            string clientId = _httpContext.Request.Headers[ApiConstants.ClientId];
            if (string.IsNullOrWhiteSpace(clientId)) {
                clientId = _options.DefaultClientId;
            }
            string clientSecret = _httpContext.Request.Headers[ApiConstants.ClientSecret];
            if (string.IsNullOrWhiteSpace(clientSecret)) {
                clientSecret = _options.DefaultClientSecret;
            }
            string scope = _httpContext.Request.Headers[ApiConstants.Scope];
            if (string.IsNullOrWhiteSpace(scope)) {
                scope = _options.DefaultScope;
            }

            var client = new TokenClient(endpoint, clientId, clientSecret);
            var response = await client.RequestResourceOwnerPasswordAsync(userName, password, scope);

            return ApiResponse.FromTokenResponse(response);
        }

        private string GenerateCode(string phoneNumber) {
            var securityStamp = Encoding.Unicode.GetBytes(DefaultSecurityStamp);
            return Rfc6238Service.GenerateCode(securityStamp, phoneNumber).ToString(CultureInfo.InvariantCulture);
        }

        private bool ValidateCode(string token, string phoneNumber) {
            var securityStamp = Encoding.Unicode.GetBytes(DefaultSecurityStamp);
            int code;
            if (securityStamp != null && int.TryParse(token, out code)) {
                if (Rfc6238Service.ValidateCode(securityStamp, code, phoneNumber)) {
                    return true;
                }
            }
            _logger.LogWarning(8, $"ValidateCode() failed for phone {phoneNumber}.");
            return false;
        }
    }
}
