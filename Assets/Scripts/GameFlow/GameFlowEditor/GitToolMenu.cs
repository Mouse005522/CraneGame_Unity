using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Kun.Tool
{
    public static class GitToolMenu
    {
        [MenuItem ("Editor Flow/Git/Discard 字體 .asset 變更")]
        static void DiscardAssetFiles ()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "checkout -- \"Assets/GameResources/Font/*.asset\"",
                    WorkingDirectory = Path.GetDirectoryName (Application.dataPath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process process = Process.Start (startInfo);
                process.StandardOutput.ReadToEnd ();
                string error = process.StandardError.ReadToEnd ();
                process.WaitForExit ();

                if (process.ExitCode == 0)
                {
                    Debug.Log ("[Git] 已成功還原 Font 資料夾下所有 .asset 檔案的變更");
                    AssetDatabase.Refresh ();
                }
                else
                {
                    Debug.LogError ($"[Git] 還原字體 .asset 失敗：{error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError ($"[Git] 執行 git 命令時發生例外：{ex.Message}");
            }
        }
    }
}
