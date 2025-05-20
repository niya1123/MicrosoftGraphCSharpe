FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# コピー元のファイルが存在することを確認
COPY ["MicrosoftGraphCSharpe.sln", "./"]
COPY ["src/MicrosoftGraphCSharpe.ConsoleApp/MicrosoftGraphCSharpe.ConsoleApp.csproj", "src/MicrosoftGraphCSharpe.ConsoleApp/"]
COPY ["src/MicrosoftGraphCSharpe.Library/MicrosoftGraphCSharpe.Library.csproj", "src/MicrosoftGraphCSharpe.Library/"]
COPY ["tests/MicrosoftGraphCSharpe.Tests/MicrosoftGraphCSharpe.Tests.csproj", "tests/MicrosoftGraphCSharpe.Tests/"]

# NuGetパッケージの復元
RUN dotnet restore

# プロジェクトファイルをコピー
COPY . .

# アプリケーションをビルド
RUN dotnet build "src/MicrosoftGraphCSharpe.ConsoleApp/MicrosoftGraphCSharpe.ConsoleApp.csproj" -c Release -o /app/build

# リリース用の発行
FROM build AS publish
RUN dotnet publish "src/MicrosoftGraphCSharpe.ConsoleApp/MicrosoftGraphCSharpe.ConsoleApp.csproj" -c Release -o /app/publish

# 最終イメージ
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

# 発行したアプリケーションをコピー
COPY --from=publish /app/publish .

# 設定ファイルをコピー
COPY appsettings.json ./
COPY appsettings.Development.json ./

# 環境変数を設定
ENV DOTNET_ENVIRONMENT=Development
ENV DOTNET_CONSOLE_ALLOW_ANSI_COLOR=true

# コンテナ起動時のエントリポイント
ENTRYPOINT ["dotnet", "MicrosoftGraphCSharpe.ConsoleApp.dll"]
