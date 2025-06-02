# MicrosoftGraphCSharpe

Microsoft Graph APIを使用してC#でTeamsを操作するサンプルアプリケーション

## 概要

このプロジェクトはMicrosoft Graph APIを活用してMicrosoft Teamsを操作するサンプル実装です。**自動認証切り替え機能**により、読み取り操作には Application 認証、メッセージ送信には Delegated 認証を自動的に選択します。

## 主な特徴

- **🔄 自動認証切り替え**: 操作に応じて Application 認証と Delegated 認証を自動選択
- **📖 読み取り操作**: チーム一覧、チャネル一覧、メッセージ一覧 (Application 認証)
- **📝 メッセージ送信**: インタラクティブなメッセージ送信機能 (Delegated 認証)
- **🔧 設定不要**: 一度の設定で両方の認証モードが利用可能
- **✅ 完全テスト**: 全機能のユニットテスト実装済み

## 認証アーキテクチャ

このアプリケーションは、Microsoft Graph API の制約に対応するため、2つの認証方式を自動的に切り替えます：

### Application 認証 (Client Credential Flow)
- **用途**: 読み取り操作 (チーム、チャネル、メッセージ一覧の取得)
- **特徴**: ユーザー操作不要、自動実行可能
- **アクセス許可**: アプリケーション権限

### Delegated 認証 (Device Code Flow)
- **用途**: メッセージ送信操作
- **特徴**: ユーザー認証が必要、ブラウザでの認証フロー
- **アクセス許可**: 委任されたアクセス許可

## 必要な環境

- [.NET 8.0.400 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022、Visual Studio Code、またはその他の.NET開発環境
- Azure Active Directory テナント
- Microsoft Teamsが有効化されたテナント
- Microsoft Graph APIにアクセスするためのAzure ADアプリケーション登録

### 必要なアクセス許可

Azure AD アプリケーション登録で以下のアクセス許可を設定してください：

#### アプリケーション権限 (Application 認証用)
- `Team.ReadBasic.All` - チーム情報の読み取り
- `Channel.ReadBasic.All` - チャネル情報の読み取り
- `ChannelMessage.Read.All` - チャネルメッセージの読み取り

#### 委任されたアクセス許可 (Delegated 認証用)
- `Team.ReadBasic.All` - チーム情報の読み取り
- `Channel.ReadBasic.All` - チャネル情報の読み取り
- `ChannelMessage.Send` - チャネルメッセージの送信
- `ChannelMessage.Read.All` - チャネルメッセージの読み取り

## プロジェクト構成

- `MicrosoftGraphCSharpe.ConsoleApp`: コンソールアプリケーション
- `MicrosoftGraphCSharpe.Library`: Graph APIを使用するライブラリ
- `MicrosoftGraphCSharpe.Tests`: 単体テスト

## 追加ドキュメントと診断ツール

Azure AD 設定や権限の確認には、以下のドキュメントが役立ちます。

- **[AZURE_SETUP_GUIDE.md](AZURE_SETUP_GUIDE.md)** – メッセージ送信を有効にするための詳細な Azure AD 設定手順。
- **[QUICK_FIX_GUIDE.md](QUICK_FIX_GUIDE.md)** – `AADSTS7000218` などの一般的な認証エラーを素早く解決するためのガイド。
- **[AZURE_PERMISSION_FIX.md](AZURE_PERMISSION_FIX.md)** – API アクセス許可設定を修正するための手順。

## 🚀 自動認証切り替え機能

このアプリケーションの最大の特徴は、操作に応じて最適な認証方式を自動選択することです：

### 読み取り操作 (Application 認証)
- チーム一覧取得
- チャネル一覧取得
- メッセージ一覧取得

これらの操作は **ユーザー操作なし** で実行されます。

### メッセージ送信 (Delegated 認証)
- インタラクティブなメッセージ送信

この操作時には自動的に **Device Code Flow** が起動し、ユーザー認証を求められます：

```
🔐 ユーザー認証が必要です:
   ブラウザで以下のURLにアクセスしてください: https://microsoft.com/devicelogin
   表示される画面で以下のコードを入力してください: ABC123456
   認証完了まで少々お待ちください...
```

## メッセージ送信機能

### インタラクティブメッセージ送信

アプリケーション実行時に、対話型のメッセージ送信機能が利用できます：

- **コンソール入力**: ユーザーがコンソールからメッセージを入力
- **自動認証**: メッセージ送信時に Delegated 認証を自動起動
- **リアルタイム送信**: 入力されたメッセージを即座にTeamsチャネルに送信
- **終了コマンド**: `exit`または`quit`で機能を終了
- **エラーハンドリング**: 送信失敗時の適切なエラー処理

### 使用方法

1. アプリケーションを実行すると、まず Application 認証で読み取り操作が実行されます
2. インタラクティブモードで "送信するメッセージを入力してください" プロンプトが表示されます
3. 初回メッセージ送信時に Device Code Flow による認証が自動で開始されます
4. ブラウザで認証を完了すると、メッセージがTeamsチャネルに送信されます
5. 2回目以降の送信では認証は不要です（トークンキャッシュ済み）
6. `exit`または`quit`を入力すると機能が終了します

## 事前準備

### Azure ADアプリケーション登録

1. [Azure Portal](https://portal.azure.com)にアクセスし、「アプリの登録」でアプリケーションを登録します
2. 以下のMicrosoft Graph APIのアプリケーション権限を追加：
   - `Team.ReadBasic.All`（チームの基本情報を読み取る）
   - `TeamSettings.Read.All`（チーム設定を読み取る）
   - `ChannelMessage.Read.All`（チャンネルメッセージを読み取る）
   - `ChannelMessage.Send`（チャンネルメッセージを送信する）
3. クライアントシークレットを作成し、値をメモしておきます
4. 管理者の同意を付与します

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

## ローカル環境での実行方法

### コマンドライン

```bash
# プロジェクトディレクトリに移動
cd /Users/niya/Documents/MicrosoftGraphCSharpe

# アプリケーションのビルド
dotnet build

# コンソールアプリケーションの実行
dotnet run --project src/MicrosoftGraphCSharpe.ConsoleApp
```

### Visual Studio Code

1. Visual Studio Codeでプロジェクトフォルダを開く
2. .NET Core Launch (.NET) 構成を使用して実行/デバッグ

## テスト

```bash
# テストの実行
dotnet test
```

### CI/CD テスト

このリポジトリはGitHub Actionsを使用して、ブランチに変更があった場合に自動的にテストを実行します。以下の環境でテストが実行されます：

1. ローカル環境（dotnet testコマンドを使用）
2. Docker環境（コンテナ内でテストを実行）

GitHub Actionsの設定は`.github/workflows/run-tests.yml`ファイルで定義されています。

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

## 注意事項

- プロダクション環境で使用する場合は、クライアントシークレットなどの機密情報の管理方法を適切に見直してください
- APIアクセス権限は必要最小限にするよう設計してください
