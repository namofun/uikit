using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.SampleModule
{
    public class RandomClaimsProvider : IUserClaimsProvider
    {
        public Task<IEnumerable<Claim>> GetClaimsAsync(IUser user)
        {
            var claim = new Claim("weather", Guid.NewGuid().ToString());
            return Task.FromResult<IEnumerable<Claim>>(new[] { claim });
        }
    }
}
