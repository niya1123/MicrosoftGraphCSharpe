# Microsoft Graph C# Teams アプリ

このプロジェクトは、C# と Microsoft Graph SDK を使用して Microsoft Teams と対話する方法を示します。自動認証切り替え機能により、読み取り操作には Application 認証、メッセージ送信には Delegated 認証を自動的に選択します。

## 主な特徴

• 🔄 **自動認証切り替え**: 操作に応じて Application 認証と Delegated 認証を自動選択  
• 📖 **読み取り操作**: チーム一覧、チャンネル一覧、メッセージ一覧 (Application 認証)  
• 📝 **メッセージ送信**: インタラクティブなメッセージ送信機能 (Delegated 認証)  
• 🔧 **設定不要**: 一度の設定で両方の認証モードが利用可能  
• ✅ **完全テスト**: 全機能のユニットテスト実装済み  
• 🎯 **モック対応**: 環境変数による開発・本番環境の簡単切り替え

## 認証アーキテクチャ

このアプリケーションは、Microsoft Graph API の制約に対応するため、2つの認証方式を自動的に切り替えます：

### Application 認証 (Client Credential Flow)
• **用途**: 読み取り操作 (チーム、チャンネル、メッセージ一覧の取得)  
• **特徴**: ユーザー操作不要、自動実行可能  
• **アクセス許可**: アプリケーション権限  

### Delegated 認証 (Device Code Flow)
• **用途**: メッセージ送信操作  
• **特徴**: ユーザー認証が必要、ブラウザでの認証フロー  
• **アクセス許可**: 委任されたアクセス許可  

## 前提条件

