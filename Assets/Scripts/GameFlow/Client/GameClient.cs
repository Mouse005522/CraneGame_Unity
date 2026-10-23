using UnityEngine;
using Cysharp.Threading.Tasks;
using Kun.Tool;

namespace Kun.CraneGame
{
    public class GameClient : GameBase 
    {
        protected override string SettingScopeName => "Client";

        protected override async UniTask PrepareServices ()
        {
            var setting = await SettingParser.LoadJsonAsync<ClientSetting> ();
            Services.AddService (setting);

            webRTCClient = gameObject.AddComponent<EasyWebRTCClient> ();
            webRTCClient.Setup (setting.roomServerIp, setting.iceGatheringTimeoutSeconds);
        }

        EasyWebRTCClient webRTCClient;

        protected override void OnClose ()
        {
            if (webRTCClient != null)
            {
                webRTCClient.Close ();
                Destroy (webRTCClient);
            }
        }
    }
}