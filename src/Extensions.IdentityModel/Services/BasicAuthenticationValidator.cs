using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    public class BasicAuthenticationValidator
    {
        private readonly ISignInSlideExpiration _cache;

        public BasicAuthenticationValidator(ISignInSlideExpiration cache)
        {
            _cache = cache;
        }

        public virtual async Task ValidateAsync(ValidateCredentialsContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Username))
            {
                context.Fail("User not found.");
                return;
            }

            var sp = context.HttpContext.RequestServices;
            var user = await _cache.FindAsync(sp, context.Username);
            if (user == null)
            {
                context.Fail("User not found.");
                return;
            }

            var attempt = _cache.VerifyPassword(sp, user, context.Password);
            if (attempt == PasswordVerificationResult.Failed)
            {
                context.Fail("Login failed, password not match.");
                return;
            }

            context.Principal = await _cache.IssueAsync(sp, user, false);
            context.Success();
        }
    }
}
