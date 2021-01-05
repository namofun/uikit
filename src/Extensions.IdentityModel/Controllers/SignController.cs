using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.IdentityModule.Models;
using SatelliteSite.IdentityModule.Services;
using SatelliteSite.Services;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Controllers
{
    [Authorize]
    [Area("Account")]
    [Route("[area]/[action]")]
    public class SignController : ViewControllerBase
    {
        private IUserManager UserManager { get; }

        public SignController(IUserManager userMgr)
        {
            UserManager = userMgr;
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
        public async Task<IActionResult> Login(
            LoginModel model,
            [FromServices] ISignInManager signInManager,
            [FromQuery] string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded) return RedirectToLocal(returnUrl);
            if (result.IsLockedOut) return RedirectToAction(nameof(Lockout));

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(
            [FromServices] ISignInManager signInManager)
        {
            await signInManager.SignOutAsync();
            return Redirect("/");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromServices] IConfigurationRegistry config,
            [FromQuery] string returnUrl = null)
        {
            if (await config.GetBooleanAsync("enable_register") == false)
                return Message(
                    title: "Registration closed",
                    message: "The registration of current site is closed now.\n" +
                        "If you believe this was a mistake, please contact the site administrator.",
                    type: BootstrapColor.secondary);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterModel model,
            [FromServices] IEmailSender emailSender,
            [FromServices] ISignInManager signInManager,
            [FromServices] IConfigurationRegistry config,
            [FromQuery] string returnUrl = null)
        {
            if (await config.GetBooleanAsync("enable_register") == false)
                return Message(
                    title: "Registration closed",
                    message: "The registration of current site is closed now.\n" +
                        "If you believe this was a mistake, please contact the site administrator.",
                    type: BootstrapColor.secondary);

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
            await emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

            await signInManager.SignInAsync(user, isPersistent: false);
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
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordModel model,
            [FromServices] IEmailSender emailSender)
        {
            if (!ModelState.IsValid) return View(model);
            
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "Sign",
                values: new { userId = $"{user.Id}", code, area = "Account" },
                protocol: Request.Scheme);
            await emailSender.SendEmailAsync(model.Email, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
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

        private ViewResult ErrorView(IdentityResult result, object model)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "Misc" });
            }
        }
    }
}
