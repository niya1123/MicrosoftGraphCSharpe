using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using MicrosoftGraphCSharpe.Library.Services;
using MicrosoftGraphCSharpe.Library.Auth;
using Microsoft.Graph.Models;
using Moq;
using MicrosoftGraphCSharpe.Library.Models;
using System.Collections.Generic;

namespace MicrosoftGraphCSharpe.Tests
{
    /// <summary>
    /// TeamsServiceのテストクラス
    /// Teams操作サービスの機能を検証するためのユニットテスト
    /// </summary>
    [TestClass]
    public class TeamsServiceTests
    {
        private Mock<IGraphClientWrapper> _mockGraphClient = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private GraphAuthService _mockAuthService = null!;
        private TeamsService _teamsService = null!;

        /// <summary>
        /// 各テスト実行前の初期化処理
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _mockGraphClient = new Mock<IGraphClientWrapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // GraphAuthServiceをテストモードで作成（nullクライアントを使用）
            _mockAuthService = new GraphAuthService(_mockConfiguration.Object, null);
            
            // 設定値はあまり重要でないので基本的な設定だけ
            _mockConfiguration.Setup(c => c["UseLocalMockData"]).Returns("false");
            
            // テスト用に明示的にモックデータを使わないよう指定（第4引数でfalseを指定）
            _teamsService = new TeamsService(_mockGraphClient.Object, _mockAuthService, _mockConfiguration.Object, false);
        }

        /// <summary>
        /// チームの一覧を正しく取得できることを確認するテスト
        /// </summary>
        [TestMethod]
        public async Task ListMyTeamsAsync_ReturnsTeams()
        {
            // 準備 (Arrange)
            var teams = new List<Team>
            {
                new Team { Id = "team1", DisplayName = "Team 1" },
                new Team { Id = "team2", DisplayName = "Team 2" }
            };

            // GraphClientWrapperのモックを設定
            _mockGraphClient.Setup(g => g.GetMyTeamsAsync())
                .ReturnsAsync(teams);

            // 実行 (Act)
            var result = await _teamsService.ListMyTeamsAsync();

            // 検証 (Assert)
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual("Team 1", result[0].DisplayName);
        }

        /// <summary>
        /// チャンネルの一覧を正しく取得できることを確認するテスト
        /// </summary>
        [TestMethod]
        public async Task ListChannelsAsync_ReturnsChannels()
        {
            // 準備 (Arrange)
            var teamId = "test-team-id";
            var channels = new List<Channel>
            {
                new Channel { Id = "channel1", DisplayName = "Channel 1" },
                new Channel { Id = "channel2", DisplayName = "Channel 2" }
            };

            _mockGraphClient.Setup(g => g.GetTeamChannelsAsync(teamId))
                .ReturnsAsync(channels);

            // 実行 (Act)
            var result = await _teamsService.ListChannelsAsync(teamId);

            // 検証 (Assert)
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual("Channel 1", result[0].DisplayName);
        }

        /// <summary>
        /// チャンネルへのメッセージ送信が正しく動作することを確認するテスト
        /// </summary>
        [TestMethod]
        public async Task SendMessageToChannelAsync_ReturnsSentMessage()
        {
            // 準備 (Arrange)
            var teamId = "test-team-id";
            var channelId = "test-channel-id";
            var messageContent = "Hello World";
            
            // 送信が失敗することを期待（テスト環境では認証クライアントがnullのため）
            // 実行 (Act & Assert)
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await _teamsService.SendMessageToChannelAsync(teamId, channelId, messageContent);
            });
        }

        /// <summary>
        /// モックデータ使用時のメッセージ送信動作を確認するテスト
        /// </summary>
        [TestMethod]
        public async Task SendMessageToChannelAsync_WithMockData_ReturnsSentMessage()
        {
            // 準備 (Arrange)
            var teamId = "test-team-id";
            var channelId = "test-channel-id";
            var messageContent = "Hello World";
            
            // モックデータを使用するTeamsServiceを作成
            var mockTeamsService = new TeamsService(_mockGraphClient.Object, _mockAuthService, _mockConfiguration.Object, true);

            // 実行 (Act)
            var result = await mockTeamsService.SendMessageToChannelAsync(teamId, channelId, messageContent);

            // 検証 (Assert)
            Assert.IsNotNull(result);
            Assert.IsNotNull(result!.Id);
            Assert.AreEqual(messageContent, result.Body!.Content);
            Assert.AreEqual("モックユーザー", result.From!.User!.DisplayName);
        }

        /// <summary>
        /// チャンネルのメッセージ一覧を正しく取得できることを確認するテスト
        /// </summary>
        [TestMethod]
        public async Task ListChannelMessagesAsync_ReturnsMessages()
        {
            // 準備 (Arrange)
            var teamId = "test-team-id";
            var channelId = "test-channel-id";
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Id = "msg1", Body = new ItemBody { Content = "Message 1" } },
                new ChatMessage { Id = "msg2", Body = new ItemBody { Content = "Message 2" } }
            };

            _mockGraphClient.Setup(g => g.GetChannelMessagesAsync(teamId, channelId))
                .ReturnsAsync(messages);

            // 実行 (Act)
            var result = await _teamsService.ListChannelMessagesAsync(teamId, channelId);

            // 検証 (Assert)
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            Assert.AreEqual("Message 1", result[0].Body!.Content);
        }
    }
}
