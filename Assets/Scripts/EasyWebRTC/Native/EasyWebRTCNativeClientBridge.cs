#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public class EasyWebRTCNativeClientBridge : MonoBehaviour, IEasyWebRTCClientBridge
    {
        Coroutine updateCoroutine;
        RTCPeerConnection peerConnection;
        readonly Dictionary<string, EasyWebRTCNativeChannel> channels = new Dictionary<string, EasyWebRTCNativeChannel> ();
        bool offerStarted;
        public event Action<EasyWebRTCConnectionState> OnConnectionStateChanged;
        public EasyWebRTCConnectionState ConnectionState { get; private set; }
        public event Action<Exception> OnError;
        bool setup;
        bool closed;
        float iceGatheringTimeoutSeconds;
        public bool IsConnected => !closed && ConnectionState == EasyWebRTCConnectionState.Connected;

        public void Setup (float iceGatheringTimeoutSeconds = 3f)
        {
            if (setup || closed)
            {
                throw new InvalidOperationException ("WebRTC 橋接器已初始化或關閉。");
            }
            if (float.IsNaN (iceGatheringTimeoutSeconds) || float.IsInfinity (iceGatheringTimeoutSeconds) || iceGatheringTimeoutSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException (nameof (iceGatheringTimeoutSeconds));
            }
            this.iceGatheringTimeoutSeconds = iceGatheringTimeoutSeconds;
            setup = true;
            Debug.Log ("[WebRTC Native] 啟動 WebRTC.Update 前。", this);
            updateCoroutine = StartCoroutine (WebRTC.Update ());
            Debug.Log ("[WebRTC Native] WebRTC.Update 已啟動。", this);
            RTCConfiguration config = EasyWebRTCUtility.GetRTCConfiguration ();
            Debug.Log ("[WebRTC Native] 建立 PeerConnection 前。", this);
            peerConnection = new RTCPeerConnection (ref config);
            Debug.Log ("[WebRTC Native] PeerConnection 已建立。", this);
            peerConnection.OnConnectionStateChange = state =>
            {
                if (closed)
                {
                    return;
                }
                ConnectionState = EasyWebRTCNativeChannel.Map (state);
                foreach (var channel in channels.Values)
                {
                    channel.SetConnectionState (ConnectionState);
                }
                OnConnectionStateChanged?.Invoke (ConnectionState);
            };
            foreach (var channel in channels.Values)
            {
                AttachChannel (channel);
            }
        }

        public EasyWebRTCChannel BindChannel (string name, bool ordered, int maxRetransmits)
        {
            if (closed || offerStarted)
            {
                throw new InvalidOperationException ("Channel 必須在 CreateOffer 前綁定。");
            }
            if (channels.ContainsKey (name))
            {
                throw new ArgumentException ("Channel 名稱重複。", nameof (name));
            }
            var channel = new EasyWebRTCNativeChannel (name, ordered, maxRetransmits);
            channels.Add (name, channel);
            if (setup)
            {
                AttachChannel (channel);
            }
            return channel;
        }

        void AttachChannel (EasyWebRTCNativeChannel channel)
        {
            if (channel.State == EasyWebRTCChannelState.Closed)
            {
                return;
            }
            var options = new RTCDataChannelInit { ordered = channel.Ordered,
                maxRetransmits = channel.MaxRetransmits == -1 ? (int?)null : channel.MaxRetransmits };
            Debug.Log ($"[WebRTC Native] 建立 Channel {channel.Name} 前。", this);
            var nativeChannel = peerConnection.CreateDataChannel (channel.Name, options);
            Debug.Log ($"[WebRTC Native] Channel {channel.Name} 已建立。", this);
            channel.Attach (nativeChannel, error => OnError?.Invoke (error));
            channel.SetConnectionState (ConnectionState);
        }

        public async UniTask<string> CreateOffer (CancellationToken cancellationToken)
        {
            EnsureReady (cancellationToken);
            offerStarted = true;
            Debug.Log ("[WebRTC Native] 呼叫 CreateOffer 前。", this);
            var offerOp = peerConnection.CreateOffer ();
            Debug.Log ("[WebRTC Native] CreateOffer 已返回，等待完成。", this);
            await UniTask.WaitUntil (
                () =>
                {
                    return offerOp.IsDone;
                },
                cancellationToken: cancellationToken);
            if (offerOp.IsError)
            {
                throw new InvalidOperationException ($"建立 Offer 失敗: {offerOp.Error.message}");
            }
            Debug.Log ("[WebRTC Native] Offer 已完成。", this);
            var description = offerOp.Desc;
            Debug.Log ("[WebRTC Native] 呼叫 SetLocalDescription 前。", this);
            var localOp = peerConnection.SetLocalDescription (ref description);
            Debug.Log ("[WebRTC Native] SetLocalDescription 已返回，等待完成。", this);
            await UniTask.WaitUntil (
                () =>
                {
                    return localOp.IsDone;
                },
                cancellationToken: cancellationToken);
            if (localOp.IsError)
            {
                throw new InvalidOperationException ($"設定本地 SDP 失敗: {localOp.Error.message}");
            }
            Debug.Log ("[WebRTC Native] 本地 SDP 已完成，等待 ICE 收集。", this);
            await WaitForIceGathering (cancellationToken);
            Debug.Log ("[WebRTC Native] ICE 收集完成。", this);
            cancellationToken.ThrowIfCancellationRequested ();
            return peerConnection.LocalDescription.sdp;
        }

        public async UniTask SetAnswer (string sdp, CancellationToken cancellationToken)
        {
            EnsureReady (cancellationToken);
            var description = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = sdp };
            Debug.Log ("[WebRTC Native] 呼叫 SetRemoteDescription 前。", this);
            var remoteOp = peerConnection.SetRemoteDescription (ref description);
            Debug.Log ("[WebRTC Native] SetRemoteDescription 已返回，等待完成。", this);
            await UniTask.WaitUntil (
                () =>
                {
                    return remoteOp.IsDone;
                },
                cancellationToken: cancellationToken);
            if (remoteOp.IsError)
            {
                throw new InvalidOperationException ($"設定遠端 SDP 失敗: {remoteOp.Error.message}");
            }
            Debug.Log ("[WebRTC Native] 遠端 SDP 已完成。", this);
        }

        void EnsureReady (CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested ();
            if (!setup || closed || peerConnection == null)
            {
                throw new InvalidOperationException ("WebRTC 橋接器尚未初始化或已關閉。");
            }
        }

        async UniTask WaitForIceGathering (CancellationToken cancellationToken)
        {
            float startedAt = Time.realtimeSinceStartup;
            while (true)
            {
                EnsureReady (cancellationToken);
                if (peerConnection.GatheringState == RTCIceGatheringState.Complete)
                {
                    return;
                }
                if (Time.realtimeSinceStartup - startedAt >= iceGatheringTimeoutSeconds)
                {
                    throw new TimeoutException ("ICE 收集未完成，無法送出完整 SDP。");
                }
                await UniTask.Yield (cancellationToken);
            }
        }

        void OnDestroy ()
        {
            Close ();
        }

        public void Close ()
        {
            if (closed)
            {
                return;
            }
            closed = true;
            if (updateCoroutine != null)
            {
                StopCoroutine (updateCoroutine);
            }
            updateCoroutine = null;
            ConnectionState = EasyWebRTCConnectionState.Closed;
            foreach (var channel in channels.Values)
            {
                channel.SetConnectionState (ConnectionState);
                channel.Close ();
            }
            OnConnectionStateChanged?.Invoke (ConnectionState);
            peerConnection?.Close ();
            peerConnection?.Dispose ();
            peerConnection = null;
        }
    }
}
#endif
