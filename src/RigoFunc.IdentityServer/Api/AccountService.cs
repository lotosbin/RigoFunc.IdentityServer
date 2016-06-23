using System;
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

            throw new InvalidOperationException("User login failed");
        }

        /// <summary>
        /// Registers a new user asynchronous.
        /// </summary>
        /// <param name="model">The register model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the register operation. Task result contains the register response.</returns>
        public async Task<IResponse> RegisterAsync(RegisterInputModel model) {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user != null) {
                throw new ArgumentException($"The user: {model.PhoneNumber} had been register.");
            }

            user = await _userManager.FindByIdAsync("1");
            if (user == null || !await _userManager.VerifyChangePhoneNumberTokenAsync(user, model.Code, model.PhoneNumber)) {
                throw new ArgumentException($"cannot verify the code: {model.Code} for the phone: {model.PhoneNumber}");
            }

            user = new TUser { UserName = model.UserName ?? model.PhoneNumber, PhoneNumber = model.PhoneNumber };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);

                return await RequestTokenAsync(user.UserName, model.Password);
            }

            _logger.LogError(result.ToString());

            throw new ArgumentException($"cannot to register new user for phone: {model.PhoneNumber} code: {model.Code}");
        }

        /// <summary>
        /// Resets the password for specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the reset operation.</returns>
        public async Task<IResponse> ResetPasswordAsync(ResetPasswordModel model) {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null) {
                throw new ArgumentNullException($"cannot reset the password for user: {model.PhoneNumber}");
            }

            if (!await _userManager.VerifyChangePhoneNumberTokenAsync(user, model.Code, model.PhoneNumber)) {
                throw new ArgumentException($"The code: {model.Code} is invalide or timeout with 3 minutes.");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
            if (result.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);

                return await RequestTokenAsync(user.UserName, model.Password);
            }

            _logger.LogError(result.ToString());

            throw new ArgumentNullException($"cannot reset the password for user: {model.PhoneNumber}");
        }

        /// <summary>
        /// Sends the specified code asynchronous.
        /// </summary>
        /// <param name="model">The send code model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the send operation.</returns>
        public async Task<bool> SendCodeAsync(SendCodeInputModel model) {
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null) {
                user = await _userManager.FindByIdAsync("1");
            }

            if (user != null) {
                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
                await _smsSender.SendSmsAsync(model.PhoneNumber, code);
                return true;
            }
            else {
                throw new ArgumentException($"cannot send code for the phone: {model.PhoneNumber}");
            }
        }

        /// <summary>
        /// Updates the specified user asynchronous.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the reset operation.</returns>
        public async Task<bool> UpdateAsync(OAuthUser model) {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null) {
                throw new ArgumentException($"cannot found the user: {model.Id}");
            }

            var result = await _userManager.AddClaimsAsync(user, model.ToClaims());
            if (result.Succeeded) {
                return true;
            }

            _logger.LogError(result.ToString());

            throw new ArgumentNullException("Update User failed");
        }

        /// <summary>
        /// Verifies the specified code asynchronous.
        /// </summary>
        /// <param name="model">The veriry code model.</param>
        /// <returns>A <see cref="Task{TResult}"/> represents the verify operation.</returns>
        public async Task<IResponse> VerifyCodeAsync(VerifyCodeInputModel model) {
            var password = $"{GenericUtil.UniqueKey()}@520";
            var user = await _userManager.FindByNameAsync(model.PhoneNumber);
            if (user == null) {
                user = await _userManager.FindByIdAsync("1");
                if (user == null || !await _userManager.VerifyChangePhoneNumberTokenAsync(user, model.Code, model.PhoneNumber)) {
                    throw new ArgumentException($"cannot verify the code: {model.Code} for the phone: {model.PhoneNumber}");
                }

                user = new TUser { UserName = model.PhoneNumber, PhoneNumber = model.PhoneNumber };
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    await _smsSender.SendSmsAsync(model.PhoneNumber, password);

                    _logger.LogInformation(3, "User changed their password successfully.");

                    return await RequestTokenAsync(user.UserName, password);
                }

                _logger.LogError(result.ToString());

                throw new ArgumentException($"cannot to register new user for phone: {model.PhoneNumber} code: {model.Code}");
            }
            else {
                if (!await _userManager.VerifyChangePhoneNumberTokenAsync(user, model.Code, model.PhoneNumber)) {
                    throw new ArgumentException($"cannot verify the code: {model.Code} for the phone: {model.PhoneNumber}");
                }

                // because we doesn't known the password.
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, code, password);
                if (result.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return await RequestTokenAsync(user.UserName, password);
                }

                _logger.LogError(result.ToString());

                throw new ArgumentNullException($"cannot login use code: {model.Code} for the user: {model.PhoneNumber}");
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
    }
}
