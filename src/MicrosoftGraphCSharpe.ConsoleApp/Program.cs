using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using MicrosoftGraphCSharpe.Library.Auth;
using MicrosoftGraphCSharpe.Library.Services;
using System.IO;

/// <summary>
/// Microsoft Graph API を使用した Teams 操作のコンソールアプリケーションのメインプログラム
/// </summary>
class Program
{
    /// <summary>
    /// アプリケーションのエントリーポイント
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    static async Task Main(string[] args)
    {
        // プロセス終了時のクリーンアップ
        Console.CancelKeyPress += (sender, e) => {
            Console.WriteLine("\n\n🛑 アプリケーションを終了しています...");
            e.Cancel = false;
        };

        var host = CreateHostBuilder(args).Build();

        var teamsService = host.Services.GetRequiredService<TeamsService>();
        if (teamsService == null)
        {
            Console.WriteLine("エラー: TeamsService を読み込めませんでした。");
            return;
        }

        try
        {
            Console.WriteLine("Teams の一覧を取得しています...");
            var teams = await teamsService.ListMyTeamsAsync();
            if (teams == null || !teams.Any())
            {
                Console.WriteLine("Teams が見つからないか、一覧取得でエラーが発生しました。");
                return;
            }

            var firstTeam = teams.First();
            Console.WriteLine($"最初のチーム: {firstTeam.DisplayName} (ID: {firstTeam.Id})");

            if (string.IsNullOrEmpty(firstTeam.Id))
            {
                Console.WriteLine("最初のチーム ID が null または空です。続行できません。");
                return;
            }

            Console.WriteLine($"チーム {firstTeam.DisplayName} のチャンネル一覧を取得しています...");
            var channels = await teamsService.ListChannelsAsync(firstTeam.Id);
            if (channels == null || !channels.Any())
            {
                Console.WriteLine($"チーム {firstTeam.DisplayName} にチャンネルが見つかりません。");
                return;
            }

            var firstChannel = channels.First();
            Console.WriteLine($"最初のチャンネル: {firstChannel.DisplayName} (ID: {firstChannel.Id})");
            
            if (string.IsNullOrEmpty(firstChannel.Id))
            {
                Console.WriteLine("最初のチャンネル ID が null または空です。続行できません。");
                return;
            }

            var messageContent = "C# Graph API アプリからこんにちは！";
            Console.WriteLine($"メッセージ '{messageContent}' をチャンネル {firstChannel.DisplayName} に送信しています...");
            var sentMessage = await teamsService.SendMessageToChannelAsync(firstTeam.Id, firstChannel.Id, messageContent);
            if (sentMessage != null)
            {
                Console.WriteLine($"メッセージが送信されました。メッセージ ID: {sentMessage.Id}");
            }
            else
            {
                Console.WriteLine("メッセージの送信に失敗しました。");
            }

            // 対話的メッセージ送信機能
            Console.WriteLine($"\n--- 対話的メッセージ送信 ---");
            await InteractiveMessageSending(teamsService, firstTeam.Id, firstChannel.Id, firstChannel.DisplayName ?? "不明なチャンネル");

            Console.WriteLine($"\nチャンネル {firstChannel.DisplayName} のメッセージ一覧を取得しています...");
            var messages = await teamsService.ListChannelMessagesAsync(firstTeam.Id, firstChannel.Id);
            if (messages != null && messages.Any())
            {
                Console.WriteLine($"{messages.Count()} 件のメッセージが見つかりました:");
                foreach (var msg in messages)
                {
                    Console.WriteLine($"- {msg.Body?.Content} (送信者: {msg.From?.User?.DisplayName ?? "不明"})");
                }
            }
            else
            {
                Console.WriteLine("メッセージが見つからないか、一覧取得でエラーが発生しました。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"エラーが発生しました: {ex.Message}");
            Console.WriteLine($"詳細: {ex}");
        }
    }

    /// <summary>
    /// 対話的メッセージ送信機能
    /// ユーザーからの入力を受け取ってメッセージを送信します
    /// </summary>
    /// <param name="teamsService">TeamsServiceインスタンス</param>
    /// <param name="teamId">チームID</param>
    /// <param name="channelId">チャンネルID</param>
    /// <param name="channelName">チャンネル名</param>
    static async Task InteractiveMessageSending(TeamsService teamsService, string teamId, string channelId, string channelName)
    {
        Console.WriteLine("📝 メッセージ送信機能を開始します。");
        Console.WriteLine("   \"exit\" または \"quit\" と入力すると終了します。");
        Console.WriteLine("   空白行を入力すると送信をスキップします。\n");

        while (true)
        {
            try
            {
                Console.Write($"💬 {channelName} に送信するメッセージを入力してください: ");
                var message = Console.ReadLine();
                
                // 終了コマンドをチェック
                if (string.IsNullOrEmpty(message) || 
                    message.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                    message.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("👋 メッセージ送信機能を終了します。");
                    break;
                }
                
                // 空白メッセージをスキップ
                if (string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine("⚠️  空のメッセージはスキップされました。\n");
                    continue;
                }
                
                // メッセージを送信
                Console.WriteLine($"\n📤 メッセージを送信中: \"{message}\"");
                var sentMessage = await teamsService.SendMessageToChannelAsync(teamId, channelId, message);
                
                if (sentMessage != null)
                {
                    Console.WriteLine("✅ メッセージが正常に送信されました。");
                }
                else
                {
                    Console.WriteLine("❌ メッセージの送信に失敗しました。");
                }
                
                Console.WriteLine("");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ メッセージ送信中にエラーが発生しました: {ex.Message}");
                Console.WriteLine("🔄 次のメッセージを入力してください。\n");
            }
        }
    }

    /// <summary>
    /// ホストビルダーを作成し、サービスと構成を設定します
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    /// <returns>設定済みのIHostBuilder</returns>
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // 実行環境に応じたベースパスの設定
                string basePath;
                
                // Dockerコンテナ内で実行されているかどうかを判定
                if (Directory.Exists("/app"))
                {
                    // Dockerコンテナ内
                    basePath = "/app";
                }
                else
                {
                    // ローカル環境またはその他
                    basePath = AppDomain.CurrentDomain.BaseDirectory;
                }
                
                Console.WriteLine($"[DEBUG] 構成ファイルのベースパス: {basePath}");
                
                // カレントディレクトリのファイル一覧を表示（デバッグ用）
                Console.WriteLine("[DEBUG] ディレクトリ内のファイル:");
                foreach (var file in Directory.GetFiles(basePath))
                {
                    Console.WriteLine($"  - {Path.GetFileName(file)}");
                }
                
                // 設定ファイルが見つからない場合も動作するよう修正
                try
                {
                    config.SetBasePath(basePath);
                
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] 設定ファイルの読み込み中にエラーが発生しました: {ex.Message}");
                    Console.WriteLine("[INFO] デフォルト設定を使用します。");
                    
                    // エラーが発生した場合でも動作するよう環境変数から設定を読み込む
                    config.AddEnvironmentVariables();
                }
                
                // デバッグ用: 設定ファイルが見つかるかどうか確認
                string devSettingsPath = Path.Combine(basePath, $"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json");
                Console.WriteLine($"[DEBUG] 環境設定ファイルのパス: {devSettingsPath}");
                Console.WriteLine($"[DEBUG] 環境設定ファイルの存在: {File.Exists(devSettingsPath)}");
                Console.WriteLine($"[DEBUG] 現在の環境: {hostingContext.HostingEnvironment.EnvironmentName}");

                // さらに詳細なコンテキストのために、ディレクトリ内のファイル一覧を表示
                Console.WriteLine("[DEBUG] ディレクトリ内のファイル:");
                try
                {
                    foreach (var file in Directory.GetFiles(basePath))
                    {
                        Console.WriteLine($"  - {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] ファイル一覧取得エラー: {ex.Message}");
                }

                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<GraphAuthService>();
                services.AddSingleton<TeamsService>(provider => {
                    var authService = provider.GetRequiredService<GraphAuthService>();
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    // 新しいコンストラクタを使用（Application認証を自動取得）
                    var appClient = authService.GetApplicationClientAsync().Result;
                    return new TeamsService(appClient, configuration);
                });
            });
}
