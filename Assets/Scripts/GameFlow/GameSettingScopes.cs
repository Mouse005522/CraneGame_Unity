using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// GameFlow 使用的設定目錄與預設 parser。
    /// </summary>
    public static class GameSettingScopes
    {
        const string GameSettingFolderName = "GameSettings";

        public static SettingParser Root { get; } = new SettingParser (
            $"{Application.streamingAssetsPath}/{GameSettingFolderName}");

        public static SettingParser Server { get; } = Root.CreateChild ("Server");
        public static SettingParser Client { get; } = Root.CreateChild ("Client");
    }
}
