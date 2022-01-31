using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Identity
{
    public class TokenCredentialDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenCredentialProvider _tokenProvider;
        private readonly string[] _scopes;

        public TokenCredentialDelegatingHandler(
            ITokenCredentialProvider tokenProvider,
            string[] scopes)
        {
            _tokenProvider = tokenProvider;
            _scopes = scopes;
        }

        public static Func<IServiceProvider, TokenCredentialDelegatingHandler> Factory(string[] scopes)
        {
            return sp => new(sp.GetRequiredService<ITokenCredentialProvider>(), scopes);
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AccessToken authResult = await _tokenProvider.GetTokenAsync(_scopes, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
