using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;

namespace MicrosoftGraphCSharpe.Library.Auth
{
    public class GraphAuthService
    {
        private readonly IConfiguration _configuration;

        public GraphAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GraphServiceClient GetAuthenticatedGraphClient()
        {
            var clientId = _configuration["GraphApi:ClientId"];
            var clientSecret = _configuration["GraphApi:ClientSecret"];
            var tenantId = _configuration["GraphApi:TenantId"];

            if (string.IsNullOrEmpty(clientId) || 
                string.IsNullOrEmpty(clientSecret) || 
                string.IsNullOrEmpty(tenantId))
            {
                throw new System.Exception("Azure AD App registration details (ClientId, ClientSecret, TenantId) under 'GraphApi' section are missing or empty in configuration.");
            }

            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret);
            
            // Azure SDK logging can be configured here if needed.
            // Example: Azure.Core.Diagnostics.AzureSdkEventSourceListener.CreateConsoleLogger(System.Diagnostics.Tracing.EventLevel.LogAlways);

            var graphClient = new GraphServiceClient(clientSecretCredential);
            return graphClient;
        }
    }
}
