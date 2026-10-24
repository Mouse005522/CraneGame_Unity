using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Kun.Tool
{
    [CustomEditor (typeof (MainCore))]
    public class MainCoreEditor : SerializedObjectEditor<MainCore>
    {
        EnvirSetting envirSetting;
        ClientSetting clientSetting;
        ServerSetting serverSetting;

        const string FoldoutKeyPrefix = "MainCoreEditor_Foldout_";

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();

            if (envirSetting == null)
            {
                envirSetting = GameSettingScopes.Root.LoadJson<EnvirSetting> ();
            }


            EditorGUILayout.LabelField ("遊戲設定", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck ();

            envirSetting.entryType = EditorTool.DrawEnum (envirSetting.entryType, "遊戲類型", GUILayout.Width (300));

            GUILayout.Space (10f);

            if (envirSetting.entryType.HasFlag (EntryType.Client))
            {
                if (clientSetting == null)
                    clientSetting = GameSettingScopes.Client.LoadJson<ClientSetting> ();

                using (new EditorGUI.DisabledScope (Application.isPlaying))
                {
                    GUILayout.Label ("Client 設定");
                    clientSetting.roomServerIp = EditorGUILayout.TextField ("Room API 位址", clientSetting.roomServerIp);
                    clientSetting.iceGatheringTimeoutSeconds = EditorGUILayout.FloatField ("ICE 收集逾時（秒）", clientSetting.iceGatheringTimeoutSeconds);
                }
            }

            if (envirSetting.entryType.HasFlag (EntryType.Server))
            {
                if (serverSetting == null)
                    serverSetting = GameSettingScopes.Server.LoadJson<ServerSetting> ();

                using (new EditorGUI.DisabledScope (Application.isPlaying))
                {
                    GUILayout.Label ("Server 設定");
                    serverSetting.gameServerPipleName = EditorGUILayout.TextField ("Game Server Pipe 名稱", serverSetting.gameServerPipleName);
                    serverSetting.apiServerPipleName = EditorGUILayout.TextField ("API Server Pipe 名稱", serverSetting.apiServerPipleName);
                    serverSetting.TimeoutSeconds = EditorGUILayout.FloatField ("Pipe 傳送逾時（秒）", serverSetting.TimeoutSeconds);
                    serverSetting.iceGatheringTimeoutSeconds = EditorGUILayout.FloatField ("ICE 收集逾時（秒）", serverSetting.iceGatheringTimeoutSeconds);
                }
            }

            GUILayout.Space (10f);

            bool displayDebugGUI = PlayerPrefs.GetInt (MainCore.DisplayDebugGUIKey, 0) == 1;
            GUILayout.Label ("顯示偵錯資訊");
            displayDebugGUI = EditorGUILayout.Toggle ("Display", displayDebugGUI);

            EditorGUI.EndChangeCheck ();

            if (GUI.changed)
            {
                GameSettingScopes.Root.SaveJson (envirSetting);
                if (clientSetting != null)
                    GameSettingScopes.Client.SaveJson (clientSetting);
                if (serverSetting != null)
                    GameSettingScopes.Server.SaveJson (serverSetting);
                PlayerPrefs.SetInt (MainCore.DisplayDebugGUIKey, displayDebugGUI ? 1 : 0);
            }
        }

        static bool DrawFoldout (string title, string key)
        {
            string prefsKey = FoldoutKeyPrefix + key;
            bool current = EditorPrefs.GetBool (prefsKey, true);

            //var msg = current ? $"▼ {title}" : $"▶ {title}";
            bool next = GUILayout.Toggle (current, title, EditorStyles.foldoutHeader);

            if (next != current)
            {
                EditorPrefs.SetBool (prefsKey, next);
            }

            return next;
        }
    }
}
