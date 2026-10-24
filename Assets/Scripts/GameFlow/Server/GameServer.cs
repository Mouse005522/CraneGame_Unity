using System;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
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
            webRTCServer.OnChannelBound += HandleChannelBound;
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

        readonly List<Action> channelLogCleanup = new List<Action> ();

        void HandleChannelBound (EasyWebRTCChannel channel)
        {
            Debug.Log ($"[Server] Channel {channel.Name} 已建立，狀態 = {channel.State}，PeerConnection = {channel.ConnectionState}", this);

            Action<EasyWebRTCChannelState> stateChanged = state =>
            {
                Debug.Log ($"[Server] Channel {channel.Name} 狀態變更：{state}", this);
            };
            Action<EasyWebRTCConnectionState> connectionStateChanged = state =>
            {
                Debug.Log ($"[Server] Channel {channel.Name} PeerConnection 狀態變更：{state}", this);
            };
            EasyWebRTCDataReceived dataReceived = bytes =>
            {
                string message = Encoding.UTF8.GetString (bytes);
                Debug.Log ($"[Server] Channel {channel.Name} 收到：{message}", this);
            };

            channel.OnStateChanged += stateChanged;
            channel.OnConnectionStateChanged += connectionStateChanged;
            channel.OnDataReceived += dataReceived;

            channelLogCleanup.Add (() =>
            {
                channel.OnStateChanged -= stateChanged;
                channel.OnConnectionStateChanged -= connectionStateChanged;
                channel.OnDataReceived -= dataReceived;
            });
        }

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
                webRTCServer.OnChannelBound -= HandleChannelBound;
                // 先關閉，讓已訂閱的事件記錄最後的 Closed 狀態。
                webRTCServer.Close ();
                Destroy (webRTCServer);
            }

            foreach (var cleanup in channelLogCleanup)
            {
                cleanup ();
            }
            channelLogCleanup.Clear ();
        }
    }
}
