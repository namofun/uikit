using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authentication.EasyAuth
{
    public class EasyAuthClientPrincipal
    {
        [JsonProperty("auth_typ")]
        public string AuthenticationType { get; set; } = null!;

        [JsonProperty("claims")]
        public IEnumerable<UserClaim> Claims { get; set; } = Array.Empty<UserClaim>();

        [JsonProperty("name_typ")]
        public string NameType { get; set; } = null!;

        [JsonProperty("role_typ")]
        public string RoleType { get; set; } = null!;

        public class UserClaim
        {
            [JsonProperty("typ")]
            public string Type { get; set; } = null!;

            [JsonProperty("val")]
            public string Value { get; set; } = null!;
        }
    }
}
