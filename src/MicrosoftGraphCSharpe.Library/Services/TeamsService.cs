using Microsoft.Graph;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Graph.Models;
using System.Linq; // .Select()に必要
using System;
using Microsoft.Extensions.Configuration;
using MicrosoftGraphCSharpe.Library.Models;
using MicrosoftGraphCSharpe.Library.Auth;

namespace MicrosoftGraphCSharpe.Library.Services
{
    /// <summary>
    /// TeamsService - Microsoft Teams操作サービス
    /// Microsoft Graph APIを使用してTeamsの操作（チームの一覧取得、チャンネルの操作、メッセージの送受信）を行います。
    /// Application認証とDelegated認証を自動的に切り替える機能を備えています。
    /// モックデータを使用したローカルテスト機能も備えています。
    /// </summary>
    public class TeamsService
    {
        private readonly IGraphClientWrapper _graphClient;
        private readonly GraphAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly bool _useLocalMockData;
        private readonly SampleDataConfig? _sampleData;

        /// <summary>
        /// コンストラクタ（GraphServiceClient使用）
        /// </summary>
        /// <param name="graphServiceClient">GraphServiceClientインスタンス</param>
        /// <param name="configuration">設定情報を提供するIConfigurationインスタンス</param>
        /// <param name="useLocalMockData">モックデータを使用するかどうか（テスト用）</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合にスローされます</exception>
        public TeamsService(GraphServiceClient graphServiceClient, IConfiguration configuration, bool? useLocalMockDataOverride = null)
            : this(new GraphClientWrapper(graphServiceClient), new GraphAuthService(configuration), configuration, useLocalMockDataOverride)
        {
        }

        /// <summary>
        /// コンストラクタ（IGraphClientWrapper使用）
        /// </summary>
        /// <param name="graphClient">GraphServiceClientのラッパー</param>
        /// <param name="authService">認証サービス</param>
        /// <param name="configuration">設定情報を提供するIConfigurationインスタンス</param>
        /// <param name="useLocalMockData">モックデータを使用するかどうか（テスト用）</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合にスローされます</exception>
        public TeamsService(IGraphClientWrapper graphClient, GraphAuthService authService, IConfiguration configuration, bool? useLocalMockDataOverride = null)
        {
            _graphClient = graphClient ?? throw new ArgumentNullException(nameof(graphClient));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // テスト用に直接設定できるようにする
            if (useLocalMockDataOverride.HasValue)
            {
                _useLocalMockData = useLocalMockDataOverride.Value;
            }
            else
            {
                // 通常の設定読み込み処理
                try
                {
                    _useLocalMockData = _configuration.GetValue<bool>("UseLocalMockData", false);
                }
                catch (Exception)
                {
                    // IConfigurationからの読み込みに失敗した場合はデフォルト値を使用
                    _useLocalMockData = false;
                }
            }
            
            if (_useLocalMockData)
            {
                try
                {
                    _sampleData = _configuration.GetSection("SampleData").Get<SampleDataConfig>() ?? new SampleDataConfig();
                    Console.WriteLine("Teams APIテストのためにローカルのモックデータを使用します。");
                }
                catch (Exception)
                {
                    // サンプルデータの読み込みに失敗した場合は空のオブジェクトを使用
                    _sampleData = new SampleDataConfig();
                }
            }
        }

