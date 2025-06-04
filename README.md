# MicrosoftGraphCSharpe

Microsoft Graph APIを使用してC#でTeamsを操作するサンプルアプリケーション

## 概要

このプロジェクトはMicrosoft Graph APIを活用してMicrosoft Teamsを操作するサンプル実装です。
環境変数による柔軟な制御機能により、開発環境でのモックデータと本番環境での実際のAPI使用を簡単に切り替えることができます。

### 主な機能

- **Azure Active Directory認証**
  - Application認証（クライアント資格情報フロー）
  - Delegated認証（Device Code Flow）- メッセージ送信時
- **Teams操作**
  - Teamsの一覧取得
  - チャンネルの一覧取得
  - チャンネルへのメッセージ送信（対話型）
  - チャンネルメッセージの一覧取得
- **開発サポート**
  - モックデータによる開発・テスト環境
  - 環境変数による動作制御
  - 包括的な単体テスト

## 技術仕様

- **.NET 8.0** - 安定したパフォーマンスと互換性
- **Microsoft.Graph** - 最新のGraph API SDK
- **Azure.Identity** 1.14.0 - セキュリティ脆弱性対応済み
- **MSTest** - 包括的なテストフレームワーク

## 必要な環境

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) （.NET 9.0ではなく8.0を使用）
- Visual Studio 2022、Visual Studio Code、またはその他の.NET開発環境
- Azure Active Directory テナント
- Microsoft Teamsが有効化されたテナント
- Microsoft Graph APIにアクセスするためのAzure ADアプリケーション登録

## プロジェクト構成

- `MicrosoftGraphCSharpe.ConsoleApp`: コンソールアプリケーション
- `MicrosoftGraphCSharpe.Library`: Graph APIを使用するライブラリ
- `MicrosoftGraphCSharpe.Tests`: 単体テスト

## 事前準備

### Azure ADアプリケーション登録

