using Microsoft.Extensions.Caching.Memory;
using System;

namespace SatelliteSite.IdentityModule.Services
{
    public sealed class SlideExpirationService
    {
        private readonly IMemoryCache _cache;

        public SlideExpirationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Set(string userName)
        {
            _cache.Set(
                key: "SlideExpiration: " + userName.Normalize().ToUpperInvariant(),
                value: DateTimeOffset.UtcNow,
                absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(20));
        }

        public bool Get(string userName, DateTimeOffset? issuedUtc)
        {
            userName = userName.Normalize().ToUpperInvariant();
            return _cache.TryGetValue(
                "SlideExpiration: " + userName,
                out DateTimeOffset last) &&
                last > issuedUtc;
        }
    }
}