        /// <summary>
        /// アクセス可能なTeamsの一覧を取得します
        /// </summary>
        /// <returns>Teamオブジェクトのリスト</returns>
        public async Task<List<Team>> ListMyTeamsAsync()
        {
            Console.WriteLine("\n--- アクセス可能なTeamsの一覧を取得します ---");
            
            // モックデータを使用する場合
            if (_useLocalMockData && _sampleData?.Teams != null)
            {
                Console.WriteLine("API呼び出しの代わりにサンプルデータを使用します。");
                var teams = _sampleData.Teams.Select(t => new Team
                {
                    Id = t.Id,
                    DisplayName = t.DisplayName,
                    Description = t.Description
                }).ToList();

                foreach (var team in teams)
                {
                    Console.WriteLine($"Team ID: {team.Id}, 名前: {team.DisplayName}, 説明: {team.Description ?? "なし"}");
                }

                return teams;
            }
            
            // 実際のAPI呼び出しを行う場合
            try
            {
                // アプリケーション権限（クライアント資格情報フロー）を使用する場合は/teamsエンドポイントを使用
                // Team.ReadBasic.AllまたはTeam.ReadAllアプリケーション権限が必要です
                Console.WriteLine("アプリケーション権限を使用してteamsを取得します...");
                var teams = await _graphClient.GetMyTeamsAsync();

                if (teams != null && teams.Any())
                {
                    foreach (var team in teams)
                    {
                        Console.WriteLine($"Team ID: {team.Id}, 名前: {team.DisplayName}, 説明: {team.Description ?? "なし"}");
                    }
                    return teams;
                }
                else
                {
                    Console.WriteLine("Teamsが見つからないか、アプリケーションからアクセスできません。アプリにTeam.ReadBasic.AllまたはTeam.ReadAll権限があることを確認してください。");
                    return new List<Team>();
                }
            }
            catch (Exception ex)
            {
                // より詳細なデバッグのためにスタックトレースを含むエラー情報をログに記録
                Console.WriteLine($"Teamsの一覧取得エラー: {ex.Message}");
                Console.WriteLine($"エラー詳細: {ex}");
                return new List<Team>();
            }
        }

        /// <summary>
        /// 指定されたチームのチャンネル一覧を取得します
        /// </summary>
        /// <param name="teamId">チームID</param>
        /// <returns>Channelオブジェクトのリスト</returns>
        public async Task<List<Channel>> ListChannelsAsync(string teamId)
        {
            Console.WriteLine($"\n--- チームID: {teamId} のチャンネル一覧を取得します ---");
            if (string.IsNullOrEmpty(teamId))
            {
                Console.WriteLine("チームIDが空です。");
                return new List<Channel>();
            }
            
            // モックデータを使用する場合
            if (_useLocalMockData && _sampleData?.Channels != null)
            {
                Console.WriteLine("API呼び出しの代わりにサンプルデータを使用します。");
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
                        Console.WriteLine($"チャンネルID: {channel.Id}, 名前: {channel.DisplayName}, 説明: {channel.Description ?? "なし"}");
                    }

                    return channels;
                }
                else
                {
                    Console.WriteLine($"チームID {teamId} に対するサンプルチャンネルが見つかりません");
                    return new List<Channel>();
                }
            }
            
