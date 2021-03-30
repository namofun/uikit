using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SatelliteSite.IdentityModule.Models;
using SatelliteSite.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Controllers
{
    [Authorize]
    [Area("Account")]
    [Route("[area]/[action]")]
    [AuditPoint(AuditlogType.User)]
    public class SignController : ViewControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ISignInManager _signInManager;
        private readonly IConfigurationRegistry _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<SignController> _localizer;

        public SignController(
            ISignInManager signInManager,
            IConfigurationRegistry registry,
            IEmailSender emailSender,
            IMediator mediator,
            IStringLocalizer<SignController> localizer)
        {
            _signInManager = signInManager;
            _userManager = signInManager.UserManager;
            _configuration = registry;
            _emailSender = emailSender;
            _mediator = mediator;
            _localizer = localizer;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
            }
            else if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localizer["Invalid login attempt."]);
                return View(model);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpGet("/account/login-with-2fa")]
        [AllowAnonymous]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginWith2faModel { RememberMe = rememberMe });
        }


        [HttpPost("/account/login-with-2fa")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localizer["Invalid authenticator code."]);
                return View();
            }
        }


        [HttpGet]
        [AllowAnonymous]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localizer["Invalid recovery code entered."]);
                return View();
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Sign", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }


        [HttpGet]
        [AllowAnonymous]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                StatusMessage = _localizer["Error from external provider: {0}", remoteError];
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                if (await _configuration.GetBooleanAsync("enable_register") == false)
                    return ExternalRegisterClosed();

                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginModel { Email = email });
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel model, string returnUrl = null)
        {
            if (await _configuration.GetBooleanAsync("enable_register") == false)
                return ExternalRegisterClosed();

            var check = new RegisterNotification(model.Username);
            await _mediator.Publish(check);
            if (check.Failed) ModelState.AddModelError("xys::custom_rule", _localizer["The username is invalid. Please change another one."]);

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(nameof(ExternalLogin), model);
            }

            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Message(
                    title: _localizer["External login"],
                    message: _localizer["Error loading external login information during confirmation."],
                    type: BootstrapColor.danger);
            }

            var user = _userManager.CreateEmpty(model.Username);
            user.Email = model.Email;
            user.RegisterTime = DateTimeOffset.Now;
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded) result = await _userManager.AddLoginAsync(user, info);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);

                await HttpContext.AuditAsync(
                    "registered",
                    user.Id.ToString(),
                    $"at {HttpContext.Connection.RemoteIpAddress} via {info.LoginProvider}");

                return RedirectToLocal(returnUrl);
            }
            else
            {
                return ErrorView(result, model, nameof(ExternalLogin));
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            if (await _configuration.GetBooleanAsync("enable_register") == false)
                return RegisterClosed();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, string returnUrl = null)
        {
            if (await _configuration.GetBooleanAsync("enable_register") == false)
                return RegisterClosed();

            var check = new RegisterNotification(model.UserName);
            await _mediator.Publish(check);
            if (check.Failed) ModelState.AddModelError("xys::custom_rule", _localizer["The username is invalid. Please change another one."]);

            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = _userManager.CreateEmpty(model.UserName);
            user.Email = model.Email;
            user.RegisterTime = DateTimeOffset.Now;

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return ErrorView(result, model);

            if (user.Id == 1)
                await _userManager.AddToRoleAsync(user, "Administrator");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Sign",
                values: new { userId = $"{user.Id}", code, area = "Account" },
                protocol: Request.Scheme);

            await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

            await _signInManager.SignInAsync(user, isPersistent: false);

            await HttpContext.AuditAsync(
                "registered",
                user.Id.ToString(),
                $"at {HttpContext.Connection.RemoteIpAddress}");

            return RedirectToLocal(returnUrl);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.EmailConfirmed)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "Sign",
                values: new { userId = $"{user.Id}", code, area = "Account" },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                email: model.Email,
                subject: _localizer["Reset Password"],
                message: _localizer["Please reset your password by clicking here: <a href='{0}'>link</a>", callbackUrl]);

            await HttpContext.AuditAsync(
                "sent forgot password email",
                user.Id.ToString(),
                $"at {HttpContext.Connection.RemoteIpAddress}");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null) return NotFound();
            var model = new ResetPasswordModel { Code = code };
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Don't reveal that the user does not exist
            if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (!result.Succeeded) return ErrorView(result, model);

            await HttpContext.AuditAsync(
                "reset password",
                user.Id.ToString(),
                $"at {HttpContext.Connection.RemoteIpAddress}");

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(int userId, string code)
        {
            if (code == null) return View("ConfirmEmailError");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View("ConfirmEmailError");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "ConfirmEmailError");
        }


        private ViewResult ErrorView(IdentityResult result, object model, string viewName = null)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return viewName == null ? View(model) : View(viewName, model);
        }

        private RedirectResult RedirectToLocal(string returnUrl)
            => Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");

        private ShowMessageResult TwoFactorFail()
            => Message(
                title: _localizer["Two-factor authentication"],
                message: _localizer["Unable to load two-factor authentication user."],
                type: BootstrapColor.danger);

        private ShowMessageResult ExternalRegisterClosed()
            => Message(
                title: _localizer["Registration closed"],
                message: _localizer[
                    "The external login doesn't associate any accounts. " +
                    "And the registration of current site is closed now. " +
                    "If you believe this was a mistake, please contact the site administrator."],
                type: BootstrapColor.secondary);

        private ShowMessageResult RegisterClosed()
            => Message(
                title: _localizer["Registration closed"],
                message: _localizer[
                    "The registration of current site is closed now. " +
                    "If you believe this was a mistake, please contact the site administrator."],
                type: BootstrapColor.secondary);
    }
}
