using Microsoft.Graph.Models;
using System.Collections.Generic;

namespace MicrosoftGraphCSharpe.Library.Models
{
    /// <summary>
    /// モックデータのモデルクラス群
    /// 設定ファイルからサンプルデータを読み込むためのモデルを定義します。
    /// </summary>

    /// <summary>
    /// サンプルチームのモデルクラス
    /// </summary>
    public class SampleTeam
    {
        /// <summary>チームのID</summary>
        public string Id { get; set; }
        
        /// <summary>チームの表示名</summary>
        public string DisplayName { get; set; }
        
        /// <summary>チームの説明</summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// サンプルチャンネルのモデルクラス
    /// </summary>
    public class SampleChannel
    {
        /// <summary>チャンネルのID</summary>
        public string Id { get; set; }
        
        /// <summary>チャンネルの表示名</summary>
        public string DisplayName { get; set; }
        
        /// <summary>チャンネルの説明</summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// サンプルメッセージのモデルクラス
    /// </summary>
    public class SampleMessage
    {
        /// <summary>メッセージのID</summary>
        public string Id { get; set; }
        
        /// <summary>メッセージの内容</summary>
        public string Content { get; set; }
        
        /// <summary>送信者の名前</summary>
        public string FromName { get; set; }
    }

    /// <summary>
    /// サンプルデータの設定クラス
    /// 設定ファイルから読み取るサンプルデータの構造を定義します。
    /// </summary>
    public class SampleDataConfig
    {
        /// <summary>サンプルチームのリスト</summary>
        public List<SampleTeam> Teams { get; set; }
        
        /// <summary>チームIDをキーとしたチャンネルリストの辞書</summary>
        public Dictionary<string, List<SampleChannel>> Channels { get; set; }
        
        /// <summary>"teamId|channelId"をキーとしたメッセージリストの辞書</summary>
        public Dictionary<string, List<SampleMessage>> Messages { get; set; }
    }
}
