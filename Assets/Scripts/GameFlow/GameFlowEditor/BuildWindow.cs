using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kun.Tool
{
    [Serializable]
    public class BuildWindowCache
    {
        public EntryType singleEntryType = EntryType.Client;
        public List<EntryType> batchEntryTypes = new List<EntryType> () { EntryType.Server, EntryType.Client };
        public bool isDebug;
    }

    public class BuildWindow : EditorWindow
    {
        const string CacheKey = "BuildWindowCache";
        const string LastBuildFolderKey = "BuildWindow_LastBuildFolder";

        // HelpBox 的固定出血：左右 padding + icon 寬度
        const float HelpBoxPadding = 48f;
        // 視窗左右 margin 出血
        const float WindowBleed = 24f;
        // 視窗最小高度
        const float WindowMinHeight = 800f;

        // --- Cache ---
        JsonEditorPrefs<BuildWindowCache> windowCache;
        Vector2 batchScrollPos;

        [MenuItem ("Editor Flow/Build Window")]
        static void Open ()
        {
            var window = GetWindow<BuildWindow> ("Build Window");
            window.minSize = new Vector2 (CalcMinWindowWidth (), WindowMinHeight);
        }

        void OnGUI ()
        {
            // compile 後記憶體會被清空，在 OnGUI 開頭做 null check 保證安全
            if (windowCache == null)
            {
                windowCache = new JsonEditorPrefs<BuildWindowCache> (CacheKey, new BuildWindowCache ());
            }

            DrawSharedSettings ();
            GUILayout.Space (10f);
            EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
            GUILayout.Space (10f);

            // ── 上半區塊：遊戲場景輸出 ──
            EditorGUILayout.LabelField ("遊戲場景輸出", EditorStyles.boldLabel);
            GUILayout.Space (5f);
            DrawSingleSection ();
            GUILayout.Space (10f);
            EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
            GUILayout.Space (10f);
            DrawBatchSection ();

            GUILayout.Space (10f);
            EditorGUILayout.LabelField ("", GUI.skin.horizontalSlider);
            GUILayout.Space (10f);
        }

        // -------------------------------------------------------
        // 共用設定
        // -------------------------------------------------------

        void DrawSharedSettings ()
        {
            var cache = windowCache.CurrentValue;

            EditorGUILayout.LabelField ("共用設定", EditorStyles.boldLabel);

            GUILayout.Space (5f);

            bool newIsDebug = EditorGUILayout.Toggle ("Development Build", cache.isDebug);

            if (newIsDebug != cache.isDebug)
            {
                cache.isDebug = newIsDebug;
                windowCache.SetModify ();
            }

        }

        // -------------------------------------------------------
        // 單一輸出
        // -------------------------------------------------------

        void DrawSingleSection ()
        {
            var cache = windowCache.CurrentValue;

            EditorGUILayout.LabelField ("單一輸出", EditorStyles.boldLabel);

            GUILayout.Space (5f);

            EntryType newType = EditorTool.DrawEnum (cache.singleEntryType, "遊戲類型", GUILayout.Width (300));

            if (newType != cache.singleEntryType)
            {
                cache.singleEntryType = newType;
                windowCache.SetModify ();
            }

            GUILayout.Space (5f);

            EditorGUILayout.HelpBox (GetBuildInfoText (cache.singleEntryType), MessageType.Info);

            GUILayout.Space (5f);

            if (GUILayout.Button ("開始輸出", GUILayout.Height (35f)))
            {
                Build (cache.singleEntryType, cache.isDebug);
            }

            GUI.enabled = true;
        }

        // -------------------------------------------------------
        // 組合輸出
        // -------------------------------------------------------

        void DrawBatchSection ()
        {
            EditorGUILayout.LabelField ("組合輸出", EditorStyles.boldLabel);

            GUILayout.Space (5f);

            var list = windowCache.CurrentValue.batchEntryTypes;

            EditorTool.DrawInHorizontal (() =>
            {
                GUILayout.FlexibleSpace ();

                if (GUILayout.Button ("新增", GUILayout.Width (60f)))
                {
                    list.Add (EntryType.Client);
                    windowCache.SetModify ();
                }
            });

            GUILayout.Space (5f);

            int removeIndex = -1;

            batchScrollPos = EditorGUILayout.BeginScrollView (batchScrollPos, GUILayout.MaxHeight (150f));

            for (int i = 0; i < list.Count; i++)
            {
                int captured = i;

                EditorTool.DrawInHorizontal (() =>
                {
                    EditorGUILayout.LabelField ($"{captured + 1}.", GUILayout.Width (24f));

                    EntryType newType = EditorTool.DrawEnum (list[captured], "", GUILayout.Width (160f));

                    if (newType != list[captured])
                    {
                        list[captured] = newType;
                        windowCache.SetModify ();
                    }

                    GUILayout.Space (6f);

                    EditorGUILayout.HelpBox (GetBuildInfoText (list[captured]), MessageType.None);

                    Color prevColor = GUI.color;
                    GUI.color = new Color (1f, 0.45f, 0.45f);

                    if (GUILayout.Button ("移除", GUILayout.Width (50f)))
                    {
                        removeIndex = captured;
                    }

                    GUI.color = prevColor;
                });

                GUILayout.Space (4f);
            }

            EditorGUILayout.EndScrollView ();

            if (removeIndex >= 0)
            {
                list.RemoveAt (removeIndex);
                windowCache.SetModify ();
            }

            GUILayout.Space (5f);

            GUI.enabled = list.Count > 0;

            if (GUILayout.Button ($"全部輸出（共 {list.Count} 個）", GUILayout.Height (35f)))
            {
                BuildBatch (list, windowCache.CurrentValue.isDebug);
            }

            GUI.enabled = true;
        }

        // -------------------------------------------------------
        // Build
        // -------------------------------------------------------

        static void Build (EntryType targetEntryType, bool isDebug)
        {
            string baseFolder = PickBuildFolder ("選擇輸出資料夾");

            if (string.IsNullOrEmpty (baseFolder))
            {
                Debug.LogWarning ("取消輸出");
                return;
            }

            string timestampFolder = Path.Combine (baseFolder, GetTimestampFolderName ());
            try
            {
                ExecuteGameBuild (targetEntryType, timestampFolder, isDebug);
            }
            catch (Exception ex)
            {
                Debug.LogException (ex);
            }
        }

        static void BuildBatch (List<EntryType> entryTypes, bool isDebug)
        {
            string baseFolder = PickBuildFolder ("選擇組合輸出根目錄");

            if (string.IsNullOrEmpty (baseFolder))
            {
                Debug.LogWarning ("取消輸出");
                return;
            }

            // 批次輸出共用同一個時間戳資料夾
            string timestampFolder = Path.Combine (baseFolder, GetTimestampFolderName ());

            try
            {
                for (int i = 0; i < entryTypes.Count; i++)
                {
                    EntryType entryType = entryTypes[i];
                    EditorUtility.DisplayProgressBar (
                        "組合輸出",
                        $"[{i + 1}/{entryTypes.Count}] 正在輸出 {entryType}...",
                        (float)i / entryTypes.Count
                    );

                    // 每個 EntryType 多一層資料夾，避免覆蓋
                    string entryFolder = Path.Combine (timestampFolder, entryType.ToString ());

                    Debug.Log ($"[{i + 1}/{entryTypes.Count}] 開始輸出 {entryType}...");
                    ExecuteGameBuild (entryType, entryFolder, isDebug);
                }

                Debug.Log ("組合輸出完成");
            }
            catch (Exception ex)
            {
                Debug.LogException (ex);
                Debug.LogError ("組合輸出中止，請查看例外詳細資訊。");
            }
            finally
            {
                EditorUtility.ClearProgressBar ();
            }
        }

        static void ExecuteGameBuild (EntryType targetEntryType, string buildFolder, bool isDebug)
        {
            // --- 記錄原有狀態 ---
            EnvirSetting envirSetting = GameSettingScopes.Root.LoadJson<EnvirSetting> ();
            EntryType originalEntryType = envirSetting.entryType;
            bool displayDebugGUI = envirSetting.DisplayDebugGUI;
            string excludedFolder = null;
            string backupFolder = null;

            try
            {
                MoveExcludedSettingsOutsideAssets (targetEntryType, out excludedFolder, out backupFolder);

                // --- 套用輸出前設定 ---
                envirSetting.entryType = targetEntryType;
                envirSetting.DisplayDebugGUI = isDebug;

                GameSettingScopes.Root.SaveJson (envirSetting);

                string outputName = targetEntryType.GetBuildOutputName (Application.productName);
                string outputPath = targetEntryType.GetBuildLocationPath (buildFolder, outputName);
                string[] scenePaths = EditorBuildSettings.scenes
                    .Where (scene => scene.enabled)
                    .Select (scene => scene.path)
                    .ToArray ();
                BuildTarget target = targetEntryType.GetBuildTarget ();
                int subtarget = targetEntryType.GetBuildSubtarget ();

                BuildOptions buildOptions = GetBuildOptions (isDebug);

                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = scenePaths,
                    locationPathName = outputPath,
                    target = target,
                    subtarget = subtarget,
                    options = buildOptions,
                };

                try
                {
                    BuildReport report = BuildPipeline.BuildPlayer (options);

                    Debug.Log ($"輸出結果：{report.summary.result}，路徑：{outputPath}");

                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                    Debug.LogError ($"輸出失敗，路徑：{outputPath}");
                }
            }
            finally
            {
                try
                {
                    RestoreExcludedSettings (excludedFolder, backupFolder);
                }
                finally
                {
                    // --- 還原原有狀態 ---
                    envirSetting.entryType = originalEntryType;
                    envirSetting.DisplayDebugGUI = displayDebugGUI;
                    GameSettingScopes.Root.SaveJson (envirSetting);

                    Debug.Log ($"已還原 entryType = {originalEntryType}");
                }
            }
        }

        // -------------------------------------------------------
        // Helpers
        // -------------------------------------------------------

        /// <summary>
        /// 將另一端的設定資料夾與 .meta 暫時移至 Assets 外，避免打包。
        /// </summary>
        static void MoveExcludedSettingsOutsideAssets (EntryType targetEntryType, out string excludedFolder, out string backupFolder)
        {
            // 透過 out 保留移動途中發生例外時的復原路徑。
            excludedFolder = null;
            backupFolder = null;
            string excludedSetting = targetEntryType == EntryType.Server ? GameSettingScopes.ClientScopeName
                : targetEntryType == EntryType.Client ? GameSettingScopes.ServerScopeName : null;

            if (excludedSetting != null)
            {
                excludedFolder = targetEntryType == EntryType.Server
                    ? GameSettingScopes.ClientFolderPath
                    : GameSettingScopes.ServerFolderPath;

                if (Directory.Exists (excludedFolder))
                {
                    // 暫存在 Assets 外，避免被打包；與專案同磁碟以便直接移動。
                    string backupRoot = Path.Combine (Application.dataPath, "..", "Library", "BuildWindowSettings", Guid.NewGuid ().ToString ("N"));
                    Directory.CreateDirectory (backupRoot);
                    backupFolder = Path.Combine (backupRoot, excludedSetting);
                    Directory.Move (excludedFolder, backupFolder);

                    if (File.Exists (excludedFolder + ".meta"))
                    {
                        File.Move (excludedFolder + ".meta", backupFolder + ".meta");
                    }

                    AssetDatabase.Refresh ();
                }
            }
        }

        /// <summary>
        /// 復原暫時移出的設定資料夾與 .meta，並移除空的暫存目錄。
        /// </summary>
        static void RestoreExcludedSettings (string excludedFolder, string backupFolder)
        {
            if (backupFolder != null)
            {
                if (Directory.Exists (backupFolder))
                {
                    Directory.Move (backupFolder, excludedFolder);
                }

                if (File.Exists (backupFolder + ".meta"))
                {
                    File.Move (backupFolder + ".meta", excludedFolder + ".meta");
                }

                Directory.Delete (Path.GetDirectoryName (backupFolder));
                AssetDatabase.Refresh ();
            }
        }

        /// <summary>
        /// 開啟資料夾選擇面板，自動帶入上次路徑並將結果緩存至 EditorPrefs
        /// </summary>
        static string PickBuildFolder (string title)
        {
            string lastFolder = EditorPrefs.GetString (LastBuildFolderKey, "");
            string selected = EditorUtility.SaveFolderPanel (title, lastFolder, "");

            if (!string.IsNullOrEmpty (selected))
            {
                EditorPrefs.SetString (LastBuildFolderKey, selected);
            }

            return selected;
        }

        /// <summary>
        /// 以 UTC 時間產生資料夾名稱，格式：yyyy_MM_dd_HH_mm
        /// </summary>
        static string GetTimestampFolderName ()
        {
            return DateTime.UtcNow.ToString ("yyyy_MM_dd_HH_mm");
        }

        /// <summary>
        /// 計算所有 EntryType 中最長的 HelpBox 文字寬度，加上出血後作為視窗最小寬度
        /// </summary>
        static float CalcMinWindowWidth ()
        {
            float maxTextWidth = 0f;

            foreach (EntryType entryType in Enum.GetValues (typeof (EntryType)))
            {
                string text = GetBuildInfoText (entryType);
                float textWidth = EditorStyles.helpBox.CalcSize (new GUIContent (text)).x;

                if (textWidth > maxTextWidth)
                {
                    maxTextWidth = textWidth;
                }
            }

            return maxTextWidth + HelpBoxPadding + WindowBleed;
        }

        static string GetBuildInfoText (EntryType entryType)
        {
            string platform = entryType.GetBuildProfileName ();
            return $"entryType = {entryType}  |  平台：{platform}";
        }

        static BuildOptions GetBuildOptions (bool isDebug)
        {
            if (isDebug)
            {
                return BuildOptions.ShowBuiltPlayer | BuildOptions.Development;
            }

            return BuildOptions.ShowBuiltPlayer;
        }
    }
}
