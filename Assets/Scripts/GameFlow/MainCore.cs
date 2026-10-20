using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Kun.Tool;

namespace Kun.Tool
{
    public class MainCore : MonoBehaviour
    {
        List<EasyWebRTCBase> RTCBases = new List<EasyWebRTCBase> ();

        public const string DisplayDebugGUIKey = "DisplayDebugGUI";

        public static bool DisplayDebugGUI;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake ()
        {
            // 在初始化時設定
            UniTaskScheduler.UnobservedTaskException += (ex) =>
            {
                Debug.LogError ($"未處理的 UniTask 異常");
                Debug.LogException (ex);
            };

            SettingDisplayDebugGUI ();

            RunFlow ().Forget ();
        }

        

        async UniTaskVoid RunFlow ()
        {
            var envirSetting = await SettingParser.LoadJsonAsync<EnvirSetting> ();

            Debug.Log ($"EnvirSetting: {JsonUtility.ToJson (envirSetting)}");

            var entryType = envirSetting.entryType;

            if (entryType == EntryType.Host) 
            {
                // 如果是 Host 模式，則使用本地 IP
                envirSetting.serverIp = "127.0.0.1";
            }

            if (entryType.HasFlag (EntryType.Server)) 
            {
                Debug.Log ($"啟動 Server 模式");
                var server = gameObject.AddComponent<EasyWebRTCServer> ();
                server.Setup (envirSetting.serverIp, envirSetting.serverPort);

                RTCBases.Add (server);
            }

            if (entryType.HasFlag (EntryType.Client)) 
            {
                Debug.Log ($"啟動 Client 模式");
                var client = gameObject.AddComponent<EasyWebRTCClient> ();
                client.Setup (envirSetting.serverIp, envirSetting.serverPort);
                RTCBases.Add (client);
            }
        }

        void SettingDisplayDebugGUI ()
        {
            if (Application.isEditor)
            {
                DisplayDebugGUI = PlayerPrefs.GetInt (DisplayDebugGUIKey, 0) == 1;
            }
            else
            {
                DisplayDebugGUI = false;
            }
        }

        void OnDestroy ()
        {
            foreach (var rtcBase in RTCBases)
            {
                if (rtcBase != null)
                {
                    try 
                    {
                        rtcBase.Close ();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError ($"關閉 RTCBase 時發生異常");
                        Debug.LogException (ex);
                    }
                }
            }
        }
    }
}