• **.NET 8.0 SDK** (推奨) - [ダウンロード](https://dotnet.microsoft.com/download/dotnet/8.0)  
• **Visual Studio 2022、Visual Studio Code**、またはその他の.NET開発環境  
• **Docker** (Docker ベースの実行用)  
• **Microsoft Graph の必要なアクセス許可を持つ Azure AD アプリケーション登録** - [Azure ADアプリケーション登録方法](https://learn.microsoft.com/ja-jp/graph/auth-register-app-v2)

### 必要なアクセス許可

Azure AD アプリケーション登録で以下のアクセス許可を設定してください：

#### アプリケーション権限 (Application 認証用)
• `Team.ReadBasic.All` - チーム情報の読み取り  
• `TeamSettings.Read.All` - チーム設定の読み取り  
• `ChannelMessage.Read.All` - チャンネルメッセージの読み取り  

#### 委任されたアクセス許可 (Delegated 認証用)
• `ChannelMessage.Send` - チャンネルメッセージの送信  
• `User.Read` - ユーザー情報の読み取り

### 重要な設定
1. [Azure Portal](https://portal.azure.com) > **アプリの登録** > 対象アプリ選択
2. **認証** > **詳細設定** > **パブリック クライアント フローを許可する** を **はい** に設定
3. **クライアントシークレット**を作成し、値をメモ
4. **管理者の同意**を付与

## プロジェクト構成

```
MicrosoftGraphCSharpe/
├── src/
│   ├── MicrosoftGraphCSharpe.ConsoleApp/     # コンソールアプリケーション
│   │   ├── Program.cs                        # メインエントリポイント
│   │   └── MicrosoftGraphCSharpe.ConsoleApp.csproj
│   └── MicrosoftGraphCSharpe.Library/        # Graph API ライブラリ
│       ├── Auth/
│       │   └── GraphAuthService.cs          # Microsoft Graph 認証ロジック
│       ├── Services/
│       │   ├── TeamsService.cs              # Teams操作サービス
│       │   ├── GraphClientWrapper.cs        # Graph API ラッパー
│       │   └── IGraphClientWrapper.cs       # インターフェース
│       ├── Models/
│       │   └── MockData.cs                  # モックデータ定義
│       └── MicrosoftGraphCSharpe.Library.csproj
├── tests/
│   └── MicrosoftGraphCSharpe.Tests/          # ユニットテスト
│       ├── GraphAuthServiceTests.cs         # 認証機能テスト
│       ├── TeamsServiceTests.cs             # Teams操作機能テスト
│       └── MicrosoftGraphCSharpe.Tests.csproj
├── appsettings.json                          # 基本設定とモックデータ
├── appsettings.Development.json              # 開発環境設定
├── appsettings.Development.json.example      # 設定ファイル例
├── Dockerfile                               # Docker設定
├── docker-compose.yml                       # Docker Compose設定
├── AZURE_SETUP_GUIDE.md                     # Azure AD詳細設定手順
├── QUICK_FIX_GUIDE.md                       # 一般的なエラー解決ガイド
├── AZURE_PERMISSION_FIX.md                  # API アクセス許可修正手順
└── README.md
```

## セットアップ

1. リポジトリをクローンする（該当する場合）か、プロジェクトファイルを作成します
2. 依存関係をインストールします:
   ```bash
   dotnet restore
   ```
3. プロジェクトのルートに `appsettings.Development.json.example` をコピーして `appsettings.Development.json` ファイルを作成します:
   ```bash
   cp appsettings.Development.json.example appsettings.Development.json
   ```
4. `appsettings.Development.json` ファイルに Azure AD アプリケーションの詳細を記入します:
   • `ClientId`: Azure AD アプリケーション (クライアント) ID
   • `ClientSecret`: Azure AD アプリケーションのクライアントシークレット
   • `TenantId`: Azure AD ディレクトリ (テナント) ID
   • `TargetTeamId` (任意): 操作に使用するデフォルトのチーム ID
   • `TargetChannelId` (任意): 操作に使用するデフォルトのチャネル ID

### 追加ドキュメントと診断ツール

Azure AD 設定や権限の確認には、以下のドキュメントが役立ちます：

• [AZURE_SETUP_GUIDE.md](./AZURE_SETUP_GUIDE.md) – メッセージ送信を有効にするための詳細な Azure AD 設定手順  
• [QUICK_FIX_GUIDE.md](./QUICK_FIX_GUIDE.md) – `AADSTS7000218` などの一般的な認証エラーを素早く解決するためのガイド  
• [AZURE_PERMISSION_FIX.md](./AZURE_PERMISSION_FIX.md) – API アクセス許可設定を修正するための手順

### Team ID と Channel ID の取得方法

Microsoft Teams UIからTeam IDとChannel IDを取得する方法：

#### 1. Team ID の取得
• Microsoft Teams でチームを開く  
• ブラウザのアドレスバーのURLを確認  
• URLに含まれる `groupId=` パラメータの値がTeam ID  
• 例: `https://teams.microsoft.com/_#/teamDashboard/General?groupId=a536b7f7-b65b-431a-b71e-cd386882d3e6`  
• この場合のTeam ID: `a536b7f7-b65b-431a-b71e-cd386882d3e6`

#### 2. Channel ID の取得
• Microsoft Teams でチャンネルを開く  
• ブラウザのアドレスバーのURLを確認  
• URLに含まれる `threadId=` パラメータの値がChannel ID（URL エンコードされている）  
• 例: `https://teams.microsoft.com/_#/channel/19%3Ab4cff4a9964b42dca8f2de52042dd340%40thread.tacv2/General?groupId=...&threadId=19%3Ab4cff4a9964b42dca8f2de52042dd340%40thread.tacv2`  
• **重要**: URLデコードが必要  
  - エンコード済み: `19%3Ab4cff4a9964b42dca8f2de52042dd340%40thread.tacv2`  
  - デコード後: `19:b4cff4a9964b42dca8f2de52042dd340@thread.tacv2`  
• `appsettings.Development.json` ファイルにはデコード後の値を使用

## 🚀 自動認証切り替え機能

このアプリケーションの最大の特徴は、操作に応じて最適な認証方式を自動選択することです：

### 読み取り操作 (Application 認証)
• `listMyTeams()` - チーム一覧取得  
• `listChannels(teamId)` - チャンネル一覧取得  
• `listChannelMessages(teamId, channelId)` - メッセージ一覧取得  

これらの操作は **ユーザー操作なし** で実行されます。

### メッセージ送信 (Delegated 認証)
• `sendMessageToChannel(teamId, channelId, message)` - メッセージ送信  

この操作時には自動的に Device Code Flow が起動し、ユーザー認証を求められます：

```
🔐 ユーザー認証が必要です:
   ブラウザで以下のURLにアクセスしてください: https://microsoft.com/devicelogin
   表示される画面で以下のコードを入力してください: ABC123456
   認証完了まで少々お待ちください...
```

## メッセージ送信機能

### インタラクティブメッセージ送信
アプリケーション実行時に、対話型のメッセージ送信機能が利用できます：

• **コンソール入力**: ユーザーがコンソールからメッセージを入力  
• **自動認証**: メッセージ送信時に Delegated 認証を自動起動  
• **リアルタイム送信**: 入力されたメッセージを即座にTeamsチャンネルに送信  
• **終了コマンド**: `exit`または`quit`で機能を終了  
• **エラーハンドリング**: 送信失敗時の適切なエラー処理  

### 実装内容
```csharp
// メッセージ送信 (認証は自動選択)
await sendMessageToChannel(teamId, channelId, message);
```

主な機能：
1. 自動 Delegated 認証クライアント取得
2. 空のメッセージコンテンツの検証
3. プレーンテキスト形式でのメッセージ送信
4. 送信成功・失敗の視覚的フィードバック（絵文字付き）

### 使用方法
1. アプリケーションを実行すると、まず Application 認証で読み取り操作が実行されます
2. インタラクティブモードで "メッセージを入力してください (exit/quitで終了): " プロンプトが表示されます
3. 初回メッセージ送信時に Device Code Flow による認証が自動で開始されます
4. ブラウザで認証を完了すると、メッセージがTeamsチャンネルに送信されます
5. 2回目以降の送信では認証は不要です（トークンキャッシュ済み）
6. `exit`または`quit`を入力すると機能が終了します

## ローカル開発

1. **C# コードをビルドします**:
   ```bash
   dotnet build
   ```

2. **アプリケーションを実行します**:
   ```bash
   dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp
   ```
   これにより、通常はメインスクリプト (`Program.cs`) が実行されます。このスクリプトは、以下の処理を実行します (環境変数 `TARGET_TEAM_ID` および `TARGET_CHANNEL_ID` の設定に依存します):
   
   • 参加しているチームの一覧を表示します
   • `TARGET_TEAM_ID` が設定されていれば、そのチームのチャンネル一覧を表示します
   • `TARGET_TEAM_ID` と `TARGET_CHANNEL_ID` が設定されていれば、そのチャンネルの最新メッセージ数件を表示し、インタラクティブメッセージ送信機能を開始します

3. **開発モード (自動リビルドと再起動あり)**:
   ```bash
   dotnet watch run --project src/MicrosoftGraphCSharpe.ConsoleApp
   ```

### 環境変数による制御（推奨）

設定ファイルを編集せずに、環境変数でモックデータの使用を制御することができます：

```bash
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

## Docker実行

Docker を使用してアプリケーションを実行するには、`docker-compose` を利用するのが最も簡単です。

1. **appsettings.Development.json ファイルの準備**: ローカル開発と同様に、プロジェクトのルートに設定ファイルを作成し、必要な環境変数（`CLIENT_ID`, `CLIENT_SECRET`, `TENANT_ID` など）を設定してください。`docker-compose.yml` はこの設定ファイルを自動的に読み込みます。

2. **Docker イメージのビルド** (初回または変更時):
   ```bash
   docker-compose build
   ```
   または、`up` コマンドに `--build` オプションを付けることでもビルドできます。

3. **Docker Compose でコンテナを起動**:
   ```bash
   docker-compose up
   ```
   これにより、イメージがビルドされ（まだビルドされていない場合）、コンテナが起動します。アプリケーションのログがコンソールに出力されます。
   
   デタッチモード（バックグラウンド実行）で起動する場合:
   ```bash
   docker-compose up -d
   ```

4. **コンテナの停止と削除**:
   ```bash
   docker-compose down
   ```

**(補足) Docker コマンドで直接実行する場合:**

`docker-compose` を使用せずに `docker run` で直接コンテナを実行したい場合は、まずイメージをビルドする必要があります。

1. **Docker イメージのビルド**:
   ```bash
   docker build -t ms-graph-teams-app .
   ```

2. **Docker コンテナの実行**: 設定ファイルの内容をコンテナに環境変数として渡す必要があります。例えば、設定ファイルを `--volume` オプションで指定します。
   ```bash
   docker run --rm -v $(pwd)/appsettings.Development.json:/app/appsettings.Development.json ms-graph-teams-app
   ```
   または、個々の環境変数を `-e` オプションで指定することも可能です。

## スクリプト

• `dotnet build`: C# を実行可能ファイルにコンパイルします  
• `dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp`: コンパイルされた C# アプリケーションを実行します  
• `dotnet watch run --project src/MicrosoftGraphCSharpe.ConsoleApp`: 開発モードでアプリケーションを実行します（ファイル変更時に自動再起動）  
• `dotnet test`: MSTest を使用してテストを実行します  
• `dotnet test --logger trx`: テスト結果をTRX形式で出力します（CI/CD用）    

## テスト

テストは MSTest を使用して実装されています。実際の認証情報を使わずにテストを実行できるようモックを使用しています。

テストファイルは `tests` ディレクトリにあります：

• `GraphAuthServiceTests.cs` - 認証機能のテスト  
• `TeamsServiceTests.cs` - Graph APIを使ったTeams操作機能のテスト  

### テスト実行方法

1. **全テスト実行**:
   ```bash
   dotnet test
   ```

2. **詳細出力でテスト実行**:
   ```bash
   dotnet test --verbosity normal
   ```

3. **カバレッジレポート付きテスト実行**:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

4. **特定のテストファイルのみ実行**:
   ```bash
   dotnet test --filter "FullyQualifiedName~GraphAuthServiceTests"
   ```
   または、特定のテストクラスのみ実行:
   ```bash
   dotnet test --filter "FullyQualifiedName~TeamsServiceTests"
   ```

#### 継続的インテグレーション

このプロジェクトは、ブランチへの変更やプルリクエストごとに自動テストを実行する準備が整っています：

1. **ローカル環境テスト**: .NET 8.0の標準環境でテストを実行  
2. **Docker環境テスト**: Dockerコンテナ内でテストを実行  

これにより、異なる環境でのアプリケーションの動作を検証できます。
