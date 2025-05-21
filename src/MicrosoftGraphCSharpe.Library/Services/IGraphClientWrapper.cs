using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicrosoftGraphCSharpe.Library.Services
{
    /// <summary>
    /// GraphServiceClientのラッパーインターフェース
    /// テストの容易性のためにGraphServiceClientの必要なメソッドだけを抽象化します
    /// </summary>
    public interface IGraphClientWrapper
    {
        /// <summary>
        /// ユーザーが参加しているチームの一覧を取得します
        /// </summary>
        Task<List<Team>> GetMyTeamsAsync();

        /// <summary>
        /// チームのチャンネル一覧を取得します
        /// </summary>
        Task<List<Channel>> GetTeamChannelsAsync(string teamId);

        /// <summary>
        /// チャンネルのメッセージ一覧を取得します
        /// </summary>
        Task<List<ChatMessage>> GetChannelMessagesAsync(string teamId, string channelId);

        /// <summary>
        /// チャンネルにメッセージを送信します
        /// </summary>
        Task<ChatMessage> SendMessageToChannelAsync(string teamId, string channelId, string message);
    }
}
