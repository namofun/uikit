using Microsoft.AspNetCore.Identity;

namespace SatelliteSite.Entities
{
    public class Role : IdentityRole<int>
    {
        public Role() { }

        public Role(string roleName)
        {
            Name = roleName;
        }

        public string ShortName { get; set; }

        public string Description { get; set; }
    }
}
