﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SatelliteSite.Entities;
using SatelliteSite.IdentityModule.Models;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Controllers
{
    [Area("Account")]
    [Authorize]
    [Route("[controller]")]
    public class ProfileController : ViewControllerBase
    {
        UserManager UserManager { get; }
        SignInManager SignInManager { get; }
        ILogger<ProfileController> Logger { get; }
        IEmailSender EmailSender { get; }

        public ProfileController(
            UserManager um,
            SignInManager sim,
            ILogger<ProfileController> logger,
            IEmailSender emailSender)
        {
            UserManager = um;
            SignInManager = sim;
            Logger = logger;
            EmailSender = emailSender;
        }

        private IActionResult ViewWithError(IdentityResult result, object model)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        private async Task<User> GetUserAsync()
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
            if (user.NormalizedUserName != username.ToUpper())
                return NotFound();
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
            if (user.NormalizedUserName != username.ToUpper())
                return NotFound();

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
            if (user.NormalizedUserName != username.ToUpper())
                return NotFound();

            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (!hasPassword)
                return RedirectToAction(nameof(SetPassword));
            
            return View(new ChangePasswordModel());
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string username, ChangePasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (user.NormalizedUserName != username.ToUpper())
                return NotFound();

            var changePasswordResult = await UserManager
                .ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
                return ViewWithError(changePasswordResult, model);
            
            await SignInManager.SignInAsync(user, isPersistent: false);
            Logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }


        [HttpGet("{username}/[action]")]
        public async Task<IActionResult> SetPassword(string username)
        {
            var user = await GetUserAsync();
            if (user.NormalizedUserName != username.ToUpper())
                return NotFound();

            var hasPassword = await UserManager.HasPasswordAsync(user);
            if (hasPassword)
                return RedirectToAction(nameof(ChangePassword));
            
            return View(new SetPasswordModel());
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(string username, SetPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (user.NormalizedUserName != username.ToUpper())
                return NotFound();

            var addPasswordResult = await UserManager
                .AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
                return ViewWithError(addPasswordResult, model);

            await SignInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Your password has been set.";
            return RedirectToAction(nameof(SetPassword));
        }


        [HttpPost("{username}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(string username, IndexViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await GetUserAsync();
            if (user.NormalizedUserName != username.ToUpper())
                return NotFound();

            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Sign",
                values: new { userId = $"{user.Id}", code, area = "Account" },
                protocol: Request.Scheme);

            try
            {
                await EmailSender.SendEmailConfirmationAsync(user.Email, callbackUrl);
                StatusMessage = "Verification email sent. Please check your email.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error sending mails: " + ex.Message;
                Logger.LogError(ex, "Mail sending failed.");
            }

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
                if (user.NormalizedUserName != username.ToUpper())
                    return NotFound();

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