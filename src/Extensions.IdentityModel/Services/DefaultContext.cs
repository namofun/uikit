using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;

namespace SatelliteSite
{
    public class DefaultContext : IdentityDbContext<User, Role, int>
    {
        public DefaultContext(DbContextOptions<DefaultContext> options)
            : base(options)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            this.FromSupplierToModelBuilder(builder);
        }
    }
}
