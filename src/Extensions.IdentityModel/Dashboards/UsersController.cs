using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.IdentityModule.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/[controller]")]
    [AuditPoint(AuditlogType.User)]
    public class UsersController : ViewControllerBase
    {
        public IUserManager UserManager { get; }

        public UsersController(IUserManager userManager)
        {
            UserManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page < 1) return NotFound();
            var model = await UserManager.ListUsersAsync(page, 100);
            if (model.Count == 0) return NotFound();
            var userRoles = await UserManager.ListUserRolesAsync(model[0].Id, model[^1].Id);
            var roles = await UserManager.ListNamedRolesAsync();
            return View(model.As(a => (a, userRoles[a.Id].Where(roles.ContainsKey).Select(ur => roles[ur].Name))));
        }


        [HttpGet("{uid}")]
        public async Task<IActionResult> Detail(int uid)
        {
            var user = await UserManager.FindByIdAsync(uid);
            if (user == null) return NotFound();
            ViewBag.Roles = await UserManager.ListRolesAsync(user);
            return View(user);
        }


        [HttpGet("{uid}/[action]")]
        public async Task<IActionResult> Edit(int uid)
        {
            var user = await UserManager.FindByIdAsync(uid);
            if (user == null) return NotFound();

            var hasRole = await UserManager.ListUserRolesAsync(uid, uid);
            var roles = await UserManager.ListNamedRolesAsync();
            ViewBag.Roles = roles;

            return View(new UserEditModel
            {
                Email = user.Email,
                NickName = user.NickName,
                UserId = user.Id,
                UserName = user.UserName,
                Roles = hasRole[uid].Intersect(roles.Keys).ToArray()
            });
        }


        [HttpPost("{uid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int uid, UserEditModel model)
        {
            var user = await UserManager.FindByIdAsync(uid);
            if (user == null) return NotFound();

            var msg = "";

            if (model.Password != null)
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                var result = await UserManager.ResetPasswordAsync(user, token, model.Password);
                if (!result.Succeeded) msg += $"Error in reset password: {result.Errors.First().Description}.\n";
            }

            if (model.Email != null && user.Email != model.Email)
            {
                var result = await UserManager.SetEmailAsync(user, model.Email);
                if (!result.Succeeded) msg += $"Error in set email: {result.Errors.First().Description}.\n";
            }

            if (model.NickName != null)
            {
                user.NickName = model.NickName;
                var result = await UserManager.UpdateAsync(user);
                if (!result.Succeeded) msg += $"Error in set nickname: {result.Errors.First().Description}.\n";
            }

            // checking roles
            var hasRoles = await UserManager.ListUserRolesAsync(uid, uid);
            var roles = await UserManager.ListNamedRolesAsync();
            var hasRole = hasRoles[uid].Intersect(roles.Keys).ToArray();
            model.Roles = roles.Keys.Intersect(model.Roles ?? Enumerable.Empty<int>()).ToArray();
            if (UserManager.GetUserName(User) == user.UserName)
                model.Roles = model.Roles.Append(-1).Distinct().ToArray();
            var r1 = await UserManager.AddToRolesAsync(user,
                model.Roles.Except(hasRole).Select(i => roles[i].Name));
            var r2 = await UserManager.RemoveFromRolesAsync(user,
                hasRole.Except(model.Roles).Select(i => roles[i].Name));
            if (!r1.Succeeded) msg += $"Error in adding roles: {r1.Errors.First().Description}.\n";
            if (!r2.Succeeded) msg += $"Error in removing roles: {r2.Errors.First().Description}.\n";

            if (string.IsNullOrWhiteSpace(msg)) msg = null;
            StatusMessage = msg ?? $"User u{uid} updated successfully.";
            await HttpContext.AuditAsync("updated", $"{uid}");
            return RedirectToAction(nameof(Detail), new { uid });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> MailList()
        {
            var mails = await UserManager.ListSubscribedEmailsAsync();
            var sb = new StringBuilder();
            for (int i = 0; i < mails.Count; i++)
                sb.Append(mails[i]).Append(i == mails.Count - 1 || i % 50 == 49 ? "\n\n" : ";");
            return Content(sb.ToString());
        }
    }
}
