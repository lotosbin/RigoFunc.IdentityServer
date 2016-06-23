using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RigoFunc.ApiCore.Services;
using Host.EntityFrameworkCore;

namespace Host.UI.Register {
    public class RegisterController : Controller {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILogger<RegisterController> logger) {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = logger;
        }

        [HttpGet("ui/register", Name = "Register")]
        [AllowAnonymous]
        public IActionResult Index(string returnUrl = null) {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("ui/register")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterModel model, string returnUrl = null) {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid) {
                var user = new AppUser { UserName = model.UserName ?? model.PhoneNumber, PhoneNumber = model.PhoneNumber };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded) {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                    //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void AddErrors(IdentityResult result) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }
            else {
                return Redirect( "~/");
            }
        }
    }
}
