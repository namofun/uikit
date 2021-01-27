﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ExtSystemClock = Microsoft.Extensions.Internal.SystemClock;

namespace Microsoft.AspNetCore.Authentication
{
    public sealed class DefaultSignInSlideExpiration<TUser> : ISignInSlideExpiration
        where TUser : SatelliteSite.IdentityModule.Entities.User
    {
        private readonly IMemoryCache _cache;
        private readonly ILookupNormalizer _normalizer;

        public DefaultSignInSlideExpiration()
        {
            _normalizer = new UpperInvariantLookupNormalizer();
            var systemClock = new ExtSystemClock();
            var options = new MemoryCacheOptions { Clock = systemClock };
            _cache = new MemoryCache(options);
        }

        public void MarkExpired(string userName)
        {
            userName = _normalizer.NormalizeName(userName);
            _cache.Set(
                key: "SlideExpiration: " + userName,
                value: DateTimeOffset.UtcNow,
                absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(20));
        }

        public bool CheckExpired(string userName, DateTimeOffset? issuedUtc)
        {
            userName = _normalizer.NormalizeName(userName);
            return _cache.TryGetValue(
                "SlideExpiration: " + userName,
                out DateTimeOffset last) &&
                last > issuedUtc;
        }

        public Task<IUser> FindAsync(IServiceProvider serviceProvider, string userName)
        {
            userName = _normalizer.NormalizeName(userName);
            return _cache.GetOrCreateAsync(
                "User: " + userName,
                async entry =>
                {
                    var userManager = serviceProvider.GetRequiredService<UserManager<TUser>>();
                    var result = await userManager.FindByNameAsync(userName);
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return (IUser)result;
                });
        }

        public PasswordVerificationResult VerifyPassword(IServiceProvider serviceProvider, IUser _user, string providedPassword)
        {
            if (!(_user is TUser user)) throw new ArgumentException("Error user type passed.");
            if (user.PasswordHash == null) return PasswordVerificationResult.Failed;
            var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<TUser>>();
            return passwordHasher.VerifyHashedPassword(user, user.PasswordHash, providedPassword);
        }

        public Task<ClaimsPrincipal> IssueAsync(IServiceProvider serviceProvider, IUser _user, bool full)
        {
            if (!(_user is TUser user)) throw new ArgumentException("Error user type passed.");
            return _cache.GetOrCreateAsync(
                "ClaimsPrincipal: " + user.NormalizedUserName,
                async entry =>
                {
                    var userClaimsPrincipalFactory = full
                        ? serviceProvider.GetRequiredService<IUserClaimsPrincipalFactory<TUser>>()
                        : serviceProvider.GetRequiredService<ILightweightUserClaimsPrincipalFactory<TUser>>();

                    var result = await userClaimsPrincipalFactory.CreateAsync(user);
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
                    return result;
                });
        }
    }
}
