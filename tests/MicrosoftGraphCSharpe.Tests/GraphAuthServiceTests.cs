using Microsoft.Extensions.Configuration;
using MicrosoftGraphCSharpe.Library.Auth;
using Moq;

namespace MicrosoftGraphCSharpe.Tests
{
    [TestClass]
    public class GraphAuthServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = null!;

        [TestInitialize]
        public void Setup()
        {
            // Setup mock configuration for each test
            var appSettings = new Dictionary<string, string?>
            {
                {"AzureAd:TenantId", "test_tenant_id"},
                {"AzureAd:ClientId", "test_client_id"},
                {"AzureAd:ClientSecret", "test_client_secret"}
            };

            _mockConfiguration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns((string?)null); // For GetValue
            configurationSection.Setup(a => a.Key).Returns((string?)null); // For GetValue
            configurationSection.Setup(a => a.Path).Returns((string?)null); // For GetValue


            _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()))
                .Returns((string key) => {
                    var mockSection = new Mock<IConfigurationSection>();
                    mockSection.Setup(s => s.Value).Returns(appSettings.TryGetValue(key, out var value) ? value : null);
                    mockSection.Setup(s => s.Key).Returns(key.Split(':').Last());
                     mockSection.Setup(s => s.Path).Returns(key);

                    // Handle nested sections like "AzureAd:TenantId"
                    if (key.Contains(':'))
                    {
                        var parts = key.Split(':');
                        var topLevelKey = parts[0];
                        var nestedKey = parts[1];
                        
                        var topLevelSection = new Mock<IConfigurationSection>();

                        var nestedAppSettings = appSettings
                            .Where(kvp => kvp.Key.StartsWith(topLevelKey + ":"))
                            .ToDictionary(kvp => kvp.Key.Substring(topLevelKey.Length + 1), kvp => kvp.Value);

                        topLevelSection.Setup(s => s.GetChildren()).Returns(
                            nestedAppSettings.Select(kvp => {
                                var childSection = new Mock<IConfigurationSection>();
                                childSection.Setup(cs => cs.Key).Returns(kvp.Key);
                                childSection.Setup(cs => cs.Value).Returns(kvp.Value);
                                return childSection.Object;
                            }).ToList());
                        
                        // Specific setup for GetSection("AzureAd")["TenantId"]
                         _mockConfiguration.Setup(c => c.GetSection(topLevelKey).GetSection(nestedKey))
                            .Returns(() => {
                                var childSection = new Mock<IConfigurationSection>();
                                childSection.Setup(s => s.Value).Returns(appSettings[key]);
                                return childSection.Object;
                            });
                        
                        _mockConfiguration.Setup(c => c.GetSection(topLevelKey))
                            .Returns(topLevelSection.Object);

                        return mockSection.Object; // Should not be hit if GetSection("AzureAd") is called first
                    }
                    
                    return mockSection.Object;
                });
        }

        [TestMethod]
        public void GetAuthenticatedGraphClient_WithValidConfig_ReturnsClient()
        {
            // Arrange
            var authService = new GraphAuthService(_mockConfiguration.Object);

            // Act
            var client = authService.GetAuthenticatedGraphClient();

            // Assert
            Assert.IsNotNull(client);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetAuthenticatedGraphClient_MissingTenantId_ThrowsArgumentNullException()
        {
            // Arrange
            var appSettings = new Dictionary<string, string?>
            {
                //{"AzureAd:TenantId", "test_tenant_id"}, // Missing TenantId
                {"AzureAd:ClientId", "test_client_id"},
                {"AzureAd:ClientSecret", "test_client_secret"}
            };
             _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()))
                .Returns((string key) => {
                    var mockSection = new Mock<IConfigurationSection>();
                    mockSection.Setup(s => s.Value).Returns(appSettings.TryGetValue(key, out var value) ? value : null);
                    mockSection.Setup(s => s.Key).Returns(key.Split(':').Last());
                    mockSection.Setup(s => s.Path).Returns(key);
                     if (key.Contains(':'))
                    {
                        var parts = key.Split(':');
                        var topLevelKey = parts[0];
                        var nestedKey = parts[1];
                        
                        var topLevelSection = new Mock<IConfigurationSection>();

                        var nestedAppSettings = appSettings
                            .Where(kvp => kvp.Key.StartsWith(topLevelKey + ":"))
                            .ToDictionary(kvp => kvp.Key.Substring(topLevelKey.Length + 1), kvp => kvp.Value);
                        
                        topLevelSection.Setup(s => s.GetChildren()).Returns(
                            nestedAppSettings.Select(kvp => {
                                var childSection = new Mock<IConfigurationSection>();
                                childSection.Setup(cs => cs.Key).Returns(kvp.Key);
                                childSection.Setup(cs => cs.Value).Returns(kvp.Value);
                                return childSection.Object;
                            }).ToList());
                        
                         _mockConfiguration.Setup(c => c.GetSection(topLevelKey).GetSection(nestedKey))
                            .Returns(() => {
                                var childSection = new Mock<IConfigurationSection>();
                                // Simulate missing TenantId by returning null for its value
                                if (key == "AzureAd:TenantId") childSection.Setup(s => s.Value).Returns((string)null);
                                else childSection.Setup(s => s.Value).Returns(appSettings[key]);
                                return childSection.Object;
                            });
                        
                        _mockConfiguration.Setup(c => c.GetSection(topLevelKey))
                            .Returns(topLevelSection.Object);
                    }
                    return mockSection.Object;
                });


            var authService = new GraphAuthService(_mockConfiguration.Object);

            // Act
            authService.GetAuthenticatedGraphClient(); // Should throw
        }
    }
}
