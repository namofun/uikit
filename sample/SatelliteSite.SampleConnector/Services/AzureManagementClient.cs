using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SatelliteSite.SampleConnector.Services
{
    public class AzureManagementClient
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

    public class LogicAppsClient
    {
        private readonly HttpClient _client;
        private readonly string _url;

        public LogicAppsClient(HttpClient httpClient, IConfiguration configuration)
        {
            _client = httpClient;
            _url = configuration.GetConnectionString("LogicAppsDemo");
        }

        public async Task<string> InvokeAsync()
        {
            using var resp = await _client.PostAsJsonAsync(_url, new { });
            return await resp.Content.ReadAsStringAsync();
        }
    }

    public class SubscriptionResponse
    {
        public Subscription[] Value { get; set; }
    }

    public class Subscription
    {
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
        public string DisplayName { get; set; }
        public string State { get; set; }
        public SubscriptionPolicies SubscriptionPolicies { get; set; }
    }

    public class SubscriptionPolicies
    {
        public string LocationPlacementId { get; set; }
        public string QuotaId { get; set; }
        public string SpendingLimit { get; set; }
    }
}
