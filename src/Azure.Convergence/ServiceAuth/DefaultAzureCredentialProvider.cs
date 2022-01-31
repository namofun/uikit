using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Identity
{
    public class TokenCredentialProviderBase : ITokenCredentialProvider
    {
        private readonly TokenCredential _credential;
        private readonly IMemoryCache _memoryCache;

        public TokenCredentialProviderBase(
            TokenCredential credential,
            IMemoryCache memoryCache)
        {
            _credential = credential;
            _memoryCache = memoryCache;
        }

        public Task<AccessToken> GetTokenAsync(
            string[] scopes,
            CancellationToken cancellationToken = default)
        {
            return _memoryCache.GetOrCreateAsync(new CacheKey(scopes), async cacheEntry =>
            {
                AccessToken accessToken =
                    await _credential.GetTokenAsync(
                        new TokenRequestContext(scopes),
                        cancellationToken);

                cacheEntry.AbsoluteExpiration = accessToken.ExpiresOn.AddMinutes(-5);
                return accessToken;
            });
        }

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly int _hashCode;
            public readonly string[] _scope;

            public bool Equals(CacheKey other)
                => _scope.Length == other._scope.Length
                    && _scope.SequenceEqual(other._scope);

            public override bool Equals(object? obj)
                => obj is CacheKey key && Equals(key);

            public override int GetHashCode()
                => _hashCode;

            public CacheKey(string[] scope)
            {
                if (scope == null)
                {
                    throw new ArgumentNullException(nameof(scope));
                }

                HashCode hashCode = new();
                hashCode.Add(scope.Length);
                for (int i = 0; i < scope.Length; i++)
                {
                    hashCode.Add(scope[i]);
                }

                _hashCode = hashCode.ToHashCode();
                _scope = scope;
            }
        }
    }

    public class DefaultAzureCredentialProvider : TokenCredentialProviderBase
    {
        public DefaultAzureCredentialProvider(
            IOptions<DefaultAzureCredentialOptions> options,
            IMemoryCache memoryCache)
            : base(new DefaultAzureCredential(options.Value), memoryCache)
        {
        }
    }
}
