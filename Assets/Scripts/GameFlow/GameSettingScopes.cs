using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// GameFlow 使用的設定目錄與預設 parser。
    /// </summary>
    public static class GameSettingScopes
    {
        public const string GameSettingFolderName = "GameSettings";
        public const string ServerScopeName = "Server";
        public const string ClientScopeName = "Client";

        public static string RootFolderPath => $"{Application.streamingAssetsPath}/{GameSettingFolderName}";
        public static string ServerFolderPath => Server.FolderPath;
        public static string ClientFolderPath => Client.FolderPath;

        public static SettingParser Root { get; } = new SettingParser (
            RootFolderPath);

        public static SettingParser Server { get; } = Root.CreateChild (ServerScopeName);
        public static SettingParser Client { get; } = Root.CreateChild (ClientScopeName);
    }
}
