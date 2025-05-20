using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using MicrosoftGraphCSharpe.Library.Services;
using Microsoft.Graph.Models;
using Moq;
using MicrosoftGraphCSharpe.Library.Models;
using System.Collections.Generic;

namespace MicrosoftGraphCSharpe.Tests
{
    [TestClass]
    public class TeamsServiceTests
    {
        private Mock<GraphServiceClient> _mockGraphServiceClient = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private TeamsService _teamsService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockGraphServiceClient = new Mock<GraphServiceClient>(MockBehavior.Strict, null, null, null);
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup mock configuration with UseLocalMockData = false to use real API calls in tests
            _mockConfiguration.Setup(c => c.GetValue<bool>("UseLocalMockData", false)).Returns(false);
            
            _teamsService = new TeamsService(_mockGraphServiceClient.Object, _mockConfiguration.Object);
        }

        [TestMethod]
        public async Task ListMyTeamsAsync_ReturnsTeams()
        {
            // Arrange
            var teamCollectionResponse = new TeamCollectionResponse
            {
                Value = new List<Team>
                {
                    new Team { Id = "team1", DisplayName = "Team 1" },
                    new Team { Id = "team2", DisplayName = "Team 2" }
                }
            };

            // Update to use Teams endpoint instead of Me.JoinedTeams
            _mockGraphServiceClient.Setup(g => g.Teams.GetAsync(
                    It.IsAny<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Graph.Teams.TeamsRequestBuilder.TeamsRequestBuilderGetQueryParameters>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(teamCollectionResponse);

            // Act
            var result = await _teamsService.ListMyTeamsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual("Team 1", result[0].DisplayName);
        }

        [TestMethod]
        public async Task ListChannelsAsync_ReturnsChannels()
        {
            // Arrange
            var teamId = "test-team-id";
            var channelCollectionResponse = new ChannelCollectionResponse
            {
                Value = new List<Channel>
                {
                    new Channel { Id = "channel1", DisplayName = "Channel 1" },
                    new Channel { Id = "channel2", DisplayName = "Channel 2" }
                }
            };

            _mockGraphServiceClient.Setup(g => g.Teams[teamId].Channels.GetAsync(
                    It.IsAny<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Graph.Teams.Item.Channels.ChannelsRequestBuilder.ChannelsRequestBuilderGetQueryParameters>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(channelCollectionResponse);

            // Act
            var result = await _teamsService.ListChannelsAsync(teamId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual("Channel 1", result[0].DisplayName);
        }

        [TestMethod]
        public async Task SendMessageToChannelAsync_ReturnsSentMessage()
        {
            // Arrange
            var teamId = "test-team-id";
            var channelId = "test-channel-id";
            var messageContent = "Hello World";
            var chatMessage = new ChatMessage { Body = new ItemBody { Content = messageContent } };
            var sentChatMessage = new ChatMessage { Id = "message-id", Body = new ItemBody { Content = messageContent } };

            _mockGraphServiceClient.Setup(g => g.Teams[teamId].Channels[channelId].Messages.PostAsync(
                    It.IsAny<ChatMessage>(),
                    It.IsAny<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Kiota.Abstractions.DefaultQueryParameters>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(sentChatMessage);

            // Act
            var result = await _teamsService.SendMessageToChannelAsync(teamId, channelId, messageContent);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("message-id", result!.Id);
            Assert.AreEqual(messageContent, result.Body!.Content);
        }

        [TestMethod]
        public async Task ListChannelMessagesAsync_ReturnsMessages()
        {
            // Arrange
            var teamId = "test-team-id";
            var channelId = "test-channel-id";
            var chatMessageCollectionResponse = new ChatMessageCollectionResponse
            {
                Value = new List<ChatMessage>
                {
                    new ChatMessage { Id = "msg1", Body = new ItemBody { Content = "Message 1" } },
                    new ChatMessage { Id = "msg2", Body = new ItemBody { Content = "Message 2" } }
                }
            };

            _mockGraphServiceClient.Setup(g => g.Teams[teamId].Channels[channelId].Messages.GetAsync(
                    It.IsAny<Action<Microsoft.Kiota.Abstractions.RequestConfiguration<Microsoft.Graph.Teams.Item.Channels.Item.Messages.MessagesRequestBuilder.MessagesRequestBuilderGetQueryParameters>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMessageCollectionResponse);

            // Act
            var result = await _teamsService.ListChannelMessagesAsync(teamId, channelId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual("Message 1", result[0].Body!.Content);
        }
    }
}
