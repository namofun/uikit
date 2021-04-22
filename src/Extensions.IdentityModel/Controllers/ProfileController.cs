using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.IdentityModule.Models;
using SatelliteSite.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Controllers
{
    [Area("Account")]
    [Authorize]
    [Route("[controller]")]
    [AuditPoint(AuditlogType.User)]
    public class ProfileController : ViewControllerBase
    {
        private IUserManager UserManager { get; }
        private ISignInManager SignInManager { get; }

        public ProfileController(ISignInManager signInManager)
        {
            SignInManager = signInManager;
            UserManager = signInManager.UserManager;
        }

        private IActionResult ViewWithError(IdentityResult result, object model)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        private async Task<IUser> GetUserAsync()
        {
            var user = await UserManager.GetUserAsync(User);
            var userId = UserManager.GetUserId(User);
            ViewBag.User = user;
            return user ?? throw new ApplicationException(
                $"Unable to load user with ID '{userId}'.");
        }


        [HttpGet("{username}/[action]")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Claims(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();
            return View();
        }


        [HttpGet("{username}", Name = "AccountProfile")]
        [AllowAnonymous]
        public async Task<IActionResult> Show(string username)
        {
            var user = await UserManager.FindByNameAsync(username);
            if (user is null) return NotFound();
            ViewBag.User = user;
            return View();
        }


        [HttpGet("{username}/[action]")]
        public async Task<IActionResult> Edit(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var model = new IndexViewModel
            {
                Username = user.UserName,
                NickName = user.NickName,
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                Plan = user.Plan,
                SubscribeNews = user.SubscribeNews,
            };

            if (string.IsNullOrEmpty(StatusMessage))
                TempData["StatusMessage"] = "You can change your avatar in GAVATAR.";

            return View(model);
        }


        [HttpGet("{username}/[action]")]
        public async Task<IActionResult> ChangePassword(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            if (!user.HasPassword())
                return RedirectToAction(nameof(SetPassword));
            
            return View(new ChangePasswordModel());
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string username, ChangePasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var changePasswordResult = await UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
                return ViewWithError(changePasswordResult, model);

            await SignInManager.SignInAsync(user, isPersistent: false);
            await HttpContext.AuditAsync("changed password", user.Id.ToString());
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }


        [HttpGet("{username}/[action]")]
        public async Task<IActionResult> SetPassword(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            if (user.HasPassword())
                return RedirectToAction(nameof(ChangePassword));
            
            return View(new SetPasswordModel());
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(string username, SetPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var addPasswordResult = await UserManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
                return ViewWithError(addPasswordResult, model);

            await SignInManager.SignInAsync(user, isPersistent: false);
            await HttpContext.AuditAsync("set password", user.Id.ToString());
            StatusMessage = "Your password has been set.";

            return RedirectToAction(nameof(SetPassword));
        }


        [HttpGet("{username}/[action]")]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public async Task<IActionResult> ExternalLogins(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var model = new ExternalLoginsModel { CurrentLogins = await UserManager.GetLoginsAsync(user) };
            model.OtherLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = user.HasPassword() || model.CurrentLogins.Count > 1;
            model.StatusMessage = StatusMessage;

            return View(model);
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public async Task<IActionResult> LinkLogin(string username, string provider)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback));
            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, User.GetUserId());
            return new ChallengeResult(provider, properties);
        }


        [HttpGet("{username}/[action]")]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public async Task<IActionResult> LinkLoginCallback(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var info = await SignInManager.GetExternalLoginInfoAsync(user.Id.ToString());
            if (info == null)
            {
                throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await UserManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToAction(nameof(ExternalLogins));
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.ExternalLogin))]
        public async Task<IActionResult> RemoveLogin(string username, RemoveLoginModel model)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var result = await UserManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
            }

            await SignInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "The external login was removed.";
            return RedirectToAction(nameof(ExternalLogins));
        }


        [HttpGet("{username}/2fa")]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> TwoFactorAuthentication(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            return View(new TwoFactorAuthenticationModel
            {
                HasAuthenticator = await UserManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodes = await UserManager.GetRecoveryCodesAsync(user),
            });
        }


        [HttpGet("/profile/{username}/disable-2fa")]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> Disable2faWarning(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            return View(nameof(Disable2fa));
        }


        [HttpPost("/profile/{username}/disable-2fa")]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> Disable2fa(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var disable2faResult = await UserManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            await HttpContext.AuditAsync("disabled 2fa", $"{user.Id}");
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }


        [HttpGet("{username}/enable-2fa")]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> EnableAuthenticator(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var model = new EnableAuthenticatorModel();
            await LoadSharedKeyAndQrCodeUriAsync(user, model);

            return View(model);
        }


        [HttpPost("{username}/enable-2fa")]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> EnableAuthenticator(string username, EnableAuthenticatorModel model)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await UserManager.VerifyTwoFactorTokenAsync(
                user, UserManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            await UserManager.SetTwoFactorEnabledAsync(user, true);
            await HttpContext.AuditAsync("enabled 2fa", $"{user.Id}");

            await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            StatusMessage = "Authenticator app enrolled! New recovery codes are generated and showed bellow. Please keep them.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }


        [HttpGet("{username}/reset-2fa")]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> ResetAuthenticatorWarning(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            return View(nameof(ResetAuthenticator));
        }


        [HttpPost("{username}/reset-2fa")]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> ResetAuthenticator(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            await UserManager.SetTwoFactorEnabledAsync(user, false);
            await UserManager.ResetAuthenticatorKeyAsync(user);
            await HttpContext.AuditAsync("reset 2fa key", $"{user.Id}");

            return RedirectToAction(nameof(EnableAuthenticator));
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        [IdentityAdvancedFeature(nameof(IdentityAdvancedOptions.TwoFactorAuthentication))]
        public async Task<IActionResult> GenerateRecoveryCodes(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            if (!user.TwoFactorEnabled) return No2faEnabled("generate recovery codes");
            await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            StatusMessage = "New recovery codes are generated and showed bellow. Please keep them.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(
            [FromRoute] string username,
            IndexViewModel model,
            [FromServices] IEmailSender emailSender)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Sign",
                values: new { userId = $"{user.Id}", code, area = "Account" },
                protocol: Request.Scheme);

            await emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl);
            StatusMessage = "Verification email sent. Please check your email.";

            await HttpContext.AuditAsync("send verification email", user.Id.ToString());
            return RedirectToAction(nameof(Edit));
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string username, IndexViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await GetUserAsync();
                if (!user.HasUserName(username)) return NotFound();

                var email = user.Email;
                if (model.Email != email)
                {
                    var setEmailResult = await UserManager.SetEmailAsync(user, model.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        throw new ApplicationException(
                            $"Unexpected error occurred setting email for user with ID '{user.Id}'. "
                            + (setEmailResult.Errors.FirstOrDefault()?.Description ?? ""));
                    }
                }

                if (string.IsNullOrEmpty(model.NickName))
                    model.NickName = null;
                if (string.IsNullOrEmpty(model.Plan))
                    model.Plan = null;

                user.NickName = model.NickName;
                user.Plan = model.Plan;
                user.SubscribeNews = model.SubscribeNews;

                await UserManager.UpdateAsync(user);
                await SignInManager.RefreshSignInAsync(user);
                await HttpContext.AuditAsync("update profile", user.Id.ToString());
                StatusMessage = "Your profile has been updated";
            }
            catch (ApplicationException ex)
            {
                StatusMessage = ex.Message;
            }

            return RedirectToAction(nameof(Edit));
        }


        private async Task LoadSharedKeyAndQrCodeUriAsync(IUser user, EnableAuthenticatorModel model)
        {
            var unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await UserManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey);
            model.AuthenticatorUri = UserManager.FormatAuthenticatorUri(user.UserName, user.Email, unformattedKey);

            static string FormatKey(string unformattedKey)
            {
                var result = new System.Text.StringBuilder();

                int currentPosition = 0;
                while (currentPosition + 4 < unformattedKey.Length)
                {
                    result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                    currentPosition += 4;
                }

                if (currentPosition < unformattedKey.Length)
                {
                    result.Append(unformattedKey[currentPosition..]);
                }

                return result.ToString().ToLowerInvariant();
            }
        }

        private ShowMessageResult No2faEnabled(string action)
            => Message(
                title: "Two-factor authentication",
                message: $"Cannot {action} for you as you do not have 2FA enabled.",
                type: BootstrapColor.danger);
    }
}
