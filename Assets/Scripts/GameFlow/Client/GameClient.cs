using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public class GameClient : GameBase 
    {
        protected override string SettingScopeName => "Client";

        protected override UniTask PrepareServices ()
        {
            webRTCClient = gameObject.AddComponent<EasyWebRTCClient> ();
            webRTCClient.Setup (serverIp, serverPort);

            return UniTask.CompletedTask;
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