using Azure.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Identity
{
    /// <summary>
    /// The provider for azure token credentials.
    /// </summary>
    public interface ITokenCredentialProvider
    {
        /// <summary>
        /// Gets an <see cref="AccessToken"/> for the specified set of scopes.
        /// </summary>
        /// <param name="scopes">The requested authentication scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>A valid <see cref="AccessToken"/>. This token may be cached.</returns>
        Task<AccessToken> GetTokenAsync(string[] scopes, CancellationToken cancellationToken = default);
    }
}
