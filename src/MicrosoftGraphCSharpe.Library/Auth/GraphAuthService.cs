using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace MicrosoftGraphCSharpe.Library.Auth
{
    /// <summary>
    /// 認証タイプを定義
    /// </summary>
    public enum AuthType
    {
        Application,
        Delegated
    }

    /// <summary>
    /// GraphAuthService - Microsoft Graph API認証サービス
    /// Application認証とDelegated認証の両方をサポートし、操作に応じて自動的に切り替えます。
    /// </summary>
    public class GraphAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly bool _isTestMode;
        private readonly GraphServiceClient? _testClient;
        
        // シングルトンインスタンス管理
        private GraphServiceClient? _applicationClient;
        private GraphServiceClient? _delegatedClient;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="configuration">設定情報を提供するIConfigurationインスタンス</param>
        public GraphAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            _isTestMode = false;
        }
        
        /// <summary>
        /// テスト用コンストラクタ
        /// </summary>
        /// <param name="configuration">設定情報を提供するIConfigurationインスタンス</param>
        /// <param name="testClient">テスト用のGraphClientインスタンス</param>
        public GraphAuthService(IConfiguration configuration, GraphServiceClient? testClient)
        {
            _configuration = configuration;
            _testClient = testClient;
            _isTestMode = true;
        }

        /// <summary>
        /// Application認証クライアントを取得（読み取り操作用）
        /// Client Credential Flowを使用します
        /// </summary>
        /// <returns>認証済みのGraphServiceClientインスタンス</returns>
        /// <exception cref="System.Exception">認証情報が設定ファイルに存在しない場合にスローされます</exception>
        public async Task<GraphServiceClient> GetApplicationClientAsync()
        {
            // テストモードの場合、事前に設定されたクライアントを返す
            if (_isTestMode)
            {
                return _testClient ?? throw new InvalidOperationException("テストクライアントが設定されていません");
            }

            if (_applicationClient == null)
            {
                Console.WriteLine("🔧 Application認証クライアントを初期化しています...");
                _applicationClient = await CreateApplicationClientAsync();
                Console.WriteLine("✅ Application認証クライアントの初期化が完了しました。");
            }
            
            return _applicationClient;
        }

        /// <summary>
        /// Delegated認証クライアントを取得（メッセージ送信用）
        /// Device Code Flowを使用します
        /// </summary>
        /// <returns>認証済みのGraphServiceClientインスタンス</returns>
        /// <exception cref="System.Exception">認証情報が設定ファイルに存在しない場合にスローされます</exception>
        public async Task<GraphServiceClient> GetDelegatedClientAsync()
        {
            // テストモードの場合、事前に設定されたクライアントを返す
            if (_isTestMode)
            {
                return _testClient ?? throw new InvalidOperationException("テストクライアントが設定されていません");
            }

            if (_delegatedClient == null)
            {
                Console.WriteLine("🔧 Delegated認証クライアントを初期化しています...");
                _delegatedClient = await CreateDelegatedClientAsync();
                Console.WriteLine("✅ Delegated認証クライアントの初期化が完了しました。");
            }
            
            return _delegatedClient;
        }

        /// <summary>
        /// Application認証クライアントを作成
        /// </summary>
        private async Task<GraphServiceClient> CreateApplicationClientAsync()
        {
            var clientId = _configuration["GraphApi:ClientId"];
            var clientSecret = _configuration["GraphApi:ClientSecret"];
            var tenantId = _configuration["GraphApi:TenantId"];

            if (string.IsNullOrEmpty(clientId) || 
                string.IsNullOrEmpty(clientSecret) || 
                string.IsNullOrEmpty(tenantId))
            {
                throw new System.Exception("Azure AD アプリケーション登録の詳細情報（ClientId、ClientSecret、TenantId）が'GraphApi'セクション内に見つからないか空です。");
            }

            try
            {
                var clientSecretCredential = new ClientSecretCredential(
                    tenantId, clientId, clientSecret);
                
                var graphClient = new GraphServiceClient(clientSecretCredential);
                return await Task.FromResult(graphClient);
            }
            catch (Exception ex)
            {
                throw new System.Exception($"Application認証エラー: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delegated認証クライアントを作成
        /// Device Code Flowを使用（Azure ADアプリでPublicクライアントフローが有効である必要があります）
        /// </summary>
        private async Task<GraphServiceClient> CreateDelegatedClientAsync()
        {
            var clientId = _configuration["GraphApi:ClientId"];
            var tenantId = _configuration["GraphApi:TenantId"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId))
            {
                throw new System.Exception("Azure AD アプリケーション登録の詳細情報（ClientId、TenantId）が'GraphApi'セクション内に見つからないか空です。");
            }

            try
            {
                Console.WriteLine("🔧 Device Code Flow認証を設定中...");
                Console.WriteLine("⚠️ Azure AD アプリケーションでPublicクライアントフローが有効になっている必要があります。");
                Console.WriteLine("   Azure Portal > App registrations > 認証 > 詳細設定 > Publicクライアントフローを許可する = はい");

                var deviceCodeCredential = new DeviceCodeCredential(
                    tenantId: tenantId,
                    clientId: clientId,
                    deviceCodeCallback: (code, cancellation) =>
                    {
                        DisplayDeviceCodeInstructions(code);
                        return Task.CompletedTask;
                    });

                var graphClient = new GraphServiceClient(deviceCodeCredential, 
                    new[]
                    {
                        "https://graph.microsoft.com/Team.ReadBasic.All",
                        "https://graph.microsoft.com/Channel.ReadBasic.All",
                        "https://graph.microsoft.com/ChannelMessage.Send",
                        "https://graph.microsoft.com/ChannelMessage.Read.All",
                        "https://graph.microsoft.com/User.Read"
                    });

                return await Task.FromResult(graphClient);
            }
            catch (Exception ex)
            {
                throw new System.Exception($"Delegated認証エラー: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Device Code認証の指示を表示
        /// </summary>
        private void DisplayDeviceCodeInstructions(Azure.Identity.DeviceCodeInfo deviceCodeInfo)
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("🔐           ユーザー認証が必要です               🔐");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("");
            Console.WriteLine("📋 認証手順:");
            Console.WriteLine("   1. ブラウザで以下のURLにアクセスしてください:");
            Console.WriteLine($"      📱 {deviceCodeInfo.VerificationUri}");
            Console.WriteLine("");
            Console.WriteLine("   2. 表示される画面で以下のコードを入力してください:");
            Console.WriteLine($"      🔑 {deviceCodeInfo.UserCode}");
            Console.WriteLine("");
            Console.WriteLine("   3. 認証完了まで少々お待ちください...");
            Console.WriteLine("");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("");
            Console.WriteLine("💡 認証に失敗する場合は、Azure ADアプリの設定を確認してください：");
            Console.WriteLine("   • Azure Portal > Azure Active Directory > App registrations");
            Console.WriteLine($"   • アプリ \"{_configuration["GraphApi:ClientId"]}\" を選択");
            Console.WriteLine("   • 認証 > 詳細設定 > \"パブリック クライアント フローを許可する\" を \"はい\" に設定");
            Console.WriteLine("   • API のアクセス許可でDelegatedアクセス許可が正しく設定されていることを確認");
            Console.WriteLine("");
            Console.WriteLine("⏳ 認証完了をお待ちしています...");
            Console.WriteLine("");
        }

        /// <summary>
        /// 認証クライアントをリセット（テスト用など）
        /// </summary>
        public void Reset()
        {
            _applicationClient = null;
            _delegatedClient = null;
        }

        /// <summary>
        /// 認証済みのGraphServiceClientを取得します（後方互換性のため）
        /// </summary>
        /// <returns>認証済みのGraphServiceClientインスタンス</returns>
        /// <exception cref="System.Exception">認証情報が設定ファイルに存在しない場合にスローされます</exception>
        [Obsolete("GetApplicationClientAsync() を使用してください")]
        public GraphServiceClient GetAuthenticatedGraphClient()
        {
            Console.WriteLine("⚠️ GetAuthenticatedGraphClient() は非推奨です。GetApplicationClientAsync() を使用してください。");
            return GetApplicationClientAsync().Result;
        }
    }
}
