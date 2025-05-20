using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using MicrosoftGraphCSharpe.Library.Auth;
using MicrosoftGraphCSharpe.Library.Services;
using System.IO; // Add this using directive

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var teamsService = host.Services.GetRequiredService<TeamsService>();
        if (teamsService == null)
        {
            Console.WriteLine("Error: TeamsService could not be loaded.");
            return;
        }

        try
        {
            Console.WriteLine("Listing your teams...");
            var teams = await teamsService.ListMyTeamsAsync();
            if (teams == null || !teams.Any())
            {
                Console.WriteLine("No teams found or error listing teams.");
                return;
            }

            var firstTeam = teams.First();
            Console.WriteLine($"First team found: {firstTeam.DisplayName} (ID: {firstTeam.Id})");

            if (string.IsNullOrEmpty(firstTeam.Id))
            {
                Console.WriteLine("First team ID is null or empty, cannot proceed.");
                return;
            }

            Console.WriteLine($"Listing channels for team {firstTeam.DisplayName}...");
            var channels = await teamsService.ListChannelsAsync(firstTeam.Id);
            if (channels == null || !channels.Any())
            {
                Console.WriteLine($"No channels found for team {firstTeam.DisplayName}.");
                return;
            }

            var firstChannel = channels.First();
            Console.WriteLine($"First channel found: {firstChannel.DisplayName} (ID: {firstChannel.Id})");
            
            if (string.IsNullOrEmpty(firstChannel.Id))
            {
                Console.WriteLine("First channel ID is null or empty, cannot proceed.");
                return;
            }

            var messageContent = "Hello from the C# Graph API app!";
            Console.WriteLine($"Sending message '{messageContent}' to channel {firstChannel.DisplayName}...");
            var sentMessage = await teamsService.SendMessageToChannelAsync(firstTeam.Id, firstChannel.Id, messageContent);
            if (sentMessage != null)
            {
                Console.WriteLine($"Message sent. Message ID: {sentMessage.Id}");
            }
            else
            {
                Console.WriteLine("Failed to send message.");
            }

            Console.WriteLine($"Listing messages for channel {firstChannel.DisplayName}...");
            var messages = await teamsService.ListChannelMessagesAsync(firstTeam.Id, firstChannel.Id);
            if (messages != null && messages.Any())
            {
                Console.WriteLine($"Found {messages.Count()} messages:");
                foreach (var msg in messages)
                {
                    Console.WriteLine($"- {msg.Body?.Content} (From: {msg.From?.User?.DisplayName ?? "Unknown"})");
                }
            }
            else
            {
                Console.WriteLine("No messages found or error listing messages.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            // Consider more detailed logging or error handling here
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Use current directory as base path for config files
                var basePath = Directory.GetCurrentDirectory();
                Console.WriteLine($"[DEBUG] Configuration base path set to: {basePath}");
                config.SetBasePath(basePath);
                
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                
                // For debugging: Check if the settings files are found
                string settingsPath = Path.Combine(basePath, "appsettings.json");
                string devSettingsPath = Path.Combine(basePath, $"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json");
                Console.WriteLine($"[DEBUG] Looking for settings file at: {settingsPath}");
                Console.WriteLine($"[DEBUG] Settings file exists: {File.Exists(settingsPath)}");
                Console.WriteLine($"[DEBUG] Looking for environment settings file at: {devSettingsPath}");
                Console.WriteLine($"[DEBUG] Environment settings file exists: {File.Exists(devSettingsPath)}");
                Console.WriteLine($"[DEBUG] Current Environment: {hostingContext.HostingEnvironment.EnvironmentName}");

                // For even more detailed context, list files in the directory
                Console.WriteLine("[DEBUG] Files in directory:");
                try
                {
                    foreach (var file in Directory.GetFiles(basePath))
                    {
                        Console.WriteLine($"  - {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error listing files: {ex.Message}");
                }

                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<GraphAuthService>();
                services.AddSingleton(provider =>
                {
                    var authService = provider.GetRequiredService<GraphAuthService>();
                    return authService.GetAuthenticatedGraphClient();
                });
                services.AddSingleton<TeamsService>(provider => {
                    var graphServiceClient = provider.GetRequiredService<GraphServiceClient>();
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    return new TeamsService(graphServiceClient, configuration);
                });
            });
}
