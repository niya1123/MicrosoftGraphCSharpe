# MicrosoftGraphCSharpe

Microsoft Graph APIを使用してC#でTeamsを操作するサンプルアプリケーション

## 概要

このプロジェクトはMicrosoft Graph APIを活用してMicrosoft Teamsを操作するサンプル実装です。
アプリケーションの主な機能:

- Azure Active Directoryの認証（クライアント資格情報フロー）
- Teamsの一覧取得
- チャンネルの一覧取得
- チャンネルへのメッセージ送信
- チャンネルメッセージの一覧取得

## 必要な環境

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
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
