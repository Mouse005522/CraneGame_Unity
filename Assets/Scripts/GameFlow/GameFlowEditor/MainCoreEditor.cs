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

            GUILayout.Label ("連線port");

            int newPort = EditorGUILayout.IntField (envirSetting.serverPort, GUILayout.Width (300f));
            envirSetting.serverPort = newPort;

            if (envirSetting.entryType.HasFlag (EntryType.Client))
            {
                // 只有在非 Play Mode 時才能修改 VR 設定
                GUI.enabled = !Application.isPlaying;

                GUILayout.Label ("連線ip");

                string newIp = EditorGUILayout.TextField (envirSetting.serverIp, GUILayout.Width (300f));
                envirSetting.serverIp = newIp;

                GUI.enabled = true;
            }

            GUILayout.Space (10f);

            bool displayDebugGUI = PlayerPrefs.GetInt (MainCore.DisplayDebugGUIKey, 0) == 1;
            GUILayout.Label ("顯示偵錯資訊");
            displayDebugGUI = EditorGUILayout.Toggle ("Display", displayDebugGUI);

            if (envirSetting.entryType.HasFlag (EntryType.Server))
            {

            }

            EditorGUI.EndChangeCheck ();

            if (GUI.changed)
            {
                GameSettingScopes.Root.SaveJson (envirSetting);
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
