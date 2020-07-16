using Microsoft.AspNetCore.Identity;
using System;

namespace SatelliteSite.Entities
{
    public class User : IdentityUser<int>
    {
        public User() { }

        public User(string userName)
        {
            UserName = userName;
        }

        [PersonalData]
        public string NickName { get; set; }

        [PersonalData]
        public string Plan { get; set; }

        public DateTimeOffset? RegisterTime { get; set; }

        public bool SubscribeNews { get; set; } = true;
    }
}
