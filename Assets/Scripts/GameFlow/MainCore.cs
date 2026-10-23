using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Kun.Tool;
using Kun.CraneGame;

namespace Kun.Tool
{
    public class MainCore : MonoBehaviour
    {
        List<GameBase> gameBases = new List<GameBase> ();

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
            var rootServices = new ServiceCollection ();
            rootServices.SetRoot (transform);
            rootServices.AddService (GameSettingScopes.Root);

            if (!rootServices.ServiceInject.TryGetService<SettingParser> (out var rootParser))
            {
                throw new InvalidOperationException ("Root scope 未註冊 SettingParser。");
            }

            var envirSetting = await rootParser.LoadJsonAsync<EnvirSetting> ();
            if (this == null)
            {
                return;
            }

            Debug.Log ($"EnvirSetting: {JsonUtility.ToJson (envirSetting)}");

            var entryType = envirSetting.entryType;

            if (entryType.HasFlag (EntryType.Server)) 
            {
                Debug.Log ($"啟動 Server 模式");
                var server = gameObject.AddComponent<GameServer> ();
                gameBases.Add (server);
                await server.Setup (rootServices);
                if (this == null)
                    return;
            }

            if (entryType.HasFlag (EntryType.Client)) 
            {
                Debug.Log ($"啟動 Client 模式");
                var client = gameObject.AddComponent<GameClient> ();
                gameBases.Add (client);
                await client.Setup (rootServices);
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
            foreach (var gameBase in gameBases)
            {
                if (gameBase != null)
                {
                    try 
                    {
                        gameBase.Close ();
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

