using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public class EasyWebRTCWebGLClientBridge : MonoBehaviour, IEasyWebRTCClientBridge
    {
        [DllImport ("__Internal")] static extern void EasyRTC_BindChannel (string receiver, int id, string name, bool ordered, int maxRetransmits);
        [DllImport ("__Internal")] static extern void EasyRTC_CreateOffer (string receiver, float timeout);
        [DllImport ("__Internal")] static extern void EasyRTC_SetAnswer (string receiver, string sdp);
        [DllImport ("__Internal")] static extern void EasyRTC_Close (string receiver);

        readonly Dictionary<int, EasyWebRTCWebGLChannel> channels = new Dictionary<int, EasyWebRTCWebGLChannel> ();
        readonly HashSet<string> names = new HashSet<string> ();
        EasyWebRTCBrowserBridge receiver;
        UniTaskCompletionSource<string> pendingOffer;
        UniTaskCompletionSource pendingAnswer;
        public event Action<Exception> OnError;
        public event Action<EasyWebRTCConnectionState> OnConnectionStateChanged;
        public EasyWebRTCConnectionState ConnectionState { get; private set; }
        bool setup;
        bool closed;
        bool offerStarted;
        float iceGatheringTimeoutSeconds;
        public bool IsConnected => !closed && ConnectionState == EasyWebRTCConnectionState.Connected;

        public EasyWebRTCChannel BindChannel (string name, bool ordered, int maxRetransmits)
        {
            if (closed || offerStarted)
            {
                throw new InvalidOperationException ("Channel 必須在 CreateOffer 前綁定。");
            }
            var channel = new EasyWebRTCWebGLChannel (channels.Count, name, ordered, maxRetransmits);
            if (!names.Add (name))
            {
                throw new ArgumentException ("Channel 名稱重複。", nameof (name));
            }
            channels.Add (channel.Id, channel);
            if (setup)
            {
                AttachChannel (channel);
            }
            return channel;
        }

        void AttachChannel (EasyWebRTCWebGLChannel channel)
        {
            if (channel.State == EasyWebRTCChannelState.Closed)
            {
                return;
            }
            channel.Attach (receiver.name);
            EasyRTC_BindChannel (receiver.name, channel.Id, channel.Name, channel.Ordered, channel.MaxRetransmits);
            channel.SetConnectionState (ConnectionState);
        }

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
            var host = new GameObject ("EasyRTC_" + Guid.NewGuid ().ToString ("N"));
            host.transform.SetParent (transform, false);
            receiver = host.AddComponent<EasyWebRTCBrowserBridge> ();
            receiver.ChannelData = ReceiveChannelData;
            receiver.ChannelState = ReceiveChannelState;
            receiver.PeerState = SetConnectionState;
            receiver.OfferCreated = sdp =>
            {
                pendingOffer?.TrySetResult (sdp);
            };
            receiver.AnswerApplied = () =>
            {
                pendingAnswer?.TrySetResult ();
            };
            receiver.Failed = message =>
            {
                var error = new InvalidOperationException (message);
                if (pendingOffer != null)
                {
                    pendingOffer.TrySetException (error);
                }
                else if (pendingAnswer != null)
                {
                    pendingAnswer.TrySetException (error);
                }
                else
                {
                    OnError?.Invoke (error);
                }
            };
            receiver.Initialize ();
            foreach (var channel in channels.Values)
            {
                AttachChannel (channel);
            }
        }

        void SetConnectionState (EasyWebRTCConnectionState state)
        {
            if (ConnectionState == state)
            {
                return;
            }
            ConnectionState = state;
            foreach (var channel in channels.Values)
            {
                channel.SetConnectionState (state);
            }
            OnConnectionStateChanged?.Invoke (state);
        }

        unsafe void ReceiveChannelData (int channelId, IntPtr pointer, int length)
        {
            if (closed)
            {
                return;
            }
            if (length < 0 || (pointer == IntPtr.Zero && length != 0))
            {
                throw new ArgumentException ("WebRTC callback buffer 無效。");
            }
            if (channels.TryGetValue (channelId, out var channel))
            {
                if (channel.State == EasyWebRTCChannelState.Closed)
                {
                    return;
                }
                // 直接映射 JS buffer，不配置陣列；Span 僅在同步事件期間有效。
                var bytes = new ReadOnlySpan<byte> (pointer.ToPointer (), length);
                channel.Receive (bytes);
            }
            else
            {
                Debug.LogError ($"找不到 WebRTC channel id = {channelId}。");
            }
        }

        void ReceiveChannelState (int channelId, EasyWebRTCChannelState state)
        {
            if (closed)
            {
                return;
            }
            if (channels.TryGetValue (channelId, out var channel))
            {
                channel.SetState (state);
            }
            else
            {
                Debug.LogError ($"找不到 WebRTC channel id = {channelId}。");
            }
        }

        public async UniTask<string> CreateOffer (CancellationToken cancellationToken)
        {
            EnsureReady (cancellationToken);
            offerStarted = true;
            pendingOffer = new UniTaskCompletionSource<string> ();
            try
            {
                EasyRTC_CreateOffer (receiver.name, iceGatheringTimeoutSeconds);
                return await pendingOffer.Task.AttachExternalCancellation (cancellationToken);
            }
            finally
            {
                pendingOffer = null;
            }
        }

        public async UniTask SetAnswer (string sdp, CancellationToken cancellationToken)
        {
            EnsureReady (cancellationToken);
            pendingAnswer = new UniTaskCompletionSource ();
            try
            {
                EasyRTC_SetAnswer (receiver.name, sdp);
                await pendingAnswer.Task.AttachExternalCancellation (cancellationToken);
            }
            finally
            {
                pendingAnswer = null;
            }
        }

        void EnsureReady (CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested ();
            if (!setup || closed || receiver == null)
            {
                throw new InvalidOperationException ("WebRTC 橋接器尚未初始化或已關閉。");
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
            pendingOffer?.TrySetCanceled ();
            pendingAnswer?.TrySetCanceled ();
            SetConnectionState (EasyWebRTCConnectionState.Closed);
            foreach (var channel in channels.Values)
            {
                channel.Close ();
            }
            if (receiver == null)
            {
                return;
            }
            EasyRTC_Close (receiver.name);
            receiver.Release ();
            receiver.ChannelData = null;
            receiver.ChannelState = null;
            receiver.PeerState = null;
            receiver.Failed = null;
            receiver.OfferCreated = null;
            receiver.AnswerApplied = null;
            Destroy (receiver.gameObject);
            receiver = null;
        }
    }
}
