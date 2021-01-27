using Microsoft.AspNetCore.Identity;

namespace SatelliteSite.IdentityModule.Models
{
    public class RolesAddition : IAdditionalRole
    {
        public string Category => "Roles";

        public string Title { get; }

        public string Text { get; }

        public string GetUrl(object urlHelper) => null;

        public RolesAddition(IRole role)
        {
            Title = role.Description;
            Text = role.Description;
        }
    }
}