            // 実際のAPI呼び出しを行う場合
            try
            {
                var channels = await _graphClient.GetTeamChannelsAsync(teamId);

                if (channels != null && channels.Any())
                {
                    foreach (var channel in channels)
                    {
                        Console.WriteLine($"チャンネルID: {channel.Id}, 名前: {channel.DisplayName}, 説明: {channel.Description ?? "なし"}");
                    }
                    return channels;
                }
                else
                {
                    Console.WriteLine("このチームにチャンネルが見つかりません。");
                    return new List<Channel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"チームID {teamId} のチャンネル一覧取得エラー: {ex.Message}");
                Console.WriteLine($"エラー詳細: {ex}");
                return new List<Channel>();
            }
        }

        /// <summary>
        /// 指定されたチャンネルにメッセージを送信します
        /// Delegated認証を使用して実際のユーザーとしてメッセージを送信します
        /// </summary>
        /// <param name="teamId">チームID</param>
        /// <param name="channelId">チャンネルID</param>
        /// <param name="messageContent">送信するメッセージの内容</param>
        /// <returns>送信されたメッセージ情報</returns>
        public async Task<ChatMessage> SendMessageToChannelAsync(string teamId, string channelId, string messageContent)
        {
            Console.WriteLine($"\n--- チームID: {teamId}, チャンネルID: {channelId} にメッセージを送信します ---");
            if (string.IsNullOrEmpty(teamId))
            {
                Console.WriteLine("チームIDは空にできません。");
                throw new ArgumentNullException(nameof(teamId), "チームIDはnullまたは空です。");
            }
            if (string.IsNullOrEmpty(channelId))
            {
                Console.WriteLine("チャンネルIDは空にできません。");
                throw new ArgumentNullException(nameof(channelId), "チャンネルIDはnullまたは空です。");
            }
            if (string.IsNullOrEmpty(messageContent?.Trim()))
            {
                Console.WriteLine("メッセージ内容は空にできません。");
                throw new ArgumentNullException(nameof(messageContent), "メッセージ内容はnullまたは空です。");
            }
            
            // モックデータを使用する場合
            if (_useLocalMockData)
            {
                Console.WriteLine("メッセージ送信のモック実装を使用します。");
                var messageId = Guid.NewGuid().ToString();
                Console.WriteLine($"✅ メッセージが正常に送信されました（モック）。ID: {messageId}");
                
                return new ChatMessage
                {
                    Id = messageId,
                    Body = new ItemBody { Content = messageContent, ContentType = BodyType.Text },
                    From = new ChatMessageFromIdentitySet 
                    { 
                        User = new Identity { DisplayName = "モックユーザー" } 
                    }
                };
            }
            
            // Delegated認証でメッセージを送信
            try
            {
                Console.WriteLine("📤 Delegated認証でメッセージを送信中...");
                var delegatedClient = await _authService.GetDelegatedClientAsync();
                
                var message = new ChatMessage
                {
                    Body = new ItemBody
                    {
                        Content = messageContent,
                        ContentType = BodyType.Text
                    }
                };

                var sentMessage = await delegatedClient.Teams[teamId].Channels[channelId].Messages.PostAsync(message);
                
                Console.WriteLine("✅ メッセージが正常に送信されました。");
                return sentMessage ?? throw new InvalidOperationException("送信されたメッセージが正しく返されませんでした");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ メッセージ送信に失敗しました: {ex.Message}");
                Console.WriteLine("\n💡 メッセージ送信を成功させるには、以下を実行してください:");
                Console.WriteLine("   1. Azure Portal > App registrations > 認証:");
                Console.WriteLine("      - リダイレクト URI: http://localhost:3000/auth/callback");
                Console.WriteLine("      - Publicクライアントフローを許可: はい");
                Console.WriteLine("   2. Azure Portal > API のアクセス許可:");
                Console.WriteLine("      - ChannelMessage.Send (Delegated)");
                Console.WriteLine("   3. Teams管理センターでアプリケーションを承認\n");
                throw new Exception($"メッセージ送信エラー: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 指定されたチャンネルのメッセージ一覧を取得します
        /// </summary>
        /// <param name="teamId">チームID</param>
        /// <param name="channelId">チャンネルID</param>
        /// <param name="top">取得するメッセージの最大数</param>
        /// <returns>ChatMessageオブジェクトのリスト</returns>
        public async Task<List<ChatMessage>> ListChannelMessagesAsync(string teamId, string channelId, int top = 10)
        {
            Console.WriteLine($"\n--- チームID: {teamId}, チャンネルID: {channelId} の最新{top}件のメッセージを取得します ---");
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(channelId))
            {
                Console.WriteLine("チームIDとチャンネルIDは空にできません。");
                return new List<ChatMessage>();
            }
            
            // モックデータを使用する場合
            if (_useLocalMockData && _sampleData?.Messages != null)
            {
                Console.WriteLine("API呼び出しの代わりにサンプルデータを使用します。");
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
                        Console.WriteLine($"メッセージID: {message.Id}, 送信者: {message.From?.User?.DisplayName ?? "不明"}, 内容: {message.Body?.Content}");
                    }

                    return messages;
                }
                else
                {
                    Console.WriteLine($"チームID {teamId} とチャンネルID {channelId} に対するサンプルメッセージが見つかりません");
                    return new List<ChatMessage>();
                }
            }
            
            // 実際のAPI呼び出しを行う場合
            try
            {
                var messages = await _graphClient.GetChannelMessagesAsync(teamId, channelId);

                if (messages != null && messages.Any())
                {
                    foreach (var message in messages)
                    {
                        Console.WriteLine($"メッセージID: {message.Id}, 送信者: {message.From?.User?.DisplayName ?? "不明"}, 内容: {message.Body?.Content}, 作成日時: {message.CreatedDateTime}");
                    }
                    return messages;
                }
                else
                {
                    Console.WriteLine("このチャンネルにメッセージが見つかりません。");
                    return new List<ChatMessage>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"チャンネルID {channelId} のメッセージ一覧取得エラー: {ex.Message}");
                Console.WriteLine($"エラー詳細: {ex}");
                return new List<ChatMessage>();
            }
        }
    }
}
