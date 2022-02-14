using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.EasyAuth
{
    public class EasyAuthAuthenticationHandler : AuthenticationHandler<EasyAuthAuthenticationOptions>
    {
        public EasyAuthAuthenticationHandler(
            IOptionsMonitor<EasyAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        private EasyAuthClientPrincipal? GetClaimsPrincipal(out string? authenticationScheme)
        {
            string? enabledEnv = Environment.GetEnvironmentVariable("WEBSITE_AUTH_ENABLED", EnvironmentVariableTarget.Process);
            if (!string.Equals(enabledEnv, "True", StringComparison.InvariantCultureIgnoreCase))
            {
                authenticationScheme = null;
                return null;
            }

            authenticationScheme = Context.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault();
            string? msClientPrincipalEncoded = Context.Request.Headers["X-MS-CLIENT-PRINCIPAL"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(msClientPrincipalEncoded)
                || string.IsNullOrWhiteSpace(authenticationScheme))
                return null;

            byte[] decodedBytes = Convert.FromBase64String(msClientPrincipalEncoded);
            string msClientPrincipalDecoded = Encoding.Default.GetString(decodedBytes);
            return JsonSerializer.Deserialize<EasyAuthClientPrincipal>(msClientPrincipalDecoded);
        }

        private Claim? MapClaims(EasyAuthClientPrincipal.UserClaim claim)
        {
            string? type = claim.Type switch
            {
                "preferred_username" => "name",
                "name" => "preferred_username",
                "roles" => "role",
                "appid" or "appidacr" => claim.Type,
                "exp" or "aio" or "aud" or "iss" or "iat" or "nbf" => claim.Type,
                "ipaddr" or "uti" or "c_hash" or "nonce" or "ver" or "rh" => claim.Type,
                "http://schemas.microsoft.com/claims/authnmethodsreferences" => "amr",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" => "surname",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" => "givenname",
                "http://schemas.microsoft.com/identity/claims/objectidentifier" => "oid",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => "sub",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => "name",
                "http://schemas.microsoft.com/identity/claims/tenantid" => "tid",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" => "upn",
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => "role",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" => "email",
                "http://schemas.microsoft.com/identity/claims/identityprovider" => "idp",
                _ => null,
            };

            if (type == null) Logger.LogInformation("Unknown claim type {claimType} and value '{claimValue}'", claim.Type, claim.Value);

            return type != null ? new Claim(type, claim.Value) : null;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                EasyAuthClientPrincipal? clientPrincipal = GetClaimsPrincipal(out string? easyAuthProvider);
                if (clientPrincipal == null || easyAuthProvider == null)
                {
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                ClaimsPrincipal principal = new(
                    new ClaimsIdentity(
                        clientPrincipal.Claims.Select(MapClaims).Where(c => c != null).ToList()!,
                        "EasyAuth-" + clientPrincipal.AuthenticationType,
                        "name",
                        "role"));

                Context.User = principal;
                return Task.FromResult(
                    AuthenticateResult.Success(
                        new AuthenticationTicket(principal, easyAuthProvider)));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (!Options.UseHttp302ForChallenge && Request.Path.StartsWithSegments("/api"))
            {
                Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            else
            {
                Response.Redirect(Options.LoginUrl + "?post_login_redirect_uri=" + UrlEncoder.Encode(Request.Path.Value ?? "/"));
                return Task.CompletedTask;
            }
        }
    }
}