1. [Azure Portal](https://portal.azure.com)にアクセスし、「アプリの登録」でアプリケーションを登録します
2. 以下のMicrosoft Graph APIの**アプリケーション権限**を追加：
   - `Team.ReadBasic.All`（チームの基本情報を読み取る）
   - `TeamSettings.Read.All`（チーム設定を読み取る）
   - `ChannelMessage.Read.All`（チャンネルメッセージを読み取る）
3. 以下のMicrosoft Graph APIの**委任されたアクセス許可**を追加：
   - `ChannelMessage.Send`（チャンネルメッセージを送信する）
   - `User.Read`（ユーザー情報の読み取り）
4. **認証** > **詳細設定** > **パブリック クライアント フローを許可する** を **はい** に設定
5. クライアントシークレットを作成し、値をメモしておきます
6. 管理者の同意を付与します

## 設定

環境設定ファイル(`appsettings.Development.json`)を作成し、以下の情報を設定します。
サンプルとして`appsettings.Development.json.example`を参照してください。

```json
{
  "GraphApi": {
    "TenantId": "あなたのテナントID",
    "ClientId": "アプリケーションのクライアントID",
    "ClientSecret": "アプリケーションのクライアントシークレット"
  },
  "UseLocalMockData": false
}
```

実際のTeams環境がない場合は、`UseLocalMockData`を`true`に設定することでモックデータを使用できます。
モックデータは`appsettings.json`の`SampleData`セクションで設定できます。

### 環境変数による制御（推奨）

設定ファイルを編集せずに、環境変数でモックデータの使用を制御することができます：

```bash
# モックデータを使用する場合（開発・テスト用）
USE_MOCK_DATA=true dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp

# 実際のGraph APIを使用する場合（統合テスト・本番用）
USE_MOCK_DATA=false dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp
```

**重要**: 環境変数 `USE_MOCK_DATA` は設定ファイルの `UseLocalMockData` よりも優先されます。

## ローカル環境での実行方法

### コマンドライン

```bash
# プロジェクトディレクトリに移動
cd /Users/niya/Documents/MicrosoftGraphCSharpe

# 依存関係の復元とアプリケーションのビルド
dotnet restore
dotnet build

# 🚀 開発モード: モックデータで迅速な開発・テスト
USE_MOCK_DATA=true DOTNET_ENVIRONMENT=Development dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp

# 🔗 統合テスト: 実際のMicrosoft Graph APIで動作確認
USE_MOCK_DATA=false DOTNET_ENVIRONMENT=Development dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp

# 📁 設定ファイル依存: 環境変数なしで設定ファイルの値を使用
DOTNET_ENVIRONMENT=Development dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp

# 🏭 本番環境: 設定ファイルまたは環境変数に依存
dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp
```

**💡 開発ワークフロー:**
1. **開発段階**: `USE_MOCK_DATA=true` でモックデータを使用して機能開発
2. **統合テスト**: `USE_MOCK_DATA=false` で実際のAPIとの動作確認
3. **本番デプロイ**: 環境変数または設定ファイルで制御

**⚠️ 注意:** 
- `USE_MOCK_DATA=true`: モックデータ使用、Azure AD設定不要
- `USE_MOCK_DATA=false`: 実際のMicrosoft Graph API使用、Azure AD設定必要
- 環境変数なし: 設定ファイルの `UseLocalMockData` 値に従う

### Visual Studio Code

1. Visual Studio Codeでプロジェクトフォルダを開く
2. .NET Core Launch (.NET) 構成を使用して実行/デバッグ

## テスト

全7件のテストが含まれており、すべて成功することを確認済みです。

```bash
# テストの実行
dotnet test

# 出力例:
# 成功!   -失敗:     0、合格:     7、スキップ:     0、合計:     7
```

### テスト内容
- **GraphAuthService**: Azure AD認証サービスのテスト
- **TeamsService**: Teams操作サービスのテスト（モックデータ使用）
- **設定読み込み**: 環境変数と設定ファイルの優先順位テスト

## Docker環境での実行

Dockerfileを使用して、コンテナ環境でアプリケーションを実行することも可能です。モックデータを使用することで、実際のTeams環境がなくてもアプリケーションの動作確認ができます。

```bash
# Dockerイメージのビルド
docker build -t microsoftgraphcsharpe .

# 対話モードでコンテナ実行（結果を直接確認）
docker run -it --rm microsoftgraphcsharpe

# バックグラウンドでコンテナ実行（ログを確認）
docker run -d --name msgraph-app microsoftgraphcsharpe
docker logs -f msgraph-app

# 全てのテストを実行（モックデータを使用）
docker build -t msgraph-test --target build .
docker run --rm msgraph-test dotnet test
```

Dockerコンテナ内では`appsettings.Development.json`の設定が使用され、`DOTNET_ENVIRONMENT=Development`環境変数により開発環境用の設定が適用されます。`UseLocalMockData=true`によりモックデータが使用されます。

## トラブルシューティング

### よくある問題と解決方法

#### 1. アプリケーション実行時のエラー

**問題:** `dotnet run` 実行時に「Azure AD アプリケーション登録の詳細情報が見つからない」エラーが発生する

**解決方法:**
```bash
# 開発環境として実行する（モックデータを使用）
USE_MOCK_DATA=true DOTNET_ENVIRONMENT=Development dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp

# または、設定ファイルでモックデータを有効にして実行
DOTNET_ENVIRONMENT=Development dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp
```

**原因:** 環境変数が設定されていない場合、アプリケーションは本番環境として動作し、実際のAzure AD設定を要求します。

#### 2. セキュリティ関連のエラー

**問題:** Azure.Identity関連のセキュリティ警告やCVE-2024-35255エラー

**解決方法:** Azure.Identityパッケージを1.14.0に更新済みです。以下で確認可能：
```bash
dotnet list package | grep Azure.Identity
# Azure.Identity 1.14.0 が表示されることを確認
```

#### 3. .NET バージョンの問題

**問題:** .NET 9.0関連のエラーや互換性問題

**解決方法:** このプロジェクトは.NET 8.0で動作するよう設定されています：
```bash
dotnet --version
# 8.0.404 が表示されることを確認
```

#### 4. Device Code Flow認証の問題

**問題:** メッセージ送信時の認証に失敗する

**解決方法:** Azure ADアプリの設定を確認：
1. Azure Portal > Azure Active Directory > App registrations
2. 対象アプリを選択
3. **認証** > **詳細設定** > **パブリック クライアント フローを許可する** を **はい** に設定
4. **API のアクセス許可** で委任されたアクセス許可が正しく設定されていることを確認

#### 5. APIエンドポイントエラー

**問題:** Teams一覧取得時に「Forbidden」や「Unauthorized」エラー

**解決方法:** このプロジェクトではApplication認証用に `/teams` エンドポイントを使用するよう修正済みです。
- ❌ 旧: `/me/joinedTeams`（Delegated認証のみ）
- ✅ 新: `/teams`（Application認証対応）

### 環境別の動作

- **Development環境** (`DOTNET_ENVIRONMENT=Development`): 
  - 設定ファイルまたは環境変数に基づいて動作
  - `appsettings.Development.json` の設定を使用
  - デバッグ情報の詳細出力
- **Production環境** (デフォルト): 
  - 実際のMicrosoft Graph API使用
  - Azure AD設定必須
  - 本番レベルのログ出力

### 環境変数による制御

| 環境変数 | 値 | 動作 | Azure AD設定 |
|---------|---|------|-------------|
| `USE_MOCK_DATA=true` | true | モックデータを使用 | 不要 |
| `USE_MOCK_DATA=false` | false | 実際のMicrosoft Graph APIを使用 | 必要 |
| 環境変数なし | - | 設定ファイルの `UseLocalMockData` 値に従う | 設定による |

## プロジェクトの改善履歴

### 🔧 セキュリティ強化
- Azure.Identity パッケージを1.11.0から1.14.0に更新（CVE-2024-35255対応）
- 非推奨のテストメソッド修正（`ExpectedException` → `Assert.ThrowsException`）

### 🎯 API互換性向上
- Teams一覧取得のAPIエンドポイントを修正
  - 変更前: `/me/joinedTeams`（Delegated認証のみ）
  - 変更後: `/teams`（Application認証対応）

### 🔄 開発ワークフロー改善
- 環境変数による動作制御機能を追加
- モックデータと実際のAPIの簡単な切り替え
- 設定ファイルよりも環境変数を優先する制御ロジック

### ✅ 安定性向上
- .NET 8.0での動作を保証（.NET 9.0の問題を回避）
- 全7件のテストが成功することを確認
- 実際のMicrosoft Teamsとの動作確認済み

## 注意事項

### セキュリティ
- プロダクション環境で使用する場合は、クライアントシークレットなどの機密情報の管理方法を適切に見直してください
- Azure Key VaultやAzure App Configurationなどのセキュアな設定管理サービスの使用を推奨します
- APIアクセス権限は必要最小限にするよう設計してください

### パフォーマンス
- 大量のTeamsやチャンネルを扱う場合は、ページネーション機能の実装を検討してください
- API呼び出し頻度にはMicrosoft Graphのスロットリング制限があります

### 開発
- このプロジェクトは.NET 8.0で動作するよう最適化されています
- 新しい機能追加時は、テストケースの追加も併せて行ってください
- モックデータは `appsettings.json` の `SampleData` セクションで管理されています

## 参考資料

- [Microsoft Graph API ドキュメント](https://docs.microsoft.com/ja-jp/graph/)
- [Azure Active Directory アプリ登録ガイド](https://docs.microsoft.com/ja-jp/azure/active-directory/develop/quickstart-register-app)
- [.NET 8.0 ドキュメント](https://docs.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-8)
