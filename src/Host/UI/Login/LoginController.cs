using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RigoFunc.ApiCore.Services;
using Host.EntityFrameworkCore;

namespace Host.UI.Login {
    public class LoginController : Controller {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<LoginController> _logger;
        private readonly SignInInteraction _signInInteraction;

        public LoginController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILogger<LoginController> logger,
            SignInInteraction signInInteraction) {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = logger;
            _signInInteraction = signInInteraction;
        }

        [HttpGet(Constants.RoutePaths.Login, Name = "Login")]
        public async Task<IActionResult> Index(string id) {
            var vm = new LoginViewModel();

            if (id != null) {
                var request = await _signInInteraction.GetRequestAsync(id);
                if (request != null) {
                    vm.UserName = request.LoginHint;
                    vm.SignInId = id;
                }
            }

            return View(vm);
        }

        [HttpPost(Constants.RoutePaths.Login)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginInputModel model) {
            if (ModelState.IsValid) {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded) {
                    _logger.LogInformation(1, "User logged in.");
                    if (model.SignInId != null) {
                        return new SignInResult(model.SignInId);
                    }

                    return Redirect("~/");
                }
                if (result.RequiresTwoFactor) {
                    //return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut) {
                    _logger.LogWarning(2, "User account locked out.");
                    return View("Lockout");
                }
                else {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            var vm = new LoginViewModel(model);
            return View(vm);
        }
    }
}
