using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Identity;
using Microsoft.Extensions.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class AzureCredentialTests
    {
        private AzureManagementClient CreateViaDependencyInjection(Action<IServiceCollection> configureServices)
        {
            ServiceCollection services = new ServiceCollection();

            configureServices(services);

            services.AddHttpClient<AzureManagementClient>()
                .AddAzureAuthHandler(new[] { "https://management.azure.com/.default" })
                .ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri("https://management.azure.com/"));

            services.AddOptions<DefaultAzureCredentialOptions>()
                .Configure(options => options.VisualStudioTenantId = "65f7f058-fc47-4803-b7f6-1dd03a071b50");

            services.AddMemoryCache();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<AzureManagementClient>();
        }

        [TestMethod]
        [Ignore]
        public async Task TestAzureRM_Local()
        {
            AzureManagementClient client = CreateViaDependencyInjection(_ => { });
            SubscriptionResponse resp = await client.GetSubscriptionsAsync();
        }

        [TestMethod]
        public async Task TokenCredentialCaching()
        {
            FakeTokenCredential provider = new();
            FakeResponseHandler handler = new();
            FakeSystemClock systemClock = new();

            AzureManagementClient client = CreateViaDependencyInjection(services =>
            {
                services.AddSingleton<TokenCredential>(provider);
                services.AddSingleton<ITokenCredentialProvider, TokenCredentialProviderBase>();
                services.Configure<MemoryCacheOptions>(options => options.Clock = systemClock);

                services.AddHttpClient<AzureManagementClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler);
            });

            provider.AccessToken = new AccessToken("aaa", systemClock.UtcNow.AddMinutes(6));
            handler.Handle = request =>
            {
                Assert.AreEqual("Bearer", request.Headers.Authorization.Scheme);
                Assert.AreEqual("aaa", request.Headers.Authorization.Parameter);
                return FakeResponseHandler.DefaultHandler(request);
            };

            SubscriptionResponse resp = await client.GetSubscriptionsAsync();
            Assert.AreEqual(1, resp.Value.Length);
            Assert.AreEqual(1, provider.Requests.Count);

            resp = await client.GetSubscriptionsAsync();
            Assert.AreEqual(1, provider.Requests.Count);

            systemClock.UtcNow += TimeSpan.FromMinutes(10);
            provider.AccessToken = new AccessToken("bbb", systemClock.UtcNow.AddMinutes(6));
            handler.Handle = request =>
            {
                Assert.AreEqual("Bearer", request.Headers.Authorization.Scheme);
                Assert.AreEqual("bbb", request.Headers.Authorization.Parameter);
                return FakeResponseHandler.DefaultHandler(request);
            };

            resp = await client.GetSubscriptionsAsync();
            Assert.AreEqual(2, provider.Requests.Count);
        }

        private class FakeSystemClock : ISystemClock
        {
            public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
        }

        private class FakeTokenCredential : TokenCredential
        {
            public List<TokenRequestContext> Requests { get; } = new();

            public AccessToken AccessToken { get; set; }

            public bool WillFail { get; set; }

            public override AccessToken GetToken(
                TokenRequestContext requestContext,
                CancellationToken cancellationToken)
            {
                Requests.Add(requestContext);
                if (WillFail) throw new Exception("Hello");
                else return AccessToken;
            }

            public override ValueTask<AccessToken> GetTokenAsync(
                TokenRequestContext requestContext,
                CancellationToken cancellationToken)
                => ValueTask.FromResult(GetToken(requestContext, cancellationToken));
        }

        private class FakeResponseHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, HttpResponseMessage> Handle { get; set; } = DefaultHandler;

            public static HttpResponseMessage DefaultHandler(HttpRequestMessage request)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    RequestMessage = request,
                    Version = new Version(2, 0),
                    Content = JsonContent.Create(new SubscriptionResponse
                    {
                        Value = new[]
                        {
                            new Subscription()
                            {
                                DisplayName = "a",
                                Id = "b",
                                State = "c",
                                SubscriptionId = "d",
                                SubscriptionPolicies = new SubscriptionPolicies()
                                {
                                    LocationPlacementId = "e",
                                    QuotaId = "f",
                                    SpendingLimit = "g",
                                },
                            },
                        },
                    }),
                };
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(Handle(request));
            }
        }

        private class AzureManagementClient
        {
            private readonly HttpClient _client;

            public AzureManagementClient(HttpClient httpClient)
            {
                _client = httpClient;
            }

            public Task<SubscriptionResponse> GetSubscriptionsAsync()
            {
                return _client.GetFromJsonAsync<SubscriptionResponse>("subscriptions?api-version=2014-04-01");
            }
        }

        private class SubscriptionResponse
        {
            public Subscription[] Value { get; set; }
        }

        private class Subscription
        {
            public string Id { get; set; }
            public string SubscriptionId { get; set; }
            public string DisplayName { get; set; }
            public string State { get; set; }
            public SubscriptionPolicies SubscriptionPolicies { get; set; }
        }

        private class SubscriptionPolicies
        {
            public string LocationPlacementId { get; set; }
            public string QuotaId { get; set; }
            public string SpendingLimit { get; set; }
        }
    }
}
