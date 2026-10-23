using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CraneGameServer.Services;
using CraneGameServer.WebApi;
using Kun.Tool;

namespace Kun.CraneGame
{
    public class GameServer : GameBase
    {
        protected override string SettingScopeName => "Server";

        static readonly JsonSerializerOptions PipeJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        protected override async UniTask PrepareServices ()
        {
            var setting = await SettingParser.LoadJsonAsync<ServerSetting> ();
            Services.AddService (setting);

            webRTCServer = this.gameObject.AddComponent<EasyWebRTCServer> ();
            webRTCServer.Setup (setting.iceGatheringTimeoutSeconds);

            Microsoft.Extensions.Logging.ILogger<NamedPipesRouter> logger = UnityLoggerProvider.CreateLogger<NamedPipesRouter> ("GameServer");

            var namedPipesRouter = new NamedPipesRouter ();
            namedPipesRouter.BindLogger (logger);
            this.namedPipesRouter = namedPipesRouter;
            Services.AddService<INamedPipesRouter> (namedPipesRouter);
            this.namedPipesRouter.RegisterHandler (GameRoomModel.OnJoinRoomRouter, HandleJoinRoomRequest);
            this.namedPipesRouter.Start (setting.gameServerPipleName, setting.apiServerPipleName, setting.TimeoutSeconds);
        }

        async Task<string> HandleJoinRoomRequest (string json, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.Deserialize<ClientJoinRoomRequest> (json, PipeJsonOptions);
            if (request == null || string.IsNullOrWhiteSpace (request.Sdp))
                throw new ArgumentException ("JoinRoom 請求必須包含有效的 sdp。", nameof (json));

            // Pipe callback 可能在背景執行緒，WebRTC API 必須在 Unity 主執行緒呼叫。
            await UniTask.SwitchToMainThread (cancellationToken);
            cancellationToken.ThrowIfCancellationRequested ();
            if (webRTCServer == null)
                throw new InvalidOperationException ("WebRTC Server 尚未啟動或已關閉。");

            var answerSdp = await webRTCServer.HandleOfferRequest (request.Sdp);
            var response = new ServerJoinRoomResponse { IsSuccess = true, Answer = answerSdp };
            return JsonSerializer.Serialize (response, PipeJsonOptions);
        }

        EasyWebRTCServer webRTCServer;

        INamedPipesRouter namedPipesRouter;

        protected override void OnClose ()
        {
            if (namedPipesRouter != null)
            {
                namedPipesRouter.UnregisterHandler (GameRoomModel.OnJoinRoomRouter);
                (namedPipesRouter as IDisposable)?.Dispose ();
                namedPipesRouter = null;
            }

            if (webRTCServer != null)
            {
                webRTCServer.Close ();
                Destroy (webRTCServer);
            }
        }
    }
}