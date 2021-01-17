using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        IUserManager UserManager { get; }

        public ProfileController(IUserManager um)
        {
            UserManager = um;
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


        [HttpGet("{username}")]
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
                IsEmailConfirmed = await UserManager.IsEmailConfirmedAsync(user),
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

            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (!hasPassword)
                return RedirectToAction(nameof(SetPassword));
            
            return View(new ChangePasswordModel());
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            [FromRoute] string username,
            ChangePasswordModel model,
            [FromServices] ISignInManager signInManager)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var changePasswordResult = await UserManager
                .ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
                return ViewWithError(changePasswordResult, model);
            
            await signInManager.SignInAsync(user, isPersistent: false);
            await HttpContext.AuditAsync("changed password", user.Id.ToString());
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }


        [HttpGet("{username}/[action]")]
        public async Task<IActionResult> SetPassword(string username)
        {
            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (hasPassword)
                return RedirectToAction(nameof(ChangePassword));
            
            return View(new SetPasswordModel());
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(
            [FromRoute] string username,
            SetPasswordModel model,
            [FromServices] ISignInManager signInManager)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (!user.HasUserName(username)) return NotFound();

            var addPasswordResult = await UserManager
                .AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
                return ViewWithError(addPasswordResult, model);

            await signInManager.SignInAsync(user, isPersistent: false);
            await HttpContext.AuditAsync("set password", user.Id.ToString());
            StatusMessage = "Your password has been set.";

            return RedirectToAction(nameof(SetPassword));
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(
            [FromRoute] string username,
            IndexViewModel model,
            [FromServices] IEmailSender emailSender,
            [FromServices] ILogger<IEmailSender> logger)
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

            try
            {
                await emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl);
                StatusMessage = "Verification email sent. Please check your email.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error sending mails: " + ex.Message;
                logger.LogError(ex, "Mail sending failed.");
            }

            await HttpContext.AuditAsync("send verification email", user.Id.ToString());
            return RedirectToAction(nameof(Edit));
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromRoute] string username,
            IndexViewModel model,
            [FromServices] ISignInManager signInManager)
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
                await signInManager.RefreshSignInAsync(user);
                await HttpContext.AuditAsync("update profile", user.Id.ToString());
                StatusMessage = "Your profile has been updated";
            }
            catch (ApplicationException ex)
            {
                StatusMessage = ex.Message;
            }

            return RedirectToAction(nameof(Edit));
        }
    }
}
