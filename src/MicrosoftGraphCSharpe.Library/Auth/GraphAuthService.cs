using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using System;

namespace MicrosoftGraphCSharpe.Library.Auth
{
    /// <summary>
    /// GraphAuthService - Microsoft Graph API認証サービス
    /// クライアント資格情報フロー（アプリケーション権限）を使用してMicrosoft Graph APIへの認証を行います。
    /// </summary>
    public class GraphAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly bool _isTestMode;
        private readonly GraphServiceClient _testClient;

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
        public GraphAuthService(IConfiguration configuration, GraphServiceClient testClient)
        {
            _configuration = configuration;
            _testClient = testClient;
            _isTestMode = true;
        }

        /// <summary>
        /// 認証済みのGraphServiceClientを取得します
        /// </summary>
        /// <returns>認証済みのGraphServiceClientインスタンス</returns>
        /// <exception cref="System.Exception">認証情報が設定ファイルに存在しない場合にスローされます</exception>
        public GraphServiceClient GetAuthenticatedGraphClient()
        {
            // テストモードの場合、事前に設定されたクライアントを返す
            if (_isTestMode)
            {
                return _testClient;
            }
            
            var clientId = _configuration["GraphApi:ClientId"];
            var clientSecret = _configuration["GraphApi:ClientSecret"];
            var tenantId = _configuration["GraphApi:TenantId"];

            if (string.IsNullOrEmpty(clientId) || 
                string.IsNullOrEmpty(clientSecret) || 
                string.IsNullOrEmpty(tenantId))
            {
                throw new System.Exception("Azure AD アプリケーション登録の詳細情報（ClientId、ClientSecret、TenantId）が'GraphApi'セクション内に見つからないか空です。");
            }

            // 実際の認証処理
            try
            {
                var clientSecretCredential = new ClientSecretCredential(
                    tenantId, clientId, clientSecret);
                
                // Azure SDKのログ記録が必要な場合は、ここで設定できます。
                // 例: Azure.Core.Diagnostics.AzureSdkEventSourceListener.CreateConsoleLogger(System.Diagnostics.Tracing.EventLevel.LogAlways);

                var graphClient = new GraphServiceClient(clientSecretCredential);
                return graphClient;
            }
            catch (Exception ex)
            {
                throw new System.Exception($"Graph認証エラー: {ex.Message}", ex);
            }
        }
    }
}
