using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CraneGameServer.WebApi;

namespace Kun.Tool
{
    /// <summary>管理連線流程和 Room API；平台差異交給橋接器。</summary>
    public class EasyWebRTCClient : EasyWebRTCBase
    {
        IEasyWebRTCClientBridge bridge;
        HttpClient httpClient;
        RoomClient roomClient;
        CancellationTokenSource connectionCancellation;
        readonly Dictionary<string, EasyWebRTCChannel> channels = new Dictionary<string, EasyWebRTCChannel> ();
        EasyWebRTCChannel defaultChannel;
        public IReadOnlyDictionary<string, EasyWebRTCChannel> Channels => channels;
        public event Action<EasyWebRTCConnectionState> OnConnectionStateChanged;
        public EasyWebRTCConnectionState ConnectionState => bridge?.ConnectionState ?? (IsClosed ? EasyWebRTCConnectionState.Closed : EasyWebRTCConnectionState.New);

        public EasyWebRTCChannel BindChannel (string name, bool ordered = true, int maxRetransmits = -1)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException ("Client 已關閉。");
            }
            EnsureBridge ();
            var channel = bridge.BindChannel (name, ordered, maxRetransmits);
            channels.Add (name, channel);
            channel.OnDataReceived += ReceiveData;
            if (defaultChannel == null)
            {
                defaultChannel = channel;
            }
            return channel;
        }

        void NotifyConnectionState (EasyWebRTCConnectionState state)
        {
            OnConnectionStateChanged?.Invoke (state);
        }

        void EnsureBridge ()
        {
            if (bridge != null)
            {
                return;
            }
            var host = new GameObject ("EasyWebRTCClientBridge");
            host.transform.SetParent (transform, false);
            try
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    bridge = host.AddComponent<EasyWebRTCWebGLClientBridge> ();
                }
                else
                {
#if !UNITY_WEBGL || UNITY_EDITOR
                    bridge = host.AddComponent<EasyWebRTCNativeClientBridge> ();
#else
                    throw new PlatformNotSupportedException ("此建置僅支援 WebGL 橋接器。");
#endif
                }
                bridge.OnError += HandleBridgeError;
                bridge.OnConnectionStateChanged += NotifyConnectionState;
            }
            catch
            {
                Destroy (host);
                throw;
            }
        }

        protected override bool RequiresNativeUpdate => false;
        public override bool IsConnected => !IsClosed && bridge != null && bridge.IsConnected;

        public void Setup (string ip, float iceGatheringTimeoutSeconds = 3f)
        {
            if (string.IsNullOrWhiteSpace (ip))
            {
                throw new ArgumentException ("Room API 位址不可為空。", nameof (ip));
            }
            string baseUrl = ip.Contains ("://") ? ip : $"http://{ip}";
            if (!Uri.TryCreate (baseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) || string.IsNullOrEmpty (uri.Host))
            {
                throw new ArgumentException ("Room API 位址必須是 HTTP(S) URL。", nameof (ip));
            }

            base.Setup (iceGatheringTimeoutSeconds);
            try
            {
                httpClient = new HttpClient ();
                roomClient = new RoomClient (uri.AbsoluteUri, httpClient);
                EnsureBridge ();
                if (channels.Count == 0)
                {
                    BindChannel ("rudp-channel");
                }
                bridge.Setup (iceGatheringTimeoutSeconds);
                connectionCancellation = new CancellationTokenSource ();
                ConnectToTargetServer (connectionCancellation.Token).Forget ();
            }
            catch
            {
                Close ();
                throw;
            }
        }

        async UniTask ConnectToTargetServer (CancellationToken cancellationToken)
        {
            try
            {
                string offer = await bridge.CreateOffer (cancellationToken);
                cancellationToken.ThrowIfCancellationRequested ();
                var request = new ClientJoinRoomRequest (offer);
                ServerJoinRoomResponse response = await roomClient.JoinRoomWithDiagnosticsAsync (request, cancellationToken);
                await UniTask.SwitchToMainThread (cancellationToken);
                cancellationToken.ThrowIfCancellationRequested ();
                if (response == null || !response.IsSuccess || string.IsNullOrWhiteSpace (response.Answer))
                {
                    throw new InvalidOperationException ("Room API 加入房間失敗，或未回傳 Answer SDP。");
                }
                await bridge.SetAnswer (response.Answer, cancellationToken);
                Debug.Log ("[Client] Room API 的 Answer SDP 已套用，等待 ICE 建立連線。");
            }
            catch (OperationCanceledException) when (IsClosed)
            {
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread ();
                if (!IsClosed)
                {
                    string message = RoomClient.FormatRequestException (ex);
                    Debug.LogError (message, this);
                    Close ();
                }
            }
        }

        void HandleBridgeError (Exception exception)
        {
            if (IsClosed)
            {
                return;
            }
            Debug.LogException (exception, this);
            Close ();
        }

        public override bool Send (byte[] data)
        {
            base.Send (data);
            return !IsClosed && defaultChannel != null && defaultChannel.Send (data);
        }

        public override void Close ()
        {
            if (IsClosed)
            {
                return;
            }
            base.Close ();
            connectionCancellation?.Cancel ();
            connectionCancellation?.Dispose ();
            connectionCancellation = null;
            httpClient?.Dispose ();
            httpClient = null;
            roomClient = null;
            if (bridge == null)
            {
                return;
            }
            foreach (var channel in channels.Values)
            {
                channel.OnDataReceived -= ReceiveData;
            }
            bridge.OnError -= HandleBridgeError;
            bridge.Close ();
            bridge.OnConnectionStateChanged -= NotifyConnectionState;
            if (bridge is Component component)
            {
                Destroy (component.gameObject);
            }
            bridge = null;
        }
    }
}
