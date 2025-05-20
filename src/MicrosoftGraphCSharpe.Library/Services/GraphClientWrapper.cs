using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphCSharpe.Library.Services
{
    /// <summary>
    /// GraphServiceClientのラッパークラス
    /// </summary>
    public class GraphClientWrapper : IGraphClientWrapper
    {
        private readonly GraphServiceClient _graphClient;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="graphClient">GraphServiceClientインスタンス</param>
        public GraphClientWrapper(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        /// <summary>
        /// ユーザーが参加しているチームの一覧を取得します
        /// </summary>
        public async Task<List<Team>> GetMyTeamsAsync()
        {
            var teams = await _graphClient.Me.JoinedTeams.GetAsync();
            return teams?.Value?.ToList() ?? new List<Team>();
        }

        /// <summary>
        /// チームのチャンネル一覧を取得します
        /// </summary>
        public async Task<List<Channel>> GetTeamChannelsAsync(string teamId)
        {
            var channels = await _graphClient.Teams[teamId].Channels.GetAsync();
            return channels?.Value?.ToList() ?? new List<Channel>();
        }

        /// <summary>
        /// チャンネルのメッセージ一覧を取得します
        /// </summary>
        public async Task<List<ChatMessage>> GetChannelMessagesAsync(string teamId, string channelId)
        {
            var messages = await _graphClient.Teams[teamId].Channels[channelId].Messages.GetAsync();
            return messages?.Value?.ToList() ?? new List<ChatMessage>();
        }

        /// <summary>
        /// チャンネルにメッセージを送信します
        /// </summary>
        public async Task<ChatMessage> SendMessageToChannelAsync(string teamId, string channelId, string messageContent)
        {
            var chatMessage = new ChatMessage
            {
                Body = new ItemBody
                {
                    Content = messageContent,
                    ContentType = BodyType.Text
                }
            };
            
            return await _graphClient.Teams[teamId].Channels[channelId].Messages.PostAsync(chatMessage);
        }
    }
}
