using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CraneGameServer.Services;

namespace Kun.Tool
{
    public class GameServer : GameBase
    {
        protected override string SettingScopeName => "Server";

        protected override UniTask PrepareServices ()
        {
            webRTCServer = this.gameObject.AddComponent<EasyWebRTCServer> ();
            webRTCServer.Setup (serverIp, serverPort);

            Microsoft.Extensions.Logging.ILogger<NamedPipesRouter> logger = UnityLoggerProvider.CreateLogger<NamedPipesRouter> ("GameServer");

            var namedPipesRouter = new NamedPipesRouter ();
            namedPipesRouter.BindLogger (logger);

            return UniTask.CompletedTask;
        }

        EasyWebRTCServer webRTCServer;

        INamedPipesRouter namedPipesRouter;

        protected override void OnClose ()
        {
            if (webRTCServer != null)
            {
                webRTCServer.Close ();
                Destroy (webRTCServer);
            }
        }
    }
}