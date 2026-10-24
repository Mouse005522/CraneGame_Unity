using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Text;
using Kun.Tool;

namespace Kun.CraneGame
{
    public class GameClient : GameBase 
    {
        static readonly byte[] reliableTestMessage = Encoding.UTF8.GetBytes ("reliable");
        static readonly byte[] realtimeTestMessage = Encoding.UTF8.GetBytes ("realtime");

        protected override string SettingScopeName => "Client";

        protected override async UniTask PrepareServices ()
        {
            var setting = await SettingParser.LoadJsonAsync<ClientSetting> ();
            Services.AddService (setting);

            webRTCClient = gameObject.AddComponent<EasyWebRTCClient> ();
            ReliableChannel = webRTCClient.BindChannel ("reliable", true, -1);
            RealtimeChannel = webRTCClient.BindChannel ("realtime", false, 0);
            webRTCClient.Setup (setting.roomServerIp, setting.iceGatheringTimeoutSeconds);
        }

        public EasyWebRTCChannel ReliableChannel { get; private set; }
        public EasyWebRTCChannel RealtimeChannel { get; private set; }
        public EasyWebRTCChannel GetChannel (string name) => webRTCClient.Channels[name];

        EasyWebRTCClient webRTCClient;

        protected override void Update ()
        {
            base.Update ();

            if (Input.GetKeyDown (KeyCode.Space))
            {
                SendTestMessage (ReliableChannel, reliableTestMessage);
            }
            if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter))
            {
                SendTestMessage (RealtimeChannel, realtimeTestMessage);
            }
        }

        void SendTestMessage (EasyWebRTCChannel channel, byte[] message)
        {
            if (channel == null)
            {
                Debug.LogWarning ("[Client] 測試通道尚未建立。", this);
                return;
            }

            bool sent = channel.Send (message);
            if (!sent)
            {
                Debug.LogWarning ($"[Client] Channel {channel.Name} 尚未連線或已關閉，測試訊息未送出。", this);
            }
        }

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
