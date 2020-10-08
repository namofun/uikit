using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite
{
    public class DefaultContext : IdentityDbContext<User, Role, int>
    {
        public DefaultContext(DbContextOptions<DefaultContext> options)
            : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }
    }
}
