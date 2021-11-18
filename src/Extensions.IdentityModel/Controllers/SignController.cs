using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private IUserManager UserManager { get; }
        private ISignInManager SignInManager { get; }
        private IConfigurationRegistry Configurations { get; }
        private IEmailSender EmailSender { get; }
        private IMediator Mediator { get; }

        public SignController(ISignInManager signInManager, IConfigurationRegistry registry, IEmailSender emailSender, IMediator mediator)
        {
            SignInManager = signInManager;
            UserManager = signInManager.UserManager;
            Configurations = registry;
            EmailSender = emailSender;
            Mediator = mediator;
        }


        [HttpGet(Name = "AccountLogin")]
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
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

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
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
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
            await SignInManager.SignOutAsync();
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
            var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
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

            var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await SignInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

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
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }


        [HttpGet]
        [AllowAnonymous]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
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

            var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
            var result = await SignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

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
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
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
            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }


        [HttpGet]
        [AllowAnonymous]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                StatusMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
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
                if (await Configurations.GetBooleanAsync("enable_register") == false)
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
            if (await Configurations.GetBooleanAsync("enable_register") == false)
                return ExternalRegisterClosed();

            var check = new RegisterNotification(model.Username);
            await Mediator.Publish(check);
            if (check.Failed) ModelState.AddModelError("xys::custom_rule", "The username is invalid. Please change another one.");

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(nameof(ExternalLogin), model);
            }

            // Get the information about the user from the external login provider
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Message(
                    title: "External login",
                    message: "Error loading external login information during confirmation.",
                    type: BootstrapColor.danger);
            }

            var user = UserManager.CreateEmpty(model.Username);
            user.Email = model.Email;
            user.RegisterTime = DateTimeOffset.Now;
            var result = await UserManager.CreateAsync(user);
            if (result.Succeeded) result = await UserManager.AddLoginAsync(user, info);

            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false);

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


        [HttpGet(Name = "AccountRegister")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            if (await Configurations.GetBooleanAsync("enable_register") == false)
                return RegisterClosed();

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, string returnUrl = null)
        {
            if (await Configurations.GetBooleanAsync("enable_register") == false)
                return RegisterClosed();

            var check = new RegisterNotification(model.UserName);
            await Mediator.Publish(check);
            if (check.Failed) ModelState.AddModelError("xys::custom_rule", "The username is invalid. Please change another one.");

            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = UserManager.CreateEmpty(model.UserName);
            user.Email = model.Email;
            user.RegisterTime = DateTimeOffset.Now;

            var result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return ErrorView(result, model);

            if (user.Id == 1)
                await UserManager.AddToRoleAsync(user, "Administrator");

            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Sign",
                values: new { userId = $"{user.Id}", code, area = "Account" },
                protocol: Request.Scheme);

            await EmailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

            await SignInManager.SignInAsync(user, isPersistent: false);

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

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null || !user.EmailConfirmed)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "Sign",
                values: new { userId = $"{user.Id}", code, area = "Account" },
                protocol: Request.Scheme);

            await EmailSender.SendEmailAsync(
                email: model.Email,
                subject: "Reset Password",
                message: $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

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
            var user = await UserManager.FindByEmailAsync(model.Email);

            // Don't reveal that the user does not exist
            if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));
            var result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);

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

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null) return View("ConfirmEmailError");

            var result = await UserManager.ConfirmEmailAsync(user, code);
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
                title: "Two-factor authentication",
                message: "Unable to load two-factor authentication user.",
                type: BootstrapColor.danger);

        private ShowMessageResult ExternalRegisterClosed()
            => Message(
                title: "Registration closed",
                message: "The external login doesn't associate any accounts.\n" +
                    "And the registration of current site is closed now.\n" +
                    "If you believe this was a mistake, please contact the site administrator.",
                type: BootstrapColor.secondary);

        private ShowMessageResult RegisterClosed()
            => Message(
                title: "Registration closed",
                message: "The registration of current site is closed now.\n" +
                    "If you believe this was a mistake, please contact the site administrator.",
                type: BootstrapColor.secondary);
    }
}
