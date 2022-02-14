using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Authentication.EasyAuth
{
    public class EasyAuthClientPrincipal
    {
        [JsonPropertyName("auth_typ")]
        public string AuthenticationType { get; set; } = null!;

        [JsonPropertyName("claims")]
        public IEnumerable<UserClaim> Claims { get; set; } = Array.Empty<UserClaim>();

        [JsonPropertyName("name_typ")]
        public string NameType { get; set; } = null!;

        [JsonPropertyName("role_typ")]
        public string RoleType { get; set; } = null!;

        public class UserClaim
        {
            [JsonPropertyName("typ")]
            public string Type { get; set; } = null!;

            [JsonPropertyName("val")]
            public string Value { get; set; } = null!;

            public UserClaim()
            {
            }

            public UserClaim(Claim claim)
            {
                Type = claim.Type;
                Value = claim.Value;
            }
        }

        public EasyAuthClientPrincipal()
        {
        }

        public EasyAuthClientPrincipal(ClaimsIdentity claimsIdentity)
        {
            AuthenticationType = claimsIdentity.AuthenticationType!;
            NameType = claimsIdentity.NameClaimType;
            RoleType = claimsIdentity.RoleClaimType;
            Claims = claimsIdentity.Claims.Select(c => new UserClaim(c));
        }
    }
}
