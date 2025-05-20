using Microsoft.Graph;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Graph.Models;
using System.Linq; // Required for .Select()
using System;
using Microsoft.Extensions.Configuration;
using MicrosoftGraphCSharpe.Library.Models;

namespace MicrosoftGraphCSharpe.Library.Services
{
    public class TeamsService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IConfiguration _configuration;
        private readonly bool _useLocalMockData;
        private readonly SampleDataConfig _sampleData;

        public TeamsService(GraphServiceClient graphServiceClient, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            _useLocalMockData = _configuration.GetValue<bool>("UseLocalMockData", false);
            if (_useLocalMockData)
            {
                _sampleData = _configuration.GetSection("SampleData").Get<SampleDataConfig>();
                Console.WriteLine("Using local mock data for Teams API testing.");
            }
        }

        public async Task<List<Team>?> ListMyTeamsAsync()
        {
            Console.WriteLine("\n--- Listing All Accessible Teams ---");
            
            if (_useLocalMockData && _sampleData?.Teams != null)
            {
                Console.WriteLine("Using sample data for teams instead of API call.");
                var teams = _sampleData.Teams.Select(t => new Team
                {
                    Id = t.Id,
                    DisplayName = t.DisplayName,
                    Description = t.Description
                }).ToList();

                foreach (var team in teams)
                {
                    Console.WriteLine($"Team ID: {team.Id}, Name: {team.DisplayName}, Description: {team.Description ?? "N/A"}");
                }

                return teams;
            }
            
            try
            {
                // For application permissions, use /teams endpoint instead of /me/joinedTeams
                // This requires Team.ReadBasic.All or Team.ReadAll application permission
                Console.WriteLine("Fetching teams using application permissions...");
                var teamsResponse = await _graphServiceClient.Teams.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "description" };
                });

                if (teamsResponse?.Value != null && teamsResponse.Value.Any())
                {
                    foreach (var team in teamsResponse.Value)
                    {
                        Console.WriteLine($"Team ID: {team.Id}, Name: {team.DisplayName}, Description: {team.Description ?? "N/A"}");
                    }
                    return teamsResponse.Value;
                }
                else
                {
                    Console.WriteLine("No teams found or accessible by the application. Make sure the app has Team.ReadBasic.All or Team.ReadAll permission.");
                    return new List<Team>();
                }
            }
            catch (Exception ex)
            {
                // Log detailed error including the stack trace for easier debugging
                Console.WriteLine($"Error listing teams: {ex.Message}");
                Console.WriteLine($"Error details: {ex}");
                return null;
            }
        }

        public async Task<List<Channel>?> ListChannelsAsync(string teamId)
        {
            Console.WriteLine($"\n--- Listing Channels for Team ID: {teamId} ---");
            if (string.IsNullOrEmpty(teamId))
            {
                Console.WriteLine("Team ID cannot be empty.");
                return null;
            }
            
            if (_useLocalMockData && _sampleData?.Channels != null)
            {
                Console.WriteLine("Using sample data for channels instead of API call.");
                if (_sampleData.Channels.TryGetValue(teamId, out var sampleChannels))
                {
                    var channels = sampleChannels.Select(c => new Channel
                    {
                        Id = c.Id,
                        DisplayName = c.DisplayName,
                        Description = c.Description
                    }).ToList();

                    foreach (var channel in channels)
                    {
                        Console.WriteLine($"Channel ID: {channel.Id}, Name: {channel.DisplayName}, Description: {channel.Description ?? "N/A"}");
                    }

                    return channels;
                }
                else
                {
                    Console.WriteLine($"No sample channels found for team ID {teamId}");
                    return new List<Channel>();
                }
            }
            
            try
            {
                var channels = await _graphServiceClient.Teams[teamId].Channels.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "description" };
                });

                if (channels?.Value != null && channels.Value.Any())
                {
                    foreach (var channel in channels.Value)
                    {
                        Console.WriteLine($"Channel ID: {channel.Id}, Name: {channel.DisplayName}, Description: {channel.Description ?? "N/A"}");
                    }
                    return channels.Value;
                }
                else
                {
                    Console.WriteLine("No channels found in this team.");
                    return new List<Channel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing channels for team {teamId}: {ex.Message}");
                Console.WriteLine($"Error details: {ex}");
                return null;
            }
        }

        public async Task<ChatMessage?> SendMessageToChannelAsync(string teamId, string channelId, string messageContent)
        {
            Console.WriteLine($"\n--- Sending Message to Team ID: {teamId}, Channel ID: {channelId} ---");
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(messageContent))
            {
                Console.WriteLine("Team ID, Channel ID, and Message Content cannot be empty.");
                return null;
            }
            
            if (_useLocalMockData)
            {
                Console.WriteLine("Using mock implementation for sending message.");
                var messageId = Guid.NewGuid().ToString();
                Console.WriteLine($"Message sent successfully (mock). ID: {messageId}");
                
                return new ChatMessage
                {
                    Id = messageId,
                    Body = new ItemBody { Content = messageContent, ContentType = BodyType.Html },
                    From = new ChatMessageFromIdentitySet 
                    { 
                        User = new Identity { DisplayName = "Mock User" } 
                    }
                };
            }
            
            try
            {
                var requestBody = new ChatMessage
                {
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = messageContent
                    }
                };

                var sentMessage = await _graphServiceClient.Teams[teamId].Channels[channelId].Messages.PostAsync(requestBody);
                Console.WriteLine($"Message sent successfully. ID: {sentMessage?.Id}");
                return sentMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                Console.WriteLine($"Error details: {ex}");
                return null;
            }
        }

        public async Task<List<ChatMessage>?> ListChannelMessagesAsync(string teamId, string channelId, int top = 10)
        {
            Console.WriteLine($"\n--- Listing Last {top} Messages for Team ID: {teamId}, Channel ID: {channelId} ---");
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(channelId))
            {
                Console.WriteLine("Team ID and Channel ID cannot be empty.");
                return null;
            }
            
            if (_useLocalMockData && _sampleData?.Messages != null)
            {
                Console.WriteLine("Using sample data for messages instead of API call.");
                var key = $"{teamId}|{channelId}";
                if (_sampleData.Messages.TryGetValue(key, out var sampleMessages))
                {
                    var messages = sampleMessages.Select(m => new ChatMessage
                    {
                        Id = m.Id,
                        Body = new ItemBody { Content = m.Content },
                        From = new ChatMessageFromIdentitySet 
                        { 
                            User = new Identity { DisplayName = m.FromName } 
                        }
                    }).ToList();

                    foreach (var message in messages)
                    {
                        Console.WriteLine($"Message ID: {message.Id}, From: {message.From?.User?.DisplayName ?? "N/A"}, Content: {message.Body?.Content}");
                    }

                    return messages;
                }
                else
                {
                    Console.WriteLine($"No sample messages found for team ID {teamId} and channel ID {channelId}");
                    return new List<ChatMessage>();
                }
            }
            
            try
            {
                var messages = await _graphServiceClient.Teams[teamId].Channels[channelId].Messages.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Top = top;
                    requestConfiguration.QueryParameters.Orderby = new string[] { "lastModifiedDateTime desc" }; // Get the latest messages
                    requestConfiguration.QueryParameters.Select = new string[] { "id", "body", "from", "createdDateTime", "lastModifiedDateTime" };
                });

                if (messages?.Value != null && messages.Value.Any())
                {
                    foreach (var message in messages.Value)
                    {
                        Console.WriteLine($"Message ID: {message.Id}, From: {message.From?.User?.DisplayName ?? "N/A"}, Content: {message.Body?.Content}, Created: {message.CreatedDateTime}");
                    }
                    return messages.Value;
                }
                else
                {
                    Console.WriteLine("No messages found in this channel.");
                    return new List<ChatMessage>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing messages for channel {channelId}: {ex.Message}");
                Console.WriteLine($"Error details: {ex}");
                return null;
            }
        }
    }
}
